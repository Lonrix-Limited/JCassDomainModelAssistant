using System;
using System.Linq;
using JcassDm.Cli;
using Xunit;

namespace JcassDm.Tests;

/// <summary>What each verb writes, and what it refuses to write.</summary>
public class VerbTests
{
    // -----------------------------------------------------------------------------
    // set-meta
    // -----------------------------------------------------------------------------

    [Fact]
    public void SetMeta_writes_all_three_settings()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run(
            "set-meta", bundle.Path,
            "--main-dll", "MyRoadModel.dll",
            "--main-class", "MyRoadModel",
            "--display-name", "My Road Model",
            "--force");

        Assert.Equal(ExitCode.Ok, result.ExitCode);

        string dump = bundle.Dump();
        Assert.Contains("main_dll | MyRoadModel.dll", dump, StringComparison.Ordinal);
        Assert.Contains("main_class | MyRoadModel", dump, StringComparison.Ordinal);
        Assert.Contains("model_name | My Road Model", dump, StringComparison.Ordinal);
    }

    [Fact]
    public void SetMeta_writes_all_three_or_none_of_them()
    {
        // Half a rename is worse than none: the bundle would load the right assembly and
        // then fail to find the class in it.
        using var bundle = TestBundle.FromReferenceModel();
        string before = bundle.Dump();

        ToolResult result = bundle.Run(
            "set-meta", bundle.Path,
            "--main-dll", "DomainModelSample.dll",   // already correct
            "--main-class", "SomethingElse");        // conflicts

        Assert.Equal(ExitCode.Conflict, result.ExitCode);
        Assert.Equal(before, bundle.Dump());
    }

    [Fact]
    public void SetMeta_adds_a_setting_the_sheet_does_not_have_yet()
    {
        using var bundle = TestBundle.FromReferenceModel();
        DumpTests.SetCell(bundle.Path, "meta", row: 4, column: 1, value: "");   // drop model_name

        ToolResult result = bundle.Run("set-meta", bundle.Path, "--display-name", "Restored");

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("model_name | Restored", bundle.Dump(), StringComparison.Ordinal);
    }

    [Fact]
    public void SetMeta_needs_at_least_one_setting()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run("set-meta", bundle.Path);

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Contains("at least one", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void SetMeta_refuses_a_main_dll_without_the_extension()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run("set-meta", bundle.Path, "--main-dll", "MyRoadModel");

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Contains("MyRoadModel.dll", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void SetMeta_refuses_a_main_class_that_looks_like_a_file_name()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run("set-meta", bundle.Path, "--main-class", "MyRoadModel.dll");

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Contains("drop the .dll", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void SetMeta_points_out_a_dll_and_class_that_cannot_both_be_right()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run(
            "set-meta", bundle.Path,
            "--main-dll", "MyRoadModel.dll", "--main-class", "SomethingElse", "--force");

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("note", result.Output, StringComparison.Ordinal);
        Assert.Contains(".csproj", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void SetMeta_accepts_model_name_as_an_alias_for_display_name()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run("set-meta", bundle.Path, "--model-name", "Aliased", "--force");

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("model_name | Aliased", bundle.Dump(), StringComparison.Ordinal);
    }

    // -----------------------------------------------------------------------------
    // add-treatment
    // -----------------------------------------------------------------------------

    [Fact]
    public void AddTreatment_appends_a_row_and_leaves_the_existing_ones_alone()
    {
        using var bundle = TestBundle.FromReferenceModel();
        WorkbookSnapshot before = WorkbookSnapshot.Take(bundle.Path);

        ToolResult result = bundle.Run(
            "add-treatment", bundle.Path,
            "--name", "reseal", "--budget-category", "resurfacing",
            "--description", "Reseal the surface", "--comments", "See TreatmentTrigger.cs");

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        before.AssertExistingCellsUnchanged(WorkbookSnapshot.Take(bundle.Path), "treatments");
        Assert.Contains(
            "reseal | reseal | resurfacing | Reseal the surface | See TreatmentTrigger.cs",
            bundle.Dump(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void AddTreatment_defaults_category_to_the_treatment_name()
    {
        using var bundle = TestBundle.FromReferenceModel();

        bundle.Run("add-treatment", bundle.Path, "--name", "reseal", "--budget-category", "resurfacing");

        Assert.Contains("| reseal | reseal | resurfacing |", bundle.Dump(), StringComparison.Ordinal);
    }

    [Fact]
    public void AddTreatment_names_the_C_sharp_work_it_cannot_do()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run(
            "add-treatment", bundle.Path, "--name", "reseal", "--budget-category", "resurfacing");

        Assert.Contains("TreatmentNames.cs", result.Output, StringComparison.Ordinal);
        Assert.Contains("budgets.xlsx", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void AddTreatment_refuses_a_name_with_a_trailing_space()
    {
        using var bundle = TestBundle.FromReferenceModel();
        string before = bundle.Dump();

        ToolResult result = bundle.Run(
            "add-treatment", bundle.Path, "--name", "reseal ", "--budget-category", "resurfacing");

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Contains("leading or trailing space", result.All, StringComparison.Ordinal);
        Assert.Equal(before, bundle.Dump());
    }

    [Fact]
    public void AddTreatment_refuses_a_column_the_sheet_does_not_have()
    {
        using var bundle = TestBundle.FromReferenceModel();
        DumpTests.RemoveColumn(bundle.Path, "treatments", column: 5);   // 'comments'

        ToolResult result = bundle.Run(
            "add-treatment", bundle.Path,
            "--name", "reseal", "--budget-category", "resurfacing", "--comments", "nope");

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Contains("no column 'comments'", result.All, StringComparison.Ordinal);
        Assert.Contains("does not add columns", result.All, StringComparison.Ordinal);
    }

    // -----------------------------------------------------------------------------
    // add-parameter
    // -----------------------------------------------------------------------------

    [Fact]
    public void AddParameter_appends_a_row()
    {
        using var bundle = TestBundle.FromReferenceModel();
        WorkbookSnapshot before = WorkbookSnapshot.Take(bundle.Path);

        ToolResult result = bundle.Run(
            "add-parameter", bundle.Path,
            "--name", "par_iri", "--min", "0", "--max", "10", "--decimals", "2",
            "--comment", "Roughness");

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        before.AssertExistingCellsUnchanged(WorkbookSnapshot.Take(bundle.Path), "parameters");
        Assert.Contains("par_iri | number | 0 | 10 | 2 | Roughness", bundle.Dump(), StringComparison.Ordinal);
    }

    [Fact]
    public void AddParameter_insists_on_min_and_max_for_a_number()
    {
        // Because the framework clamps to them rather than validating against them, a
        // defaulted 0/0 would flatten the parameter for the whole run with no error.
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run("add-parameter", bundle.Path, "--name", "par_iri");

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Contains("CLAMPS", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void AddParameter_refuses_a_range_that_is_the_wrong_way_round()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run(
            "add-parameter", bundle.Path, "--name", "par_iri", "--min", "10", "--max", "0");

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Contains("clamped", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void AddParameter_accepts_a_text_parameter_without_a_range()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run(
            "add-parameter", bundle.Path, "--name", "par_surface", "--type", "text");

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("par_surface | text | 0 | 0 |", bundle.Dump(), StringComparison.Ordinal);
    }

    [Fact]
    public void AddParameter_mentions_the_par_prefix_convention_without_enforcing_it()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run(
            "add-parameter", bundle.Path, "--name", "roughness", "--min", "0", "--max", "10");

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("'par_'", result.Output, StringComparison.Ordinal);
        Assert.Contains("roughness | number", bundle.Dump(), StringComparison.Ordinal);
    }

    [Fact]
    public void AddParameter_says_every_parameter_must_be_written_in_SetParameterValues()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run(
            "add-parameter", bundle.Path, "--name", "par_iri", "--min", "0", "--max", "10");

        Assert.Contains("SetParameterValues", result.Output, StringComparison.Ordinal);
    }

    // -----------------------------------------------------------------------------
    // add-input-header
    // -----------------------------------------------------------------------------

    [Fact]
    public void AddInputHeader_appends_a_row_with_a_numeric_example()
    {
        using var bundle = TestBundle.FromReferenceModel();
        WorkbookSnapshot before = WorkbookSnapshot.Take(bundle.Path);

        ToolResult result = bundle.Run(
            "add-input-header", bundle.Path,
            "--column", "traffic_count", "--type", "number",
            "--example", "1250", "--comment", "Annual average daily traffic");

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        before.AssertExistingCellsUnchanged(WorkbookSnapshot.Take(bundle.Path), "input_headers");
        Assert.Contains(
            "general | traffic_count | number | 1250 | Annual average daily traffic",
            bundle.Dump(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void AddInputHeader_requires_a_type_the_framework_understands()
    {
        // 'numeric' is the plausible wrong answer, and the framework would treat it as a
        // number rather than rejecting it - so the tool has to.
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run(
            "add-input-header", bundle.Path, "--column", "traffic_count", "--type", "numeric");

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Contains("number, text", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void AddInputHeader_warns_that_a_numeric_column_cannot_be_blank()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run(
            "add-input-header", bundle.Path, "--column", "traffic_count", "--type", "number");

        Assert.Contains("sentinel", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void AddInputHeader_names_both_factory_methods()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run(
            "add-input-header", bundle.Path, "--column", "surface_type", "--type", "text");

        Assert.Contains("BOTH factory methods", result.Output, StringComparison.Ordinal);
        Assert.Contains("textInputs[\"surface_type\"]", result.Output, StringComparison.Ordinal);
    }
}
