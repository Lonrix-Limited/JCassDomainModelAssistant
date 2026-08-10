using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using JcassDm.Bundle;
using JcassDm.Cli;
using JcassDm.Project;

namespace JcassDm.Verbs;

/// <summary>
/// <c>jcass-dm scaffold &lt;Name&gt; [--output &lt;path&gt;] [--element &lt;Noun&gt;]
/// [--namespace &lt;ns&gt;] [--from-sample]</c>
///
/// <para>Emits a complete domain model project whose four names cannot disagree.</para>
///
/// <para><b>This verb exists because renaming is the way models break.</b> Four strings have to
/// match - the <c>.csproj</c> filename, the assembly name it implies, the entry class, and
/// <c>meta.main_dll</c> / <c>main_class</c> in the bundle - and a normal run and a debug F5 run
/// read them by different routes, so a mismatch looks fine until F5 says "Domain Model class 'X'
/// was not found in the specified .dll". A generator that takes ONE name and writes all four
/// cannot produce that. There is deliberately no argument that sets any of the four
/// independently, and adding one would undo the whole point of the verb.</para>
///
/// <para><b>What it emits is the canonical skeleton, not a bare entry class</b> - one file per
/// stage of the framework's per-period loop, matching what every non-restricted model in the
/// Cassandra corpus converged on. The stubs carry structure and comments and no modelling
/// numbers: where a threshold belongs there is a note saying so and pointing at
/// <c>lookups.xlsx</c> and at the <c>Constants</c> property that should read it. A plausible
/// default is worse than a hole, because a hole gets filled and a default gets shipped.</para>
///
/// <para><b><c>--from-sample</c> is the walking skeleton.</b> Same file set, carrying the
/// reference model's working logic, so the engineer's first artefact runs end to end against the
/// sample inputs. They prove the whole pipeline - build, upload, debug, publish, run - on the
/// artefact they will keep, then replace sample logic with their own file by file. No throwaway
/// project and no rename later.</para>
/// </summary>
internal static class ScaffoldVerb
{
    public static int Run(ArgumentSet args, TextWriter output)
    {
        ModelName model = ReadName(args);

        string? outputOption = args.Optional("--output");
        string elementName = args.Optional("--element") ?? Skeleton.DefaultElementName;
        string? namespaceOption = args.Optional("--namespace");
        bool fromSample = args.Flag("--from-sample");
        args.CheckForUnknownOptions();

        ModelName element = ModelName.Parse(elementName, "element class name");
        if (string.Equals(element.Value, model.Value, StringComparison.Ordinal))
        {
            throw new UsageFailure(
                $"--element is '{element}', the same as the model name. They would end up as two " +
                "classes with one name. Give the element a noun from your domain, e.g. --element RoadSegment.");
        }

        string namespaceName = namespaceOption is null
            ? model.DefaultNamespace
            : RequireNamespace(namespaceOption);

        string folder = Path.GetFullPath(outputOption ?? model.Value);
        RequireEmptyTarget(folder, model);
        RequireWritableTarget(folder);

        string variant = fromSample ? "sample" : "skeleton";

        // Everything is written under a folder that did not exist a moment ago, so there is no
        // half-overwritten state to unwind. The one thing worth guarding is a failure part-way
        // through leaving a folder that looks like a model and is not.
        bool created = false;
        try
        {
            Directory.CreateDirectory(folder);
            created = true;

            WriteProjectFiles(folder, model, element.Value, namespaceName, variant, output);
            WriteBundle(folder, model, fromSample, output);
            string? refsNote = SeedRefs(folder);

            output.WriteLine();
            WriteSummary(output, model, element.Value, folder, fromSample, refsNote);
            return ExitCode.Ok;
        }
        catch
        {
            if (created)
            {
                try { Directory.Delete(folder, recursive: true); }
                catch (IOException) { /* the failure that matters is the one being rethrown */ }
            }
            throw;
        }
    }

