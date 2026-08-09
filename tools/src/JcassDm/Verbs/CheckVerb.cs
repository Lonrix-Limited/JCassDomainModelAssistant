using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using JcassDm.Bundle;
using JcassDm.Cli;
using JcassDm.Project;

namespace JcassDm.Verbs;

/// <summary>
/// <c>jcass-dm check [--project &lt;path&gt;] [--lookups &lt;path to lookups.xlsx&gt;]</c>
///
/// <para>Reports whether a domain model's C#, its bundle and its lookups still agree.</para>
///
/// <para><b>This is an explicit subset and it says so in its own output.</b> The web app's Check
/// Setup is authoritative and it sees things this cannot: the client's actual input CSV, their
/// budget columns, their configuration. What <c>check</c> covers is the set of mistakes that are
/// visible from the model folder alone - and every one of them is a mistake that otherwise fails
/// silently, late, or in a place that does not name its cause.</para>
///
/// <para><b>It is the first thing to run when you inherit a model</b>, not the last thing before
/// you publish. Somebody handed a client's existing model needs to know what state it is in
/// before they change anything, and that is what this output is written to be: a summary
/// somebody who does not write C# can act on, rather than a list of assertion failures.</para>
/// </summary>
internal static class CheckVerb
{
    public static int Run(ArgumentSet args, TextWriter output)
    {
        string projectPath = args.Optional("--project") ?? ".";
        string? lookupsPath = args.Optional("--lookups");
        args.CheckForUnknownOptions();

        ModelProject project = ModelProject.OpenForDiagnosis(projectPath, out IReadOnlyList<string> projectFiles);
        SourceFacts facts = SourceFacts.Read(project.Sources);
        var report = new CheckReport();

        output.WriteLine($"Checking {project.ProjectStem} at {project.Folder}");
        output.WriteLine();

        CheckOneProjectFile(project, projectFiles, report);

        using BundleFile? bundle = OpenBundle(project, report);
        bool bundleUsable = bundle is not null && bundle.Problems.Count == 0;

        CheckFourNames(project, bundleUsable ? bundle : null, report);
        CheckAssemblyName(project, report);

        if (bundle is not null)
        {
            CheckBundleStructure(bundle, report);
            if (bundleUsable)
            {
                CheckParameters(bundle!, facts, report);
                CheckTreatments(bundle!, facts, report);
                CheckResetArms(bundle!, facts, report);
                CheckBudgetCategories(bundle!, report);
            }
        }

        CheckLookups(facts, lookupsPath, report);

        report.Write(output);
        WriteScopeNote(output);

        return report.HasProblems ? ExitCode.CheckFailed : ExitCode.Ok;
    }

    // -----------------------------------------------------------------------------
    // The rules
    // -----------------------------------------------------------------------------

    /// <summary>
    /// The four names, and the one rule everything else in this tool is built around.
    ///
    /// <para>The <c>.csproj</c> filename is treated as the truth, because a debug F5 run has no
    /// choice but to trust it: mid-edit, the source has usually drifted from whatever identity
    /// was last shipped, so the meta sheet describes the published version rather than the one
    /// on screen. A normal run reads the meta sheet instead. The two routes agree only when all
    /// four strings match.</para>
    /// </summary>
    private static void CheckOneProjectFile(
        ModelProject project, IReadOnlyList<string> projectFiles, CheckReport report)
    {
        if (projectFiles.Count == 1)
        {
            report.Pass("one .csproj at the root", Path.GetFileName(project.ProjectFilePath));
            return;
        }

        report.Problem("one .csproj at the root",
            $"There are {projectFiles.Count}: " +
            string.Join(", ", projectFiles.Select(Path.GetFileName)) + ".",
            "A web debug run refuses to guess between two project files, so this stops F5 " +
            "working on its own - before anything about the model itself is even looked at. " +
            $"The rest of these checks assume '{Path.GetFileName(project.ProjectFilePath)}' is " +
            "the real one.",
            "Delete or move the one you do not want, then run check again.");
    }

