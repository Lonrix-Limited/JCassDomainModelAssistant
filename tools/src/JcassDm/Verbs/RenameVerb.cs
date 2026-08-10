using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JcassDm.Bundle;
using JcassDm.Cli;
using JcassDm.Project;

namespace JcassDm.Verbs;

/// <summary>
/// <c>jcass-dm rename &lt;NewName&gt; [--project &lt;path&gt;] [--namespace]</c>
///
/// <para>Changes all four names on a model that already exists: the <c>.csproj</c> filename, the
/// entry class and the file holding it, and <c>meta.main_dll</c> / <c>meta.main_class</c> in the
/// bundle. The assembly name follows the <c>.csproj</c> filename, which is why
/// <c>&lt;AssemblyName&gt;</c> must stay unset.</para>
///
/// <para><b>This is scaffold's counterpart, and it exists for the takeover case.</b> Not every
/// engineer starts fresh: inheriting a model that already runs for a client is a first-class
/// route in, and an inherited model may already break the four-name rule. The framework's own
/// unit-test domain model does - its project is <c>JCassUnitTestDomainModel</c> and its class is
/// <c>UnitTestDomainModel</c> - and a debug F5 run against it fails for exactly that reason.
/// Fixing it by hand is four edits in four file formats, which is the error-prone manual
/// operation this tool exists to remove.</para>
///
/// <para><b>All or nothing.</b> Every file that will change is backed up before anything is
/// written, and any failure restores the lot. A half-renamed model is worse than the problem it
/// was meant to fix: the engineer no longer knows which state they are in, and neither does the
/// agent helping them.</para>
///
/// <para><b>The namespace is not one of the four</b> and is left alone unless
/// <c>--namespace</c> is passed. Nothing resolves a domain model by namespace - a project can
/// carry a completely unrelated one and run perfectly. It is only ever confusing, never broken,
/// which is why it is opt-in and reported separately.</para>
/// </summary>
internal static class RenameVerb
{
    public static int Run(ArgumentSet args, TextWriter output)
    {
        ModelName target = ReadName(args);
        string projectPath = args.Optional("--project") ?? ".";
        bool renameNamespace = args.Flag("--namespace");
        args.CheckForUnknownOptions();

        ModelProject project = ModelProject.Open(projectPath);
        RenamePlan plan = RenamePlan.Build(project, target, renameNamespace);

        if (plan.IsNoOp)
        {
            output.WriteLine($"unchanged  All four names already read '{target}'. Nothing written.");
            return ExitCode.Ok;
        }

        plan.Describe(output);
        plan.Apply(output);

        output.WriteLine();
        output.WriteLine($"Renamed to {target}. The four names now agree:");
        output.WriteLine($"  1. {target.ProjectFileName}");
        output.WriteLine($"  2. assembly {target.AssemblyName}   (inherited from the filename)");
        output.WriteLine($"  3. class {target.ClassName} : DomainModelBase");
        output.WriteLine($"  4. meta.main_dll = {target.MainDll}, meta.main_class = {target.MainClass}");
        output.WriteLine();
        output.WriteLine("Rebuild before you do anything else - a rename is a source change like any other:");
        output.WriteLine($"  dotnet build \"{Path.Combine(project.Folder, target.ProjectFileName)}\" -c Debug --no-incremental");

        if (!renameNamespace && plan.NamespacesThatCouldMove.Count > 0)
        {
            output.WriteLine();
            output.WriteLine("note       The namespace was left alone, and that is not a problem:");
            foreach (string ns in plan.NamespacesThatCouldMove)
            {
                output.WriteLine($"             {ns}");
            }
            output.WriteLine("           The namespace is NOT one of the four names. Nothing resolves a domain model");
            output.WriteLine("           by it, and a model whose namespace bears no relation to its name runs");
            output.WriteLine("           perfectly. A stale one is only confusing to read. Pass --namespace if you");
            output.WriteLine("           want it moved too.");
        }

        return ExitCode.Ok;
    }