    private static ModelName ReadName(ArgumentSet args)
    {
        IReadOnlyList<string> positionals = args.Positionals;
        if (positionals.Count == 0)
        {
            throw new UsageFailure(
                "scaffold needs a name: jcass-dm scaffold MyRoadModel" + Environment.NewLine +
                "That one name becomes the .csproj filename, the assembly name, the entry class " +
                "and both meta settings in the bundle. It is the only name you give it.");
        }
        if (positionals.Count > 1)
        {
            throw new UsageFailure(
                "scaffold takes one name, but got " + positionals.Count + ": " +
                string.Join(", ", positionals.Select(p => $"'{p}'")) + "." + Environment.NewLine +
                "A model has exactly one name. Use --output to say where the folder goes.");
        }

        return ModelName.Parse(positionals[0]);
    }

    private static string RequireNamespace(string value)
    {
        foreach (string segment in value.Split('.'))
        {
            ModelName.Parse(segment, "namespace segment");
        }
        return value;
    }

    private static void RequireEmptyTarget(string folder, ModelName model)
    {
        if (File.Exists(folder))
        {
            throw new UsageFailure($"'{folder}' is a file, not a folder.");
        }
        if (!Directory.Exists(folder)) return;

        bool empty = Directory.EnumerateFileSystemEntries(folder).FirstOrDefault() is null;
        if (empty) return;

        throw new UsageFailure(
            $"'{folder}' already exists and is not empty. Nothing was written." + Environment.NewLine +
            Environment.NewLine +
            "scaffold never writes into an existing model - overwriting somebody's work is not " +
            "something a --force flag should make easy. If you meant to rename a model you already " +
            $"have, that is a different job:  jcass-dm rename {model} --project {folder}" +
            Environment.NewLine +
            "Otherwise pick another folder with --output.");
    }

    /// <summary>
    /// Fails clearly, before anything is written, when the target folder cannot be written to.
    ///
    /// <para><b>Without this the message is actively misleading.</b> The first write throws
    /// <see cref="UnauthorizedAccessException"/>, which reaches the top-level handler in
    /// <c>Program</c> and is reported as "jcass-dm failed unexpectedly. This is a bug in the tool"
    /// with a stack trace under it. It is not a bug: it is an engineer running scaffold against a
    /// folder their Windows account cannot write to - a managed corporate drive, a synced folder
    /// that is read-only, Program Files, the root of a drive. Reported as a tool defect it becomes
    /// a support email about something they could fix in ten seconds by choosing another
    /// folder.</para>
    ///
    /// <para>The probe is written into the nearest <i>existing</i> ancestor, because that is the
    /// folder <see cref="Directory.CreateDirectory(string)"/> will actually write into when the
    /// target does not exist yet.</para>
    /// </summary>
    private static void RequireWritableTarget(string folder)
    {
        string ancestor = NearestExistingAncestor(folder);
        string probe = Path.Combine(ancestor, ".jcass-dm-write-probe-" + Guid.NewGuid().ToString("N"));

        try
        {
            File.WriteAllText(probe, string.Empty);
            File.Delete(probe);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or SecurityException)
        {
            throw new UsageFailure(
                $"Cannot create files in '{ancestor}'. Nothing was written." + Environment.NewLine +
                Environment.NewLine +
                "Your Windows account does not have permission to write there. This is not a " +
                "problem with jcass-dm and not a problem with your model." + Environment.NewLine +
                Environment.NewLine +
                "Choose a folder you own - one under your Documents folder is always safe - and " +
                "run scaffold again with --output pointing into it. Put the model folder beside " +
                "the Assistant folder, not inside it." + Environment.NewLine +
                Environment.NewLine +
                "Underlying error: " + ex.Message);
        }
    }

    /// <summary>
    /// The closest folder up the chain from <paramref name="folder"/> that exists today.
    /// For <c>C:\Work\New\MyModel</c> where only <c>C:\Work</c> exists, that is <c>C:\Work</c>.
    /// </summary>
    private static string NearestExistingAncestor(string folder)
    {
        string? candidate = folder;
        while (candidate is not null)
        {
            if (Directory.Exists(candidate)) return candidate;
            candidate = Path.GetDirectoryName(candidate);
        }

        throw new UsageFailure(
            $"'{folder}' is not on any drive this machine can see. Nothing was written." +
            Environment.NewLine +
            "Check the drive letter, and check that a network drive is connected.");
    }