    private static void CheckFourNames(ModelProject project, BundleFile? bundle, CheckReport report)
    {
        string stem = project.ProjectStem;
        IReadOnlyList<string> entryClasses = project.AllEntryClassNames();

        if (entryClasses.Count == 0)
        {
            report.Problem("the four names",
                "No class in this project derives from DomainModelBase.",
                "The framework finds your model by looking for that base class. Without one, " +
                "nothing loads - a normal run and a debug run fail the same way.",
                $"Expected: public class {stem} : DomainModelBase");
            return;
        }
        if (entryClasses.Count > 1)
        {
            report.Problem("the four names",
                $"{entryClasses.Count} classes derive from DomainModelBase: {string.Join(", ", entryClasses)}.",
                "The framework loads exactly one, and which one is decided by the bundle rather " +
                "than by anything you can see in the code.",
                "Keep one and delete or re-base the others.");
            return;
        }

        string entryClass = entryClasses[0];
        var disagreements = new List<string>();

        if (!string.Equals(entryClass, stem, StringComparison.Ordinal))
        {
            disagreements.Add($"the entry class is '{entryClass}' but the project file is '{stem}.csproj'");
        }

        // The other two names live in the bundle. Checked here rather than with the bundle rules,
        // because a meta sheet naming a third identity is the same failure as a class that
        // disagrees with the project file - and reading only two of the four would let it pass.
        if (bundle is not null)
        {
            SheetTable meta = bundle.Sheet(SheetSpec.Meta);
            string mainDll = MetaValue(meta, MetaKeys.MainDll);
            string mainClass = MetaValue(meta, MetaKeys.MainClass);

            string expectedDll = stem + ".dll";
            if (mainDll.Length > 0 && !string.Equals(mainDll, expectedDll, StringComparison.Ordinal))
            {
                disagreements.Add($"meta.main_dll is '{mainDll}' but the project file is '{stem}.csproj', " +
                                  $"so the build produces '{expectedDll}'");
            }
            if (mainClass.Length > 0 && !string.Equals(mainClass.Split('.').Last(), stem, StringComparison.Ordinal))
            {
                disagreements.Add($"meta.main_class is '{mainClass}' but the project file is '{stem}.csproj'");
            }
        }

        if (disagreements.Count == 0)
        {
            report.Pass("the four names",
                bundle is null ? $"class and .csproj both read '{stem}'" : $"all read '{stem}'",
                bundle is null
                    ? "The bundle could not be read, so meta.main_dll and meta.main_class - the other " +
                      "two of the four - were not checked."
                    : null);
            return;
        }

        report.Problem("the four names",
            string.Join("; ", disagreements) + ".",
            "A normal model run reads the class name from the bundle's meta sheet. A debug (F5) " +
            "run ignores that sheet and derives it from the .csproj filename. They only ever " +
            "agree when all four names match - so this model may run normally today and fail on " +
            "F5 with \"Domain Model class '" + stem + "' was not found in the specified .dll\".",
            $"Fix all four at once:  jcass-dm rename {stem} --project \"{project.Folder}\"");
    }

    /// <summary>
    /// <c>&lt;AssemblyName&gt;</c> is the one setting that can break the four-name rule with
    /// nothing on screen to show for it.
    /// </summary>
    private static void CheckAssemblyName(ModelProject project, CheckReport report)
    {
        string? declared = project.DeclaredAssemblyName();
        if (declared is null)
        {
            report.Pass("<AssemblyName>", "not set, which is correct");
            return;
        }

        if (string.Equals(declared, project.ProjectStem, StringComparison.Ordinal))
        {
            report.Note("<AssemblyName>",
                $"is set to '{declared}', which matches the project filename, so nothing is broken.",
                "It is still worth deleting. It is the one setting that can break the four-name " +
                "rule silently: the day somebody renames the .csproj, the DLL keeps coming out " +
                "under the old name and only a debug run notices.",
                "Delete the <AssemblyName> element from the .csproj.");
            return;
        }

        report.Problem("<AssemblyName>",
            $"is set to '{declared}', but the project file is '{project.ProjectStem}.csproj'.",
            $"The compiled DLL will be called {declared}.dll while a debug run looks for " +
            $"{project.ProjectStem}.dll. That is the four-name rule broken in its hardest-to-see form.",
            "Delete the <AssemblyName> element. The assembly name then follows the filename, which " +
            "is what keeps the four in step.");
    }