    private static ModelName ReadName(ArgumentSet args)
    {
        IReadOnlyList<string> positionals = args.Positionals;
        if (positionals.Count == 0)
        {
            throw new UsageFailure(
                "rename needs the new name: jcass-dm rename MyRoadModel --project ..\\MyRoadModel");
        }
        if (positionals.Count > 1)
        {
            throw new UsageFailure(
                "rename takes one new name, but got " + positionals.Count + ": " +
                string.Join(", ", positionals.Select(p => $"'{p}'")) + "." + Environment.NewLine +
                "Use --project for the folder.");
        }

        return ModelName.Parse(positionals[0], "new name");
    }
}

/// <summary>
/// Everything a rename will do, worked out before any of it happens.
///
/// <para>Separating the decision from the act is what makes "all or nothing" achievable: every
/// refusal - a target file already there, no entry class to rename, a bundle that cannot be
/// written - happens while the model is still untouched.</para>
/// </summary>
internal sealed class RenamePlan
{
    private readonly ModelProject _project;
    private readonly ModelName _target;
    private readonly List<FileEdit> _edits = new();
    private readonly List<FileMove> _moves = new();
    private readonly string? _oldClassName;

    private RenamePlan(ModelProject project, ModelName target, string? oldClassName)
    {
        this._project = project;
        this._target = target;
        this._oldClassName = oldClassName;
    }

    /// <summary>Declared namespaces that <c>--namespace</c> would have moved, when it was not passed.</summary>
    public IReadOnlyList<string> NamespacesThatCouldMove { get; private set; } = Array.Empty<string>();

    /// <summary>True when all four names already read the target and there is nothing to do.</summary>
    public bool IsNoOp => this._edits.Count == 0 && this._moves.Count == 0 && !this.BundleNeedsWriting;

    /// <summary>True when either meta row disagrees with the target.</summary>
    public bool BundleNeedsWriting { get; private set; }

    /// <summary>
    /// Works out the whole rename, refusing anything that cannot be completed.
    /// </summary>
    /// <param name="project">The model to rename.</param>
    /// <param name="target">The new name.</param>
    /// <param name="renameNamespace">Whether to move namespaces starting with the old name too.</param>
    public static RenamePlan Build(ModelProject project, ModelName target, bool renameNamespace)
    {
        EntryClass? entry = project.FindEntryClass();
        IReadOnlyList<string> allEntryClasses = project.AllEntryClassNames();

        if (allEntryClasses.Count > 1)
        {
            throw new UsageFailure(
                $"{allEntryClasses.Count} classes in this project derive from DomainModelBase: " +
                string.Join(", ", allEntryClasses) + "." + Environment.NewLine +
                "The framework loads exactly one, named in the bundle, and jcass-dm will not guess " +
                "which of these you meant. Delete the ones that are not the entry class, then re-run.");
        }
        if (entry is null)
        {
            throw new UsageFailure(
                $"No class deriving from DomainModelBase found under {project.Folder}." + Environment.NewLine +
                "rename changes the entry class as one of the four names, so it needs to find it first. " +
                "If this is a model mid-edit, fix the entry class and re-run; " +
                "run  jcass-dm check --project <path>  for the fuller picture.");
        }

        var plan = new RenamePlan(project, target, entry.Name);

        plan.PlanSourceEdits(entry, renameNamespace);
        plan.PlanFileMoves(entry);
        plan.PlanBundle();

        return plan;
    }

    /// <summary>Prints what is about to happen, before it does.</summary>
    public void Describe(TextWriter output)
    {
        foreach (FileMove move in this._moves)
        {
            output.WriteLine($"rename     {move.RelativeFrom} -> {move.RelativeTo}");
        }
        if (this._oldClassName is not null && !string.Equals(this._oldClassName, this._target.ClassName, StringComparison.Ordinal))
        {
            output.WriteLine($"class      {this._oldClassName} -> {this._target.ClassName}" +
                             $"  (in {this._edits.Count} file{(this._edits.Count == 1 ? "" : "s")})");
        }
        else if (this._edits.Count > 0)
        {
            output.WriteLine($"edited     {this._edits.Count} source file{(this._edits.Count == 1 ? "" : "s")}");
        }
        if (this.BundleNeedsWriting)
        {
            output.WriteLine($"bundle     main_dll -> {this._target.MainDll}, main_class -> {this._target.MainClass}");
        }
    }

