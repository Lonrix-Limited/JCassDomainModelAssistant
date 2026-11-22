using System;
using System.IO;
using System.Linq;
using JcassDm.Cli;
using Xunit;

namespace JcassDm.Tests;

/// <summary>
/// <c>check</c>, against a deliberately broken model per rule.
///
/// <para><b>The point of these is the failing case, not the passing one.</b> A check that has
/// never been observed to fail has never been tested: it could be reading the wrong column, or a
/// file that is not there, and it would report OK either way. Each test below names the fixture
/// it uses and the words the engineer should see.</para>
/// </summary>
public class CheckTests
{
    [Fact]
    public void A_healthy_model_passes()
    {
        ToolResult result = Run("healthy");

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("No problems", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("PROBLEM", result.Output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("names-disagree")]
    [InlineData("parameter-not-written")]
    [InlineData("treatment-not-in-bundle")]
    [InlineData("treatment-not-in-code")]
    [InlineData("missing-reset-arm")]
    [InlineData("two-csproj")]
    [InlineData("assembly-name-set")]
    [InlineData("blank-budget-category")]
    [InlineData("target-framework-behind")]
    [InlineData("main-class-namespaced")]
    public void Every_broken_fixture_is_caught(string fixture)
    {
        ToolResult result = Run(fixture);

        Assert.Equal(ExitCode.CheckFailed, result.ExitCode);
        Assert.Contains("PROBLEM", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_bare_model_folder_is_checked_the_same_as_project()
    {
        // Until 2026-11-20 the path was silently dropped and the current folder checked instead:
        // run from the Assistant, that reported "No .csproj file in ...JCassDomainModelAssistant",
        // naming the wrong folder as the problem. Two agents lost turns to it on the same day.
        ToolResult result = RunArgs("check", FixtureModels.PathTo("names-disagree"));

        Assert.Equal(ExitCode.CheckFailed, result.ExitCode);
        Assert.Contains("the entry class is 'FixtureModel'", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_model_folder_given_twice_is_refused()
    {
        string folder = FixtureModels.PathTo("healthy");

        ToolResult result = RunArgs("check", folder, "--project", folder);

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Contains("given the model folder twice", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void Two_model_folders_are_refused()
    {
        ToolResult result = RunArgs("check", FixtureModels.PathTo("healthy"), FixtureModels.PathTo("two-csproj"));

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Contains("takes one model folder", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void Every_fixture_folder_is_covered_by_a_test()
    {
        // The list in FixtureModels.All and the folders on disk drift apart the moment somebody
        // adds a fixture and forgets a test, and the symptom is a rule that silently has no
        // coverage - which is the exact thing these fixtures exist to prevent.
        string folder = Path.Combine(TestBundle.RepoRoot(), "tools", "fixtures");
        var onDisk = Directory.GetDirectories(folder).Select(Path.GetFileName).OrderBy(n => n, StringComparer.Ordinal);

        Assert.Equal(FixtureModels.All.Keys.OrderBy(n => n, StringComparer.Ordinal), onDisk);
    }

    [Fact]
    public void The_four_names_failure_names_all_of_the_disagreements()
    {
        ToolResult result = Run("names-disagree");

        Assert.Contains("the entry class is 'FixtureModel'", result.Output, StringComparison.Ordinal);
        Assert.Contains("meta.main_dll is 'FixtureModel.dll'", result.Output, StringComparison.Ordinal);
        Assert.Contains("meta.main_class is 'FixtureModel'", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void The_four_names_failure_says_how_to_fix_it()
    {
        ToolResult result = Run("names-disagree");

        // The whole reason rename exists. A diagnosis that leaves somebody to do four manual
        // edits has handed them back the operation that caused the problem.
        Assert.Contains("jcass-dm rename", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_namespace_in_main_class_is_a_problem()
    {
        // Until 2026-11-22 only the last segment of main_class was compared, so
        // 'FixtureModel.Objects.FixtureModel' passed here - while the web app's Check bundle and
        // publish refuse it. A local check that passes what the server refuses is worse than none.
        ToolResult result = Run("main-class-namespaced");

        Assert.Equal(ExitCode.CheckFailed, result.ExitCode);
        Assert.Contains("carries a namespace", result.Output, StringComparison.Ordinal);
        Assert.Contains("bare class name 'FixtureModel'", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_meta_only_disagreement_is_explained_as_passing_F5_and_failing_Check_bundle()
    {
        // The meta sheet disagrees and the class does not, so F5 would run. Saying it fails F5 -
        // which the message said for every disagreement until 2026-11-22 - sends the engineer to
        // look for a failure they will never see.
        ToolResult result = Run("main-class-namespaced");

        Assert.Contains("passes F5", result.Output, StringComparison.Ordinal);
        Assert.Contains("Check bundle", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("was not found in the specified .dll", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_class_disagreement_is_explained_as_failing_F5()
    {
        ToolResult result = Run("names-disagree");

        Assert.Contains("was not found in the specified .dll", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_main_dll_differing_only_in_letter_case_says_so()
    {
        // The server resolves main_dll as a Linux file path, where case matters; on the engineer's
        // Windows machine the two names look interchangeable. The message has to say which it is.
        using TemporaryModel model = FixtureModels.Copy("healthy");
        string bundle = model.PathTo("domain_model_setup.xlsx");
        Assert.Equal(ExitCode.Ok, model.Run("set-meta", bundle, "--main-dll", "fixturemodel.dll", "--force").ExitCode);

        ToolResult result = model.Run("check", "--project", model.Folder);

        Assert.Equal(ExitCode.CheckFailed, result.ExitCode);
        Assert.Contains("differ only in letter case", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_missing_parameter_is_named()
    {
        ToolResult result = Run("parameter-not-written");

        Assert.Contains("par_obj", result.Output, StringComparison.Ordinal);
        Assert.Contains("SetParameterValues", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_missing_parameter_says_that_nothing_else_catches_it()
    {
        // Until 2026-08-10 this rule's message pointed downstream at a framework setup error that
        // does not exist. ModelSetupChecker.RunSetupChecksStage1 has no rule for a declared
        // parameter that is never written, and ModelParameterData simply allocates the array: the
        // parameter is zero for every element in every period and the run completes. That makes
        // this rule the ONLY defence, and the message has to say so - a modeller who believes the
        // framework will catch it later has no reason to act on a local warning now.
        ToolResult result = Run("parameter-not-written");

        Assert.Contains("Nothing else catches this", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("fails at setup when a declared parameter", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_network_function_output_does_not_have_to_be_written()
    {
        // The framework writes these itself, after every element has been stepped. Reporting one
        // as unwritten told the engineer to write it or delete its row, and deleting the row
        // breaks the model.
        using TemporaryModel model = FixtureModels.Copy("parameter-not-written");
        SetNetworkFunction(model, input: "par_condition", output: "par_obj");

        ToolResult result = model.Run("check", "--project", model.Folder);

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.DoesNotContain("never written", result.Output, StringComparison.Ordinal);
        Assert.Contains("set by network functions", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_network_function_input_still_has_to_be_written()
    {
        // The control for the test above: only the OUTPUT is the framework's job.
        using TemporaryModel model = FixtureModels.Copy("parameter-not-written");
        SetNetworkFunction(model, input: "par_obj", output: "par_rank");

        ToolResult result = model.Run("check", "--project", model.Folder);

        Assert.Equal(ExitCode.CheckFailed, result.ExitCode);
        Assert.Contains("never written in SetParameterValues: par_obj", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_row_the_framework_will_not_read_is_reported()
    {
        using TemporaryModel model = FixtureModels.Copy("healthy");
        string bundle = model.PathTo("domain_model_setup.xlsx");
        DumpTests.SetCell(bundle, "network_functions", row: 2, column: 2, value: "percent_rank");

        ToolResult result = model.Run("check", "--project", model.Folder);

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("row 2 has values but its first column", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_treatment_with_no_reset_arm_is_named()
    {
        ToolResult result = Run("missing-reset-arm");

        Assert.Contains("No case arm in the reset switch for: replace", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_blank_budget_category_is_reported_as_a_setup_failure()
    {
        // This asserted "never funded" until 2026-08-10, which was wrong and had been wrong
        // everywhere it was copied. ModelSetupChecker adds a named error for a category that
        // matches no budget column, RunSetupChecksStage1 runs unconditionally, and the run
        // throws - loudly, at setup, naming the treatment. A blank category is caught the same
        // way. The unchecked case is elsewhere: keys passed to AssignBudgetCategoryFractions at
        // run time, which no static check can see.
        ToolResult result = Run("blank-budget-category");

        Assert.Contains("No budget_category on: RMaint", result.Output, StringComparison.Ordinal);
        Assert.Contains("fails at setup", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Two_project_files_are_reported_rather_than_refused()
    {
        // check is the FIRST thing somebody runs on a model they have inherited. Refusing to look
        // at the most obviously broken case would be exactly backwards, so this one reports and
        // carries on where rename and package refuse.
        ToolResult result = Run("two-csproj");

        Assert.Equal(ExitCode.CheckFailed, result.ExitCode);
        Assert.Contains("one .csproj at the root", result.Output, StringComparison.Ordinal);
        Assert.Contains("bundle structure", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Rules_that_could_not_be_applied_are_skipped_rather_than_passed()
    {
        // A check that quietly becomes a no-op is worse than no check, because it reports the
        // same green as one that ran.
        ToolResult result = Run("healthy");

        Assert.Contains("SKIPPED", result.Output, StringComparison.Ordinal);
        Assert.Contains("lookup sets", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Lookup_sets_are_checked_when_a_lookups_file_is_given()
    {
        string lookups = Path.Combine(
            TestBundle.RepoRoot(), "reference-model", "sample-inputs", "lookups.xlsx");

        ToolResult result = Run("healthy", "--lookups", lookups);

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("lookup sets", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("SKIPPED   no lookups.xlsx given", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void The_output_says_it_is_a_local_subset()
    {
        // Locked decision 6. If this ever stops being said out loud, somebody will read a green
        // check as "this will run", and the web app's Check Setup is the authority on that.
        ToolResult result = Run("healthy");

        Assert.Contains("LOCAL SUBSET", result.Output, StringComparison.Ordinal);
        Assert.Contains("Check Setup", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_commented_out_case_arm_does_not_count_as_handling_a_treatment()
    {
        // The scaffolder's stubs are full of commented-out examples. Reading one of those as real
        // code would pass a model that handles nothing at all.
        using TemporaryModel model = FixtureModels.Copy("healthy");

        string resetter = model.PathTo("Objects", "Resetter.cs");
        string text = File.ReadAllText(resetter);
        text = text.Replace("case TreatmentNames.Repair:", "// case TreatmentNames.Repair:", StringComparison.Ordinal);
        File.WriteAllText(resetter, text);

        ToolResult result = model.Run("check", "--project", model.Folder);

        Assert.Equal(ExitCode.CheckFailed, result.ExitCode);
        Assert.Contains("reset switch for: repair", result.Output, StringComparison.Ordinal);
    }

    private static void SetNetworkFunction(TemporaryModel model, string input, string output)
    {
        string bundle = model.PathTo("domain_model_setup.xlsx");
        DumpTests.SetCell(bundle, "network_functions", row: 2, column: 1, value: input);
        DumpTests.SetCell(bundle, "network_functions", row: 2, column: 2, value: "percent_rank");
        DumpTests.SetCell(bundle, "network_functions", row: 2, column: 3, value: output);
    }

    private static ToolResult Run(string fixture, params string[] extra)
    {
        return RunArgs(new[] { "check", "--project", FixtureModels.PathTo(fixture) }.Concat(extra).ToArray());
    }

    /// <summary>Runs the tool with exactly these arguments, for tests about the command line itself.</summary>
    private static ToolResult RunArgs(params string[] args)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        int exitCode = JcassDm.Program.Run(args, output, error);
        return new ToolResult(exitCode, output.ToString(), error.ToString());
    }
}