    private static BundleFile? OpenBundle(ModelProject project, CheckReport report)
    {
        if (!project.HasBundle)
        {
            report.Problem("the bundle",
                $"There is no {ModelProject.BundleFileName} beside the .csproj.",
                "The framework reads it before it loads anything: it declares which DLL and class " +
                "to load, which input columns the model expects, and which treatments it can " +
                "produce. Without it the model cannot run at all.",
                "Expected at: " + project.BundlePath);
            return null;
        }

        try
        {
            return BundleFile.Open(project.BundlePath);
        }
        catch (CommandFailure failure)
        {
            report.Problem("the bundle", failure.Message,
                "Nothing else about the bundle could be checked.", null);
            return null;
        }
    }

    private static void CheckBundleStructure(BundleFile bundle, CheckReport report)
    {
        if (bundle.Problems.Count > 0)
        {
            report.Problem("bundle structure",
                string.Join(" ", bundle.Problems),
                "All five sheets have to be there, spelled exactly, with the columns the framework " +
                "reads by name - including network_functions, even with no rows in it.",
                "The rest of the bundle checks were skipped.");
            return;
        }

        report.Pass("bundle structure", "five sheets, all required columns present");

        foreach (string warning in bundle.Warnings)
        {
            report.Note("the bundle", warning, null, null);
        }
    }

    /// <summary>
    /// Every parameter the bundle declares has to be written in <c>SetParameterValues</c>.
    ///
    /// <para><b>Nothing in the framework catches this one</b> - verified against
    /// <c>ModelSetupChecker.RunSetupChecksStage1</c>, which has no such rule, and against
    /// <c>ModelParameterData</c>, whose arrays are simply allocated at their default. A declared
    /// parameter that is never written is zero for every element in every period, and the run
    /// completes normally. This rule is therefore the only defence there is, which is why its
    /// message says so rather than pointing downstream at a framework error that never arrives.</para>
    /// </summary>
    private static void CheckParameters(BundleFile bundle, SourceFacts facts, CheckReport report)
    {
        SheetTable sheet = bundle.Sheet(SheetSpec.Parameters);
        var declared = Rows(sheet, "parameter_name").ToList();

        if (!facts.FoundSetParameterValues)
        {
            if (declared.Count == 0)
            {
                report.Pass("parameters vs C#", "none declared, none written");
                return;
            }

            report.Problem("parameters vs C#",
                $"The bundle declares {declared.Count} parameter{(declared.Count == 1 ? "" : "s")} " +
                "and there is no SetParameterValues method anywhere in the project.",
                "Every parameter on the bundle's parameters sheet must be written through the " +
                "framework's sinks. Nothing reports it if one is not: the framework allocates the " +
                "parameter and leaves it at zero for every element in every period, and the run " +
                "completes with a column of zeros in the outputs.",
                "Declared: " + string.Join(", ", declared));
            return;
        }

        var missing = declared.Where(p => !facts.ParametersWritten.Contains(p)).ToList();
        if (missing.Count == 0)
        {
            report.Pass("parameters vs C#", $"{declared.Count} declared, all written");
            return;
        }

        report.Problem("parameters vs C#",
            $"Declared in the bundle but never written in SetParameterValues: {string.Join(", ", missing)}.",
            "Nothing else catches this. The framework does not check that a declared parameter is " +
            "ever written - it allocates the parameter and leaves it at zero, so the run completes " +
            "and the outputs carry a column of zeros that looks like a modelling result. This " +
            "check is the only place you will be told. Note that the reverse - writing a parameter " +
            "the bundle does not declare - is not checked here, because a name assembled at run " +
            "time would look like that too; the framework does throw on that one, by name.",
            "Either write it in SetParameterValues, or remove its row from the parameters sheet.");
    }