    /// <summary>
    /// Applies the whole plan, restoring everything on any failure.
    ///
    /// <para>The backup is a copy of every file that will be written or moved, taken before the
    /// first write. Restoring means putting those back and deleting anything that appeared - which
    /// covers the case that matters most, a failure between renaming the <c>.csproj</c> and
    /// writing the bundle, where the model would otherwise be left with three names agreeing and
    /// one not.</para>
    /// </summary>
    public void Apply(TextWriter output)
    {
        using var backup = new RenameBackup();

        foreach (FileEdit edit in this._edits) backup.Save(edit.Path);
        foreach (FileMove move in this._moves) backup.Save(move.From);
        if (this.BundleNeedsWriting) backup.Save(this._project.BundlePath);

        try
        {
            foreach (FileEdit edit in this._edits)
            {
                File.WriteAllText(edit.Path, edit.NewText);
            }

            foreach (FileMove move in this._moves)
            {
                File.Move(move.From, move.To);
                backup.RecordCreated(move.To);
            }

            if (this.BundleNeedsWriting)
            {
                this.WriteBundle();
            }
        }
        catch (Exception ex)
        {
            backup.Restore();

            throw new BundleFailure(
                "The rename failed part-way through and the model has been put back exactly as it was." +
                Environment.NewLine +
                $"Details: {ex.Message}" + Environment.NewLine + Environment.NewLine +
                "Nothing is half-renamed. The usual cause is a file open in another program - " +
                "Visual Studio, Excel, or a build running against the project.");
        }

        backup.Commit();
    }

