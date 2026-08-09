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
    public void Every_broken_fixture_is_caught(string fixture)
    {
        ToolResult result = Run(fixture);

        Assert.Equal(ExitCode.CheckFailed, result.ExitCode);
        Assert.Contains("PROBLEM", result.Output, StringComparison.Ordinal);
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
    public void A_missing_parameter_is_named()
    {
        ToolResult result = Run("parameter-not-written");

        Assert.Contains("par_obj", result.Output, StringComparison.Ordinal);
        Assert.Contains("SetParameterValues", result.Output, StringComparison.Ordinal);
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

    private static ToolResult Run(string fixture, params string[] extra)
    {
        var args = new[] { "check", "--project", FixtureModels.PathTo(fixture) }.Concat(extra).ToArray();

        var output = new StringWriter();
        var error = new StringWriter();
        int exitCode = JcassDm.Program.Run(args, output, error);
        return new ToolResult(exitCode, output.ToString(), error.ToString());
    }
}