    /// <summary>
    /// Bundle treatments and C# treatment names, in both directions where the project's style
    /// allows it.
    /// </summary>
    private static void CheckTreatments(BundleFile bundle, SourceFacts facts, CheckReport report)
    {
        SheetTable sheet = bundle.Sheet(SheetSpec.Treatments);
        var declared = Rows(sheet, "treatment_name").ToList();

        var notInCode = declared.Where(t => !facts.MentionsTreatment(t)).ToList();
        if (notInCode.Count > 0)
        {
            report.Problem("treatments vs C#",
                $"Declared in the bundle but never mentioned in the C#: {string.Join(", ", notInCode)}.",
                "A treatment nothing produces is dead weight in the bundle. More often it means a " +
                "name that does not match - the two strings are compared exactly, so a difference " +
                "in case, or a trailing space you cannot see, reads as a completely different " +
                "treatment.",
                "Check the spelling against your C#, or remove the row.");
        }

        if (facts.TreatmentStyle == TreatmentNameStyle.Constants)
        {
            var declaredSet = new HashSet<string>(declared, StringComparer.Ordinal);
            var notInBundle = facts.TreatmentNameConstants
                .Where(pair => !declaredSet.Contains(pair.Value))
                .Select(pair => $"{pair.Key} = \"{pair.Value}\"")
                .ToList();

            if (notInBundle.Count > 0)
            {
                report.Problem("treatments vs C#",
                    "On TreatmentNames but with no row in the bundle: " + string.Join(", ", notInBundle) + ".",
                    "A treatment your code can produce and the bundle does not declare fails the " +
                    "run the first period it fires.",
                    "Add it:  jcass-dm add-treatment <bundle> --name <name> --budget-category <category>");
            }

            if (notInCode.Count == 0 && notInBundle.Count == 0)
            {
                report.Pass("treatments vs C#",
                    $"{declared.Count} declared, matched against TreatmentNames in both directions");
            }
        }
        else if (notInCode.Count == 0)
        {
            report.Pass("treatments vs C#",
                declared.Count == 0
                    ? "none declared"
                    : $"{declared.Count} declared, all found in the C#",
                declared.Count == 0
                    ? null
                    : "Checked one way only: this project writes treatment names as bare string " +
                      "literals rather than constants on a TreatmentNames class, so jcass-dm cannot " +
                      "enumerate what the code can produce. A TreatmentNames class makes the reverse " +
                      "direction checkable too.");
        }
    }

    /// <summary>
    /// A treatment with no arm in the reset switch is funded, charged to a budget, reported as
    /// delivered - and changes nothing. That is the quietest failure in a domain model.
    /// </summary>
    private static void CheckResetArms(BundleFile bundle, SourceFacts facts, CheckReport report)
    {
        SheetTable sheet = bundle.Sheet(SheetSpec.Treatments);
        var declared = Rows(sheet, "treatment_name").ToList();

        if (declared.Count == 0)
        {
            report.Pass("treatment reset arms", "no treatments to handle");
            return;
        }

        if (!facts.FoundTreatmentSwitch)
        {
            report.Skip("treatment reset arms",
                "no switch over a treatment name was found, so jcass-dm cannot tell which " +
                "treatments your Reset handles.",
                "Models that decide resets with if/else chains or a dictionary are perfectly valid " +
                "and simply cannot be checked this way. Confirm by hand that every treatment " +
                "changes the element's state, and that an unhandled one fails loudly.");
            return;
        }

        var unhandled = declared.Where(t => !facts.TreatmentsWithCaseArm.Contains(t)).ToList();
        if (unhandled.Count == 0)
        {
            report.Pass("treatment reset arms", $"all {declared.Count} have a case arm");
            return;
        }

        report.Problem("treatment reset arms",
            $"No case arm in the reset switch for: {string.Join(", ", unhandled)}.",
            "A treatment that is triggered and funded but not handled on reset spends money and " +
            "changes nothing about the element. The forecast shows the cost and none of the " +
            "benefit, and nothing says why.",
            "Add a case for each. If a treatment deliberately leaves the element alone - routine " +
            "maintenance usually does - give it an empty case rather than letting it fall through " +
            "to the default, so the intent is written down.");
    }

