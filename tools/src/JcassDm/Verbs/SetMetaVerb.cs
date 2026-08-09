using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JcassDm.Bundle;
using JcassDm.Cli;

namespace JcassDm.Verbs;

/// <summary>
/// <c>jcass-dm set-meta &lt;bundle&gt; [--main-dll x] [--main-class y] [--display-name z] [--force]</c>
///
/// <para>Writes the three settings the framework needs before it can load anything: which
/// DLL, which class inside it, and the name a person sees in the web app.</para>
///
/// <para>The three are planned together and written together. Half a rename - the DLL name
/// updated and the class name not - produces a bundle that loads the right assembly and
/// then cannot find the class in it, which is the four-name rule failing in its most
/// confusing form. Either all of it lands or none of it does.</para>
///
/// <para>Each option is individually optional and at least one is required, which is a
/// superset of the documented signature: it means changing only the display name does not
/// oblige the caller to restate a DLL name they were not thinking about.</para>
/// </summary>
internal static class SetMetaVerb
{
    public static int Run(ArgumentSet args, TextWriter output)
    {
        string path = args.BundlePath();

        string? mainDll = args.Optional("--main-dll");
        string? mainClass = args.Optional("--main-class");
        string? displayName = args.Optional("--display-name", "--model-name");
        bool force = args.Flag("--force");
        args.CheckForUnknownOptions();

        if (mainDll is null && mainClass is null && displayName is null)
        {
            throw new UsageFailure(
                "set-meta needs at least one of --main-dll, --main-class or --display-name.");
        }

        var settings = new Dictionary<string, string>(StringComparer.Ordinal);
        if (mainDll is not null) settings[MetaKeys.MainDll] = NameRules.RequireDllFileName(mainDll, "--main-dll");
        if (mainClass is not null) settings[MetaKeys.MainClass] = NameRules.RequireTypeName(mainClass, "--main-class");
        if (displayName is not null) settings[MetaKeys.ModelName] = NameRules.RequireClean(displayName, "--display-name");

        using BundleFile bundle = BundleFile.Open(path);
        bundle.RequireWellFormed();

        var plans = settings
            .Select(setting => BundleWriter.Plan(
                bundle,
                SheetSpec.Meta,
                setting.Key,
                new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["Setting"] = setting.Key,
                    ["Value"] = setting.Value,
                }))
            .ToList();

        BundleWriter.Apply(bundle, plans, force, output);
        WarnIfNamesDisagree(bundle, output);
        return ExitCode.Ok;
    }

    /// <summary>
    /// Points out a <c>main_dll</c> and <c>main_class</c> that cannot both be right.
    ///
    /// <para>A note rather than a refusal: a namespace-qualified class name is legitimate,
    /// so only the last segment is compared, and the caller may be part-way through a
    /// rename they intend to finish. But a mismatch here is the single most common way a
    /// model breaks, and it breaks only on the debug path - a normal run reads these two
    /// values and is perfectly happy, so nothing else will mention it until somebody
    /// presses F5.</para>
    /// </summary>
    private static void WarnIfNamesDisagree(BundleFile bundle, TextWriter output)
    {
        SheetTable meta = bundle.Sheet(SheetSpec.Meta);

        int dllRow = meta.FindRowByKey("Setting", MetaKeys.MainDll);
        int classRow = meta.FindRowByKey("Setting", MetaKeys.MainClass);
        if (dllRow < 0 || classRow < 0) return;

        string dll = meta.Text(dllRow, "Value").Trim();
        string className = meta.Text(classRow, "Value").Trim();
        if (dll.Length == 0 || className.Length == 0) return;

        string assembly = dll.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? dll[..^4] : dll;
        string lastSegment = className.Split('.').Last();
        if (string.Equals(assembly, lastSegment, StringComparison.Ordinal)) return;

        output.WriteLine();
        output.WriteLine($"note       main_dll is '{dll}' and main_class is '{className}'.");
        output.WriteLine( "           A debug run in the web app ignores this sheet and derives both names from your");
        output.WriteLine( "           .csproj filename, so those two only ever agree with a debug run when the");
        output.WriteLine($"           project is called '{lastSegment}.csproj' and its entry class '{lastSegment}'.");
    }
}
