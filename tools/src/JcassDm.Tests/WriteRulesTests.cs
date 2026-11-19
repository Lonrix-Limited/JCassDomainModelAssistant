using System;
using System.Linq;
using JcassDm.Cli;
using Xunit;

namespace JcassDm.Tests;

/// <summary>
/// The two rules that are not negotiable, checked for every write verb rather than for one
/// of them. They are cross-cutting promises, so they are worth cross-cutting tests: the way
/// they get broken in practice is a new verb that forgets one.
/// </summary>
public class WriteRulesTests
{
    /// <summary>Each verb, with arguments that add a row, and the same arguments with one value changed.</summary>
    public static TheoryData<string[], string[]> EveryWriteVerb() => new()
    {
        {
            new[] { "add-treatment", "--name", "reseal", "--budget-category", "resurfacing" },
            new[] { "add-treatment", "--name", "reseal", "--budget-category", "maintenance" }
        },
        {
            new[] { "add-parameter", "--name", "par_iri", "--min", "0", "--max", "10" },
            new[] { "add-parameter", "--name", "par_iri", "--min", "0", "--max", "20" }
        },
        {
            new[] { "add-input-header", "--column", "traffic_count", "--type", "number" },
            new[] { "add-input-header", "--column", "traffic_count", "--type", "text" }
        },
        {
            // set-meta is the odd one out: its rows exist in every bundle, so establishing
            // the starting state is itself an overwrite and needs --force.
            new[] { "set-meta", "--main-class", "MyRoadModel", "--force" },
            new[] { "set-meta", "--main-class", "OtherModel" }
        },
    };

    [Theory]
    [MemberData(nameof(EveryWriteVerb))]
    public void Running_the_same_write_twice_changes_nothing_the_second_time(string[] add, string[] _)
    {
        // An agent WILL run it twice - after a failed build, after losing its scrollback,
        // after simply not being sure the first one worked.
        using var bundle = TestBundle.FromReferenceModel();

        Assert.Equal(ExitCode.Ok, bundle.Run(WithBundle(add, bundle)).ExitCode);
        string afterFirst = bundle.Dump();

        ToolResult second = bundle.Run(WithBundle(add, bundle));

        Assert.Equal(ExitCode.Ok, second.ExitCode);
        Assert.Contains("unchanged", second.Output, StringComparison.Ordinal);
        Assert.Contains("Nothing written", second.Output, StringComparison.Ordinal);
        Assert.Equal(afterFirst, bundle.Dump());
    }

    [Theory]
    [MemberData(nameof(EveryWriteVerb))]
    public void A_different_value_for_an_existing_row_is_refused_and_nothing_is_written(string[] add, string[] conflicting)
    {
        using var bundle = TestBundle.FromReferenceModel();
        Assert.Equal(ExitCode.Ok, bundle.Run(WithBundle(add, bundle)).ExitCode);
        string before = bundle.Dump();

        ToolResult result = bundle.Run(WithBundle(conflicting, bundle));

        Assert.Equal(ExitCode.Conflict, result.ExitCode);
        Assert.Contains("Nothing was written", result.All, StringComparison.Ordinal);
        Assert.Contains("--force", result.All, StringComparison.Ordinal);
        Assert.Equal(before, bundle.Dump());
    }

    [Theory]
    [MemberData(nameof(EveryWriteVerb))]
    public void Force_overwrites_and_reports_what_changed(string[] add, string[] conflicting)
    {
        using var bundle = TestBundle.FromReferenceModel();
        Assert.Equal(ExitCode.Ok, bundle.Run(WithBundle(add, bundle)).ExitCode);
        string before = bundle.Dump();

        ToolResult result = bundle.Run(WithBundle(conflicting, bundle).Append("--force").ToArray());

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("updated", result.Output, StringComparison.Ordinal);
        Assert.NotEqual(before, bundle.Dump());
    }

    [Theory]
    [MemberData(nameof(EveryWriteVerb))]
    public void A_mistyped_option_is_refused_rather_than_ignored(string[] add, string[] _)
    {
        // The failure this prevents: --budget_category silently ignored, leaving a treatment
        // with a blank budget category, which throws at setup on the next run - loudly, but
        // long after the engineer typed the option and moved on.
        using var bundle = TestBundle.FromReferenceModel();
        string before = bundle.Dump();

        ToolResult result = bundle.Run(WithBundle(add, bundle).Append("--budget_category").Append("x").ToArray());

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Contains("Unrecognised option", result.All, StringComparison.Ordinal);
        Assert.Equal(before, bundle.Dump());
    }

    [Theory]
    [MemberData(nameof(EveryWriteVerb))]
    public void A_bundle_missing_a_sheet_is_refused_by_name(string[] add, string[] _)
    {
        using var bundle = TestBundle.FromReferenceModel();
        DumpTests.RemoveSheet(bundle.Path, "network_functions");

        ToolResult result = bundle.Run(WithBundle(add, bundle));

        Assert.Equal(ExitCode.BundleInvalid, result.ExitCode);
        Assert.Contains("network_functions", result.All, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(EveryWriteVerb))]
    public void Every_sheet_the_verb_did_not_touch_comes_through_unchanged(string[] add, string[] _)
    {
        using var bundle = TestBundle.FromReferenceModel();
        WorkbookSnapshot before = WorkbookSnapshot.Take(bundle.Path);

        Assert.Equal(ExitCode.Ok, bundle.Run(WithBundle(add, bundle)).ExitCode);

        WorkbookSnapshot after = WorkbookSnapshot.Take(bundle.Path);
        before.AssertOnlyDifferenceIsAppendedRows(after);
    }

    [Fact]
    public void A_row_with_an_empty_first_cell_is_not_written_over()
    {
        // The next appended row lands exactly here, so it would overwrite what the row holds.
        using var bundle = TestBundle.FromReferenceModel();
        DumpTests.SetCell(bundle.Path, "treatments", row: 5, column: 2, value: "resurfacing");
        string before = bundle.Dump();

        ToolResult result = bundle.Run(
            "add-treatment", bundle.Path, "--name", "reseal", "--budget-category", "resurfacing");

        Assert.Equal(ExitCode.BundleInvalid, result.ExitCode);
        Assert.Contains("row 5", result.All, StringComparison.Ordinal);
        Assert.Equal(before, bundle.Dump());
    }

    [Fact]
    public void A_note_to_the_right_of_the_table_does_not_block_a_new_row()
    {
        // A new row only writes the table's columns, so it can never reach this note.
        using var bundle = TestBundle.FromReferenceModel();
        DumpTests.SetCell(bundle.Path, "treatments", row: 8, column: 20, value: "a note");

        ToolResult result = bundle.Run(
            "add-treatment", bundle.Path, "--name", "reseal", "--budget-category", "resurfacing");

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("added", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Content_in_a_later_table_column_below_the_blank_row_still_blocks_a_new_row()
    {
        // The control for the test above: new rows could reach this cell.
        using var bundle = TestBundle.FromReferenceModel();
        DumpTests.SetCell(bundle.Path, "treatments", row: 6, column: 2, value: "orphan");
        string before = bundle.Dump();

        ToolResult result = bundle.Run(
            "add-treatment", bundle.Path, "--name", "reseal", "--budget-category", "resurfacing");

        Assert.Equal(ExitCode.BundleInvalid, result.ExitCode);
        Assert.Contains("row 6", result.All, StringComparison.Ordinal);
        Assert.Equal(before, bundle.Dump());
    }

    private static string[] WithBundle(string[] args, TestBundle bundle)
        => new[] { args[0], bundle.Path }.Concat(args.Skip(1)).ToArray();
}