    /// <summary>
    /// Every treatment needs a budget category that matches a column of the client's budget sheet.
    ///
    /// <para><b>This one is loud, not silent, and the wording matters.</b> The framework's
    /// <c>ModelSetupChecker</c> validates every treatment type's own budget category against the
    /// budget columns at setup and names any mismatch - a blank one included, since a blank never
    /// matches a column. So the value of checking it here is only that it is found before an
    /// upload rather than during one. The genuinely unchecked case is a category supplied at run
    /// time to <c>TreatmentInstance.AssignBudgetCategoryFractions</c>, which jcass-dm cannot see
    /// and which the framework cannot validate either.</para>
    /// </summary>
    private static void CheckBudgetCategories(BundleFile bundle, CheckReport report)
    {
        SheetTable sheet = bundle.Sheet(SheetSpec.Treatments);

        var blank = new List<string>();
        var categories = new SortedSet<string>(StringComparer.Ordinal);

        for (int i = 0; i < sheet.RowCount; i++)
        {
            string name = sheet.Text(i, "treatment_name").Trim();
            if (name.Length == 0) continue;

            string category = sheet.Text(i, "budget_category").Trim();
            if (category.Length == 0) blank.Add($"{name} (row {sheet.ExcelRowOf(i)})");
            else categories.Add(category);
        }

        if (blank.Count > 0)
        {
            report.Problem("budget categories",
                "No budget_category on: " + string.Join(", ", blank) + ".",
                "A blank category matches no column of the client's budget sheet, so the model " +
                "fails at setup with 'Treatment budget category '' has no matching column in the " +
                "Budget.' - before a single period is modelled.",
                "Set it to a column that exists in the client's inputs\\budgets.xlsx.");
            return;
        }

        if (categories.Count == 0)
        {
            report.Pass("budget categories", "no treatments to charge");
            return;
        }

        report.Note("budget categories",
            "every treatment names one: " + string.Join(", ", categories) + ".",
            "jcass-dm cannot see the client's inputs\\budgets.xlsx, so it cannot tell you whether " +
            "these columns exist there. The framework can, and does: a category here with no " +
            "matching budget column stops the run at setup with a message naming it. So this is " +
            "a check on when you find out, not on whether you do.",
            "Run the web app's Check Setup to have it confirmed against the client's actual " +
            "budget sheet before you upload. Note that neither check covers a category name your " +
            "C# passes to AssignBudgetCategoryFractions at run time - that one is not validated " +
            "anywhere, and a wrong name ends the run mid-way with a KeyNotFoundException naming " +
            "nothing. Compare those keys against model.Budget.BudgetCategories yourself.");
    }

    /// <summary>
    /// Lookup sets the code asks for, against a <c>lookups.xlsx</c> when one is supplied.
    ///
    /// <para>Skipped rather than assumed when no file is given: the client's lookups live on the
    /// server, and an engineer working locally often has no copy. Reporting a pass on a file
    /// nobody looked at would be worse than saying nothing.</para>
    /// </summary>
    private static void CheckLookups(SourceFacts facts, string? lookupsPath, CheckReport report)
    {
        if (lookupsPath is null)
        {
            report.Skip("lookup sets",
                facts.LookupSetsUsed.Count == 0
                    ? "no lookups.xlsx given, and no lookup set names were recognised in the C#."
                    : $"no lookups.xlsx given. The C# asks for: {string.Join(", ", facts.LookupSetsUsed.OrderBy(s => s, StringComparer.Ordinal))}.",
                "Pass --lookups <path to the client's inputs\\lookups.xlsx> to have these checked. " +
                "A missing lookup set fails at setup with a message naming it, so this one is not " +
                "silent - it is just better found before a run than during one.");
            return;
        }

        string full = Path.GetFullPath(lookupsPath);
        if (!File.Exists(full))
        {
            report.Problem("lookup sets", $"No file at {full}.",
                "The --lookups path has to point at a lookups.xlsx.", null);
            return;
        }

        IReadOnlySet<string> available;
        try
        {
            available = ReadLookupSetNames(full);
        }
        catch (Exception ex)
        {
            report.Problem("lookup sets", $"Could not read {full}: {ex.Message}",
                "If it is open in Excel, close it and try again.", null);
            return;
        }

        var missing = facts.LookupSetsUsed
            .Where(s => !available.Contains(s))
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();

        if (missing.Count > 0)
        {
            report.Problem("lookup sets",
                "Asked for by the C# but not in " + Path.GetFileName(full) + ": " + string.Join(", ", missing) + ".",
                "A lookup set that is not there fails at setup. If a name here looks unfamiliar, " +
                "check for a typo in a set name constant - the spreadsheet and the C# are compared " +
                "exactly.",
                $"{Path.GetFileName(full)} has: {string.Join(", ", available.OrderBy(s => s, StringComparer.Ordinal))}");
        }
        else
        {
            report.Pass("lookup sets",
                facts.LookupSetsUsed.Count == 0
                    ? "none recognised in the C#, nothing to check"
                    : $"all {facts.LookupSetsUsed.Count} found in {Path.GetFileName(full)}");
        }

        if (facts.UnresolvedLookupSetArguments.Count > 0)
        {
            report.Note("lookup sets in code",
                "some set names could not be worked out from the source: " +
                string.Join(", ", facts.UnresolvedLookupSetArguments) + ".",
                "jcass-dm reads C# as text rather than compiling it, so a set name built at run " +
                "time, or held somewhere it cannot follow, is invisible to this check. Those were " +
                "not checked either way.", null);
        }
    }