    private void PlanSourceEdits(EntryClass entry, bool renameNamespace)
    {
        bool classChanges = !string.Equals(entry.Name, this._target.ClassName, StringComparison.Ordinal);

        // Occurrences of the old name followed by a dot are namespace-qualified when a namespace
        // of that name exists, and must be left to the namespace pass. Without this test a
        // fully-qualified `OldName.Objects.Thing` would be half-rewritten.
        bool oldNameIsNamespacePrefix = this._project.Namespaces()
            .Any(ns => string.Equals(ns, entry.Name, StringComparison.Ordinal)
                       || ns.StartsWith(entry.Name + ".", StringComparison.Ordinal));

        // Every declared namespace whose leading segment is not already the target is one that
        // --namespace would move.
        //
        // This used to be anchored on the ENTRY CLASS NAME, and that silently did nothing in the
        // case that matters most. `05-adopt-an-existing-model.md` tells the engineer to run
        // `rename <Name>` first; the verb's own success note then says "pass --namespace if you
        // want it moved too". By that point the class name already reads the target, so a stale
        // namespace matched nothing, the plan came out empty, and the verb reported "unchanged"
        // and exited 0 - having done exactly nothing the engineer asked for. A stale namespace is
        // the adoption case, which is the one time somebody actually reaches for this flag.
        List<string> movableRoots = this._project.Namespaces()
            .Select(RootSegment)
            .Where(root => !string.Equals(root, this._target.Value, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var movable = this._project.Namespaces()
            .Where(ns => movableRoots.Contains(RootSegment(ns), StringComparer.Ordinal))
            .ToList();

        if (!renameNamespace) this.NamespacesThatCouldMove = movable;

        foreach (SourceFile source in this._project.Sources)
        {
            string text = source.Text;
            if (classChanges)
            {
                text = RewriteClassName(text, entry.Name, this._target.ClassName, oldNameIsNamespacePrefix);
            }
            if (renameNamespace)
            {
                foreach (string root in movableRoots)
                {
                    text = RewriteNamespace(text, root, this._target.Value);
                }
            }

            if (!string.Equals(text, source.Text, StringComparison.Ordinal))
            {
                this._edits.Add(new FileEdit(source.Path, source.RelativePath, text));
            }
        }
    }

    /// <summary>
    /// Replaces the old class name everywhere it is used as a bare identifier, leaving
    /// <c>namespace</c> declarations and <c>using</c> directives alone.
    ///
    /// <para>Regular expressions rather than a parser, and honest about it: this is the same
    /// find-and-replace an engineer would do by hand, with the two boundaries that actually catch
    /// people. It will also rewrite the name inside a string literal, which in a domain model is
    /// almost always a doc comment or an exception message that should move anyway.</para>
    /// </summary>
    private static string RewriteClassName(string text, string oldName, string newName, bool oldNameIsNamespacePrefix)
    {
        string pattern = oldNameIsNamespacePrefix
            ? $@"(?<![\w.]){Regex.Escape(oldName)}(?![\w.])"   // not followed by '.' - that is the namespace
            : $@"(?<![\w.]){Regex.Escape(oldName)}(?!\w)";     // '.' is fine - it is a member access

        var result = new List<string>();
        foreach (string line in SplitKeepingLineEndings(text))
        {
            string trimmed = line.TrimStart();
            bool isNamespaceOrUsing =
                trimmed.StartsWith("namespace ", StringComparison.Ordinal)
                || trimmed.StartsWith("using ", StringComparison.Ordinal);

            result.Add(isNamespaceOrUsing ? line : Regex.Replace(line, pattern, newName));
        }
        return string.Concat(result);
    }

    /// <summary>Moves the leading segment of any namespace or using that starts with the old name.</summary>
    private static string RewriteNamespace(string text, string oldPrefix, string newPrefix)
    {
        string pattern = $@"^(?<lead>\s*(?:namespace|using)\s+){Regex.Escape(oldPrefix)}(?<rest>[.;\s])";
        return Regex.Replace(text, pattern, m => m.Groups["lead"].Value + newPrefix + m.Groups["rest"].Value,
            RegexOptions.Multiline);
    }

    private void PlanFileMoves(EntryClass entry)
    {
        this.PlanMove(this._project.ProjectFilePath, this._target.ProjectFileName);

        string entryFolder = Path.GetDirectoryName(entry.Source.Path)!;
        this.PlanMove(entry.Source.Path, this._target.ClassFileName, entryFolder);
    }

    private void PlanMove(string from, string newFileName, string? folder = null)
    {
        string target = Path.Combine(folder ?? Path.GetDirectoryName(from)!, newFileName);
        if (string.Equals(from, target, StringComparison.OrdinalIgnoreCase)) return;

        if (File.Exists(target))
        {
            throw new UsageFailure(
                $"'{Path.GetRelativePath(this._project.Folder, target)}' already exists, so " +
                $"'{Path.GetRelativePath(this._project.Folder, from)}' cannot be renamed on to it. " +
                "Nothing was written." + Environment.NewLine +
                "Move or delete it and re-run, or pick a different name.");
        }

        this._moves.Add(new FileMove(
            from, target,
            Path.GetRelativePath(this._project.Folder, from),
            Path.GetRelativePath(this._project.Folder, target)));
    }

    private void PlanBundle()
    {
        if (!this._project.HasBundle)
        {
            throw new BundleFailure(
                $"No {ModelProject.BundleFileName} in {this._project.Folder}." + Environment.NewLine +
                "Two of the four names live in it, so a rename without it would leave the model " +
                "inconsistent by definition. Nothing was written.");
        }

        using BundleFile bundle = BundleFile.Open(this._project.BundlePath);
        bundle.RequireWellFormed();

        SheetTable meta = bundle.Sheet(SheetSpec.Meta);
        this.BundleNeedsWriting =
            !string.Equals(MetaValue(meta, MetaKeys.MainDll), this._target.MainDll, StringComparison.Ordinal)
            || !string.Equals(MetaValue(meta, MetaKeys.MainClass), this._target.MainClass, StringComparison.Ordinal);
    }

    private void WriteBundle()
    {
        using BundleFile bundle = BundleFile.Open(this._project.BundlePath);
        bundle.RequireWellFormed();

        var plans = new List<RowPlan>
        {
            Plan(bundle, MetaKeys.MainDll, this._target.MainDll),
            Plan(bundle, MetaKeys.MainClass, this._target.MainClass),
        };

        // --force, because these rows exist and hold the old name: overwriting them IS the job,
        // and stopping to ask would make a rename impossible to complete.
        BundleWriter.Apply(bundle, plans, force: true, output: TextWriter.Null);
    }

    private static RowPlan Plan(BundleFile bundle, string key, string value)
        => BundleWriter.Plan(bundle, SheetSpec.Meta, key,
            new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["Setting"] = key,
                ["Value"] = value,
            });

    private static string MetaValue(SheetTable meta, string key)
    {
        int row = meta.FindRowByKey("Setting", key);
        return row < 0 ? string.Empty : meta.Text(row, "Value").Trim();
    }

    /// <summary>The leading segment of a namespace - <c>MyModel</c> from <c>MyModel.Objects</c>.</summary>
    private static string RootSegment(string ns)
    {
        int dot = ns.IndexOf('.');
        return dot < 0 ? ns : ns[..dot];
    }

    private static IEnumerable<string> SplitKeepingLineEndings(string text)
    {
        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] != '\n') continue;
            yield return text[start..(i + 1)];
            start = i + 1;
        }
        if (start < text.Length) yield return text[start..];
    }
}