    private static void WriteProjectFiles(
        string folder, ModelName model, string elementName, string namespaceName, string variant, TextWriter output)
    {
        string sourceFolder = Path.Combine(folder, Skeleton.SourceFolder);
        Directory.CreateDirectory(sourceFolder);

        foreach (SkeletonFile file in Skeleton.Files(model, elementName))
        {
            string from = file.Shared ? "shared" : variant;
            string text = Skeleton.Render(from, file.TemplateName, model, elementName, namespaceName);
            File.WriteAllText(Path.Combine(sourceFolder, file.FileName), text);
        }

        File.WriteAllText(
            Path.Combine(folder, model.ProjectFileName),
            Skeleton.Render("shared", "Project.csproj", model, elementName, namespaceName));

        File.WriteAllText(
            Path.Combine(folder, ".gitignore"),
            Skeleton.Render("shared", "gitignore", model, elementName, namespaceName));

        File.WriteAllText(
            Path.Combine(folder, "README.md"),
            Skeleton.Render(variant, "README.md", model, elementName, namespaceName));

        output.WriteLine($"created    {model.ProjectFileName}");
        output.WriteLine($"created    {Skeleton.SourceFolder}\\ - {Skeleton.Files(model, elementName).Count} files");
        output.WriteLine("created    README.md, .gitignore");
    }

    private static void WriteBundle(string folder, ModelName model, bool fromSample, TextWriter output)
    {
        string bundlePath = Path.Combine(folder, ModelProject.BundleFileName);

        if (!fromSample)
        {
            BundleCreator.Create(bundlePath, model.MainDll, model.MainClass, model.Value);
            output.WriteLine($"created    {ModelProject.BundleFileName} - five sheets, meta filled in, no data rows");
            return;
        }

        string? sample = AssistantLayout.FindSampleBundle();
        if (sample is null)
        {
            throw new UsageFailure(
                "--from-sample needs the reference model's bundle, and jcass-dm cannot find it." +
                Environment.NewLine +
                "It looks for reference-model/DomainModelSample/" + ModelProject.BundleFileName +
                " above the folder holding jcass-dm.exe, so this usually means the exe was copied " +
                "out of the Assistant on its own. Run it from the Assistant folder, or scaffold " +
                "without --from-sample.");
        }

        File.Copy(sample, bundlePath);

        // The copied bundle still names the reference model. Rewrite the three meta rows through
        // the ordinary write path so the same conflict and atomicity rules apply, then this
        // model's bundle agrees with its own four names rather than with somebody else's.
        using BundleFile bundle = BundleFile.Open(bundlePath);
        bundle.RequireWellFormed();

        var plans = new List<RowPlan>
        {
            MetaPlan(bundle, MetaKeys.MainDll, model.MainDll),
            MetaPlan(bundle, MetaKeys.MainClass, model.MainClass),
            MetaPlan(bundle, MetaKeys.ModelName, model.Value),
        };

        BundleWriter.Apply(bundle, plans, force: true, output: TextWriter.Null);
        output.WriteLine(
            $"created    {ModelProject.BundleFileName} - copied from the reference model, meta set to {model}");
    }