    /// <summary>
    /// The <c>lookup_set_name</c> values across every <c>lkp_</c> sheet, which is how the
    /// framework merges them: one flat table, addressed by (set name, key).
    /// </summary>
    private static IReadOnlySet<string> ReadLookupSetNames(string path)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);

        using var workbook = new XLWorkbook(path);
        foreach (IXLWorksheet sheet in workbook.Worksheets)
        {
            if (!sheet.Name.StartsWith("lkp_", StringComparison.Ordinal)) continue;

            var table = new SheetTable(sheet);
            if (!table.HasColumn("lookup_set_name")) continue;

            for (int i = 0; i < table.RowCount; i++)
            {
                string value = table.Text(i, "lookup_set_name").Trim();
                if (value.Length > 0) names.Add(value);
            }
        }

        return names;
    }

    private static string MetaValue(SheetTable meta, string key)
    {
        int row = meta.FindRowByKey("Setting", key);
        return row < 0 ? string.Empty : meta.Text(row, "Value").Trim();
    }

    private static IEnumerable<string> Rows(SheetTable sheet, string column)
    {
        for (int i = 0; i < sheet.RowCount; i++)
        {
            string value = sheet.Text(i, column).Trim();
            if (value.Length > 0) yield return value;
        }
    }

    private static void WriteScopeNote(TextWriter output)
    {
        output.WriteLine();
        output.WriteLine("WHAT THIS DID AND DID NOT COVER");
        output.WriteLine();
        output.WriteLine("  jcass-dm check is a LOCAL SUBSET. It looks at your model folder: the .csproj, the C#");
        output.WriteLine("  and the bundle. The web app's Check Setup page is authoritative and sees things this");
        output.WriteLine("  cannot - your client's actual input CSV, their budget columns, their configuration.");
        output.WriteLine("  A green result here means \"nothing locally visible is wrong\", not \"this will run\".");
        output.WriteLine();
        output.WriteLine("  It also reads C# as text rather than compiling it. Anything assembled at run time is");
        output.WriteLine("  invisible to it, and any rule it could not apply is reported as SKIPPED above rather");
        output.WriteLine("  than passed.");
    }
}

/// <summary>
/// The findings, in the order the rules ran, split into what stops the model and what is only
/// worth knowing.
///
/// <para>The split is <see cref="BundleFile.Problems"/> and <see cref="BundleFile.Warnings"/>
/// applied to the whole model rather than to one file, and it is the distinction that makes the
/// output usable: a reader who fixes only the PROBLEM lines has a model that runs.</para>
/// </summary>
internal sealed class CheckReport
{
    private readonly List<Finding> _findings = new();

    /// <summary>True when at least one finding stops the model.</summary>
    public bool HasProblems => this._findings.Any(f => f.Level == FindingLevel.Problem);