/// <summary>A file whose contents change.</summary>
internal sealed record FileEdit(string Path, string RelativePath, string NewText);

/// <summary>A file that changes name.</summary>
internal sealed record FileMove(string From, string To, string RelativeFrom, string RelativeTo);

/// <summary>
/// Copies of every file a rename is about to touch, so the whole operation can be undone.
///
/// <para>Lives in the system temporary folder rather than beside the model, so a rename that is
/// killed hard leaves nothing odd-looking in the engineer's project - and so that a project on a
/// read-only or nearly-full disk fails on the backup, before anything has been written, rather
/// than half way through.</para>
/// </summary>
internal sealed class RenameBackup : IDisposable
{
    private readonly string _folder;
    private readonly Dictionary<string, string> _saved = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _created = new();
    private bool _committed;
    private bool _restored;

    public RenameBackup()
    {
        this._folder = Path.Combine(
            Path.GetTempPath(), "jcass-dm-rename", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(this._folder);
    }

    /// <summary>Takes a copy of a file that is about to be written or moved.</summary>
    public void Save(string path)
    {
        if (this._saved.ContainsKey(path) || !File.Exists(path)) return;

        string copy = Path.Combine(this._folder, $"{this._saved.Count:D3}{Path.GetExtension(path)}");
        File.Copy(path, copy);
        this._saved[path] = copy;
    }

    /// <summary>Records a path that did not exist before, so it can be removed on a restore.</summary>
    public void RecordCreated(string path) => this._created.Add(path);

    /// <summary>
    /// Puts every saved file back and removes everything that appeared. Runs at most once, and
    /// never throws.
    ///
    /// <para>Both of those matter and both were learned from the same test. A restore that can
    /// run twice hits a file it has already put back - and if that file is the reason the rename
    /// failed in the first place, the second attempt throws out of <c>Dispose</c> and replaces a
    /// clear "the model has been put back" message with a stack trace and exit code 9. Restoring
    /// is the last thing standing between the engineer and a half-renamed model; it does not get
    /// to have its own failure mode.</para>
    /// </summary>
    public void Restore()
    {
        if (this._restored) return;
        this._restored = true;

        foreach (string created in this._created)
        {
            try { if (File.Exists(created)) File.Delete(created); }
            catch (Exception) { /* the original is restored below regardless */ }
        }

        foreach ((string original, string copy) in this._saved)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(original)!);
                File.Copy(copy, original, overwrite: true);
            }
            catch (Exception)
            {
                // A file that could not be written is a file that was never changed - the failure
                // being reported by the caller is almost always the same permission or lock that
                // stopped the write. Throwing here would replace the real cause with a worse
                // message.
            }
        }
    }

    /// <summary>Marks the operation as successful. Only the temporary copies are discarded.</summary>
    public void Commit() => this._committed = true;

    public void Dispose()
    {
        if (!this._committed)
        {
            // Disposed without a commit means an exception escaped somewhere the catch did not
            // cover. Restoring is the safe default: the whole promise of this verb is that a
            // model is never left half-renamed.
            this.Restore();
        }

        try { Directory.Delete(this._folder, recursive: true); }
        catch (Exception) { /* a leftover temp folder is not worth failing a rename over */ }
    }
}