    private static RowPlan MetaPlan(BundleFile bundle, string key, string value)
        => BundleWriter.Plan(bundle, SheetSpec.Meta, key,
            new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["Setting"] = key,
                ["Value"] = value,
            });

    /// <summary>
    /// Copies the framework reference assemblies in beside the project.
    ///
    /// <para>They have to be a copy in the project's own folder rather than a shared one further
    /// up: the <c>.csproj</c> reference is <c>refs\*.dll</c> relative to the project, and it has
    /// to stay that way because the web debug workspace stages its own framework assemblies into
    /// exactly that folder. A project pointing at <c>..\..\refs</c> would build locally and break
    /// the moment it was uploaded.</para>
    /// </summary>
    /// <returns>A line to print about what happened, or null when it simply worked.</returns>
    private static string? SeedRefs(string folder)
    {
        string target = Path.Combine(folder, "refs");
        Directory.CreateDirectory(target);

        string? source = AssistantLayout.FindRefsFolder();
        if (source is null)
        {
            File.WriteAllText(Path.Combine(target, "README.md"), RefsPlaceholder);
            return
                "refs\\ is EMPTY. jcass-dm could not find the Assistant's refs/ folder above its own " +
                "location," + Environment.NewLine +
                "           which usually means jcass-dm.exe was copied out on its own. The project " +
                "will not build" + Environment.NewLine +
                "           until framework reference assemblies are in refs\\. Copy them from the " +
                "Assistant's refs/.";
        }

        int copied = 0;
        foreach (string file in Directory.GetFiles(source))
        {
            string name = Path.GetFileName(file);
            if (name.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase)) continue;

            File.Copy(file, Path.Combine(target, name), overwrite: true);
            copied++;
        }

        return copied == 0
            ? "refs\\ is EMPTY - the Assistant's refs/ folder had nothing in it. The project will not build."
            : null;
    }

    private static void WriteSummary(
        TextWriter output, ModelName model, string elementName, string folder, bool fromSample, string? refsNote)
    {
        output.WriteLine($"Scaffolded {model} at {folder}");
        output.WriteLine();
        output.WriteLine("The four names all read '" + model + "', and they were all written from that one name:");
        output.WriteLine($"  1. {model.ProjectFileName}");
        output.WriteLine($"  2. assembly {model.AssemblyName}   (inherited - <AssemblyName> is deliberately unset)");
        output.WriteLine($"  3. class {model.ClassName} : DomainModelBase");
        output.WriteLine($"  4. meta.main_dll = {model.MainDll}, meta.main_class = {model.MainClass}");
        output.WriteLine();
        output.WriteLine("To change the name later, use  jcass-dm rename  rather than editing the four by hand.");

        if (refsNote is not null)
        {
            output.WriteLine();
            output.WriteLine("warning    " + refsNote);
        }

        output.WriteLine();
        output.WriteLine("Next:");
        output.WriteLine($"  dotnet build \"{Path.Combine(folder, model.ProjectFileName)}\" -c Debug --no-incremental");
        output.WriteLine($"  jcass-dm check --project \"{folder}\"");
        output.WriteLine();

        if (fromSample)
        {
            output.WriteLine("This is the walking skeleton: it carries the reference model's working logic, so it");
            output.WriteLine("runs end to end against the sample inputs before you have written anything. Prove the");
            output.WriteLine("whole pipeline on it - build, upload, F5, publish, run - and then replace the sample's");
            output.WriteLine("engineering with your own, one file at a time. This is the model you keep; there is no");
            output.WriteLine("throwaway project and no rename later.");
            output.WriteLine();
            output.WriteLine("It needs these lookup sets in the client's inputs\\lookups.xlsx, and fails at setup");
            output.WriteLine("naming any one that is missing:");
            output.WriteLine("  repair_thresholds, replace_thresholds, maintenance_thresholds,");
            output.WriteLine("  deterioration_rates, replacement_rates, rate_factors, unit_rates");
        }
        else
        {
            output.WriteLine("The stubs build and load, and forecast nothing: no input columns, no parameters, no");
            output.WriteLine("treatments. Every hole in them is a decision that is yours, and none has been filled");
            output.WriteLine("with a plausible-looking guess. README.md in the project says where to start.");
            output.WriteLine();
            output.WriteLine($"The element class is {elementName}. Rename it freely - unlike the four above, it is an");
            output.WriteLine("ordinary class name that nothing outside the project resolves by string.");
        }
    }

    private const string RefsPlaceholder = """
        # refs

        **This folder is empty and the project will not build until it is not.**

        It should hold the Juno Cassandra framework *reference assemblies* - the full public API
        with no method bodies. They are enough for the compiler and for IntelliSense, and
        deliberately not enough for the runtime: you author locally and you run and debug on the
        web app's Debug Model page.

        `jcass-dm scaffold` normally copies them in for you. It could not this time, because it
        could not find the Domain Model Assistant's own `refs/` folder above the location of
        `jcass-dm.exe` - which usually means the exe was copied out of the Assistant on its own.

        To fix it, copy every `.dll` and `.xml` from the Assistant's `refs/` folder into this one.
        Do not mix assemblies from two framework releases: the reference in the `.csproj` is a
        wildcard, so a leftover gets compiled against rather than ignored.
        """;
}