    /// <summary>A rule that passed.</summary>
    public void Pass(string rule, string summary, string? detail = null)
        => this._findings.Add(new Finding(FindingLevel.Pass, rule, summary, detail, null));

    /// <summary>A rule that could not be applied. Never counted as a pass.</summary>
    public void Skip(string rule, string summary, string? detail = null)
        => this._findings.Add(new Finding(FindingLevel.Skipped, rule, summary, detail, null));

    /// <summary>Worth knowing, does not stop the model running.</summary>
    public void Note(string rule, string summary, string? detail, string? action)
        => this._findings.Add(new Finding(FindingLevel.Note, rule, summary, detail, action));

    /// <summary>Stops the model, or will.</summary>
    public void Problem(string rule, string summary, string? detail, string? action)
        => this._findings.Add(new Finding(FindingLevel.Problem, rule, summary, detail, action));

    /// <summary>Prints the one-line-per-rule table, then a paragraph for anything that needs one.</summary>
    public void Write(TextWriter output)
    {
        int width = this._findings.Count == 0 ? 20 : Math.Max(20, this._findings.Max(f => f.Rule.Length));

        foreach (Finding finding in this._findings)
        {
            output.WriteLine($"  {finding.Rule.PadRight(width)}  {Label(finding.Level).PadRight(8)}  {finding.Summary}");
        }

        int problems = this._findings.Count(f => f.Level == FindingLevel.Problem);
        int notes = this._findings.Count(f => f.Level == FindingLevel.Note);
        int skipped = this._findings.Count(f => f.Level == FindingLevel.Skipped);

        output.WriteLine();
        output.WriteLine(Summarise(problems, notes, skipped));

        var explained = this._findings
            .Where(f => f.Level is FindingLevel.Problem or FindingLevel.Note or FindingLevel.Skipped)
            .Where(f => f.Detail is not null || f.Action is not null)
            .ToList();

        foreach (Finding finding in explained)
        {
            output.WriteLine();
            output.WriteLine($"{Label(finding.Level)}: {finding.Rule}");
            output.WriteLine($"  {finding.Summary}");
            if (finding.Detail is not null)
            {
                output.WriteLine();
                foreach (string line in Wrap(finding.Detail)) output.WriteLine("  " + line);
            }
            if (finding.Action is not null)
            {
                output.WriteLine();
                bool first = true;
                foreach (string line in Wrap(finding.Action, 85))
                {
                    output.WriteLine((first ? "  -> " : "     ") + line);
                    first = false;
                }
            }
        }
    }

    private static string Summarise(int problems, int notes, int skipped)
    {
        if (problems == 0 && notes == 0 && skipped == 0)
        {
            return "Nothing locally visible is wrong with this model.";
        }

        var parts = new List<string>();
        if (problems > 0) parts.Add($"{problems} problem{(problems == 1 ? "" : "s")} to fix");
        if (notes > 0) parts.Add($"{notes} worth knowing about");
        if (skipped > 0) parts.Add($"{skipped} rule{(skipped == 1 ? "" : "s")} could not be applied");

        string sentence = string.Join(", ", parts) + ".";
        return problems == 0
            ? "No problems. " + char.ToUpperInvariant(sentence[0]) + sentence[1..]
            : sentence;
    }

    private static string Label(FindingLevel level) => level switch
    {
        FindingLevel.Pass => "OK",
        FindingLevel.Note => "NOTE",
        FindingLevel.Skipped => "SKIPPED",
        _ => "PROBLEM",
    };

    /// <summary>Wraps at 88 columns, so a paragraph stays readable in a terminal that does not wrap well.</summary>
    private static IEnumerable<string> Wrap(string text, int width = 88)
    {
        var line = new System.Text.StringBuilder();
        foreach (string word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Length > 0 && line.Length + 1 + word.Length > width)
            {
                yield return line.ToString();
                line.Clear();
            }
            if (line.Length > 0) line.Append(' ');
            line.Append(word);
        }
        if (line.Length > 0) yield return line.ToString();
    }

    private enum FindingLevel { Pass, Note, Skipped, Problem }

    private sealed record Finding(FindingLevel Level, string Rule, string Summary, string? Detail, string? Action);
}
