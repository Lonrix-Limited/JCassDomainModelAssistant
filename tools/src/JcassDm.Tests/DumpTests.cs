using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using JcassDm.Cli;
using Xunit;

namespace JcassDm.Tests;

/// <summary>
/// <c>dump</c> is the verb everything else is checked with, so it is checked first and
/// hardest. If dump is wrong, every other test in this project is asserting against a lie.
/// </summary>
public class DumpTests
{
    [Fact]
    public void Prints_all_five_sheets_with_their_rows()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run("dump", bundle.Path);

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("[meta]", result.Output, StringComparison.Ordinal);
        Assert.Contains("[input_headers]", result.Output, StringComparison.Ordinal);
        Assert.Contains("[parameters]", result.Output, StringComparison.Ordinal);
        Assert.Contains("[treatments]", result.Output, StringComparison.Ordinal);
        Assert.Contains("[network_functions]", result.Output, StringComparison.Ordinal);

        Assert.Contains("main_dll | DomainModelSample.dll", result.Output, StringComparison.Ordinal);
        Assert.Contains("par_cond_rating | number | 0 | 1500 | 4 |", result.Output, StringComparison.Ordinal);
        Assert.Contains("repair | repair | repair |", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Sheets_come_out_in_the_framework_order_every_time()
    {
        using var bundle = TestBundle.FromReferenceModel();

        string output = bundle.Dump();
        int[] positions = new[] { "[meta]", "[input_headers]", "[parameters]", "[treatments]", "[network_functions]" }
            .Select(marker => output.IndexOf(marker, StringComparison.Ordinal))
            .ToArray();

        Assert.All(positions, position => Assert.True(position >= 0));
        Assert.Equal(positions.OrderBy(p => p).ToArray(), positions);
    }

    [Fact]
    public void Two_dumps_of_the_same_file_are_identical()
    {
        // The whole value of dump rests on this. If it is not deterministic, a diff of two
        // dumps is noise and nobody will use it twice.
        using var bundle = TestBundle.FromReferenceModel();

        Assert.Equal(bundle.Dump(), bundle.Dump());
    }

    [Fact]
    public void A_header_only_sheet_is_valid_and_says_so()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run("dump", bundle.Path, "--sheet", "network_functions");

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("(header row only, no data rows)", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Sheet_filter_shows_only_that_sheet()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run("dump", bundle.Path, "--sheet", "treatments");

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("[treatments]", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("[parameters]", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void An_unknown_sheet_name_is_a_usage_error_that_lists_the_real_ones()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run("dump", bundle.Path, "--sheet", "Treatments");

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Contains("no sheet called 'Treatments'", result.All, StringComparison.Ordinal);
        Assert.Contains("treatments", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void Numbers_are_rendered_the_same_on_any_machine()
    {
        // 19.1 must not come back as "19.100000000000001" or, on a comma-decimal machine,
        // as "19,1" - the second would make every dump on that machine differ from every
        // dump on ours.
        using var bundle = TestBundle.FromReferenceModel();

        Assert.Contains("| 19.1 |", bundle.Dump(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_missing_sheet_is_reported_by_name_and_exits_bundle_invalid()
    {
        using var bundle = TestBundle.FromReferenceModel();
        RemoveSheet(bundle.Path, "parameters");

        ToolResult result = bundle.Run("dump", bundle.Path);

        Assert.Equal(ExitCode.BundleInvalid, result.ExitCode);
        Assert.Contains("Required sheet 'parameters' is missing", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void A_sheet_whose_name_differs_only_in_case_is_still_missing_and_the_near_miss_is_named()
    {
        using var bundle = TestBundle.FromReferenceModel();
        RenameSheet(bundle.Path, "treatments", "Treatments");

        ToolResult result = bundle.Run("dump", bundle.Path);

        Assert.Equal(ExitCode.BundleInvalid, result.ExitCode);
        Assert.Contains("Required sheet 'treatments' is missing", result.All, StringComparison.Ordinal);
        Assert.Contains("'Treatments'", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void A_name_with_a_trailing_space_is_warned_about()
    {
        // Identical on screen to the name in the C#, and matches nothing. Exactly the class
        // of failure a binary file hides.
        using var bundle = TestBundle.FromReferenceModel();
        SetCell(bundle.Path, "treatments", row: 2, column: 1, value: "repair ");

        ToolResult result = bundle.Run("dump", bundle.Path);

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("leading or trailing space", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Content_stranded_below_the_blank_row_is_reported_rather_than_shown_as_data()
    {
        using var bundle = TestBundle.FromReferenceModel();
        // Row 5 is the blank that ends the treatments data; row 6 is below it.
        SetCell(bundle.Path, "treatments", row: 6, column: 1, value: "orphan");

        ToolResult result = bundle.Run("dump", bundle.Path);

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("row 6 has content below the blank row", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("6 | orphan", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void An_unknown_verb_is_a_usage_error()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run("dumpp", bundle.Path);

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Contains("Unknown command 'dumpp'", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void A_missing_file_is_a_bundle_error_naming_the_path()
    {
        using var bundle = TestBundle.FromReferenceModel();
        string missing = bundle.PathFor("not_here.xlsx");

        ToolResult result = bundle.Run("dump", missing);

        Assert.Equal(ExitCode.BundleInvalid, result.ExitCode);
        Assert.Contains(missing, result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void No_arguments_prints_the_help()
    {
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run();

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("THE MODEL", result.Output, StringComparison.Ordinal);
        Assert.Contains("THE BUNDLE", result.Output, StringComparison.Ordinal);
        Assert.Contains("EXIT CODES", result.Output, StringComparison.Ordinal);

        // Every verb the dispatcher accepts appears here. The help is the only place an agent
        // finds out what the tool can do, so a verb that ships without a line here is a verb
        // nobody calls.
        foreach (string verb in new[]
                 {
                     "scaffold", "rename", "check", "package",
                     "dump", "set-meta", "add-treatment", "add-parameter", "add-input-header",
                 })
        {
            Assert.Contains("  " + verb + " ", result.Output, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void The_help_explains_the_four_name_rule()
    {
        // The single failure the whole tool is organised around. If somebody reads only the help,
        // this is the thing they have to come away with.
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run();

        Assert.Contains("THE FOUR NAMES", result.Output, StringComparison.Ordinal);
        Assert.Contains("was not found in the specified .dll", result.Output, StringComparison.Ordinal);
    }

    // -----------------------------------------------------------------------------
    // Workbook surgery for the broken-bundle cases
    // -----------------------------------------------------------------------------

    internal static void RemoveSheet(string path, string sheetName)
        => Edit(path, workbook => workbook.Worksheet(sheetName).Delete());

    internal static void RenameSheet(string path, string from, string to)
        => Edit(path, workbook => workbook.Worksheet(from).Name = to);

    internal static void SetCell(string path, string sheetName, int row, int column, string value)
        => Edit(path, workbook => workbook.Worksheet(sheetName).Cell(row, column).Value = value);

    internal static void RemoveColumn(string path, string sheetName, int column)
        => Edit(path, workbook => workbook.Worksheet(sheetName).Column(column).Delete());

    private static void Edit(string path, Action<XLWorkbook> change)
    {
        string temporary = path + ".tmp.xlsx";
        using (var workbook = new XLWorkbook(path))
        {
            change(workbook);
            workbook.SaveAs(temporary);
        }
        File.Delete(path);
        File.Move(temporary, path);
    }
}
