using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using JcassDm.Cli;
using Xunit;

namespace JcassDm.Tests;

/// <summary>
/// Dump, write, dump again - and check that the second dump differs from the first on
/// exactly the lines that were meant to change.
///
/// <para>This is the test that would catch a whole class of quiet damage: a rewritten sheet,
/// a lost row, a number reformatted, a value re-encoded. None of those would fail any of the
/// per-verb tests, and all of them would show up here as extra diff lines.</para>
/// </summary>
public class RoundTripTests
{
    [Fact]
    public void Adding_a_treatment_adds_exactly_one_line_to_the_dump()
    {
        using var bundle = TestBundle.FromReferenceModel();
        string[] before = Lines(bundle.Dump());

        Assert.Equal(ExitCode.Ok, bundle.Run(
            "add-treatment", bundle.Path,
            "--name", "reseal", "--budget-category", "resurfacing",
            "--description", "Reseal the surface").ExitCode);

        string[] after = Lines(bundle.Dump());

        string[] added = after.Except(before).ToArray();
        string[] lost = before.Except(after).ToArray();

        Assert.Empty(lost);
        Assert.Single(added);
        Assert.Contains("reseal", added[0], StringComparison.Ordinal);
    }

    [Fact]
    public void Adding_a_parameter_adds_exactly_one_line_to_the_dump()
    {
        using var bundle = TestBundle.FromReferenceModel();
        string[] before = Lines(bundle.Dump());

        Assert.Equal(ExitCode.Ok, bundle.Run(
            "add-parameter", bundle.Path,
            "--name", "par_iri", "--min", "0", "--max", "10", "--decimals", "2").ExitCode);

        string[] after = Lines(bundle.Dump());

        Assert.Empty(before.Except(after));
        Assert.Single(after.Except(before));
    }

    [Fact]
    public void Adding_an_input_header_adds_exactly_one_line_to_the_dump()
    {
        using var bundle = TestBundle.FromReferenceModel();
        string[] before = Lines(bundle.Dump());

        Assert.Equal(ExitCode.Ok, bundle.Run(
            "add-input-header", bundle.Path, "--column", "traffic_count", "--type", "number").ExitCode);

        string[] after = Lines(bundle.Dump());

        Assert.Empty(before.Except(after));
        Assert.Single(after.Except(before));
    }

    [Fact]
    public void Setting_the_meta_sheet_changes_only_the_meta_lines()
    {
        using var bundle = TestBundle.FromReferenceModel();
        string[] before = Lines(bundle.Dump());

        Assert.Equal(ExitCode.Ok, bundle.Run(
            "set-meta", bundle.Path,
            "--main-dll", "MyRoadModel.dll", "--main-class", "MyRoadModel",
            "--display-name", "My Road Model", "--force").ExitCode);

        string[] after = Lines(bundle.Dump());

        string[] lost = before.Except(after).ToArray();
        string[] added = after.Except(before).ToArray();

        Assert.Equal(3, lost.Length);
        Assert.Equal(3, added.Length);
        Assert.All(lost, line => Assert.Matches(@"^\d+ \| (main_dll|main_class|model_name) \|", line));
        Assert.All(added, line => Assert.Matches(@"^\d+ \| (main_dll|main_class|model_name) \|", line));
    }

    [Fact]
    public void Several_writes_in_a_row_each_add_one_line_and_disturb_nothing_else()
    {
        // The realistic sequence: an agent adds an input column, the parameter that carries
        // it, and the treatment that uses it, in three separate calls.
        using var bundle = TestBundle.FromReferenceModel();
        string[] before = Lines(bundle.Dump());

        Assert.Equal(ExitCode.Ok, bundle.Run(
            "add-input-header", bundle.Path, "--column", "traffic_count", "--type", "number").ExitCode);
        Assert.Equal(ExitCode.Ok, bundle.Run(
            "add-parameter", bundle.Path, "--name", "par_traffic", "--min", "0", "--max", "100000").ExitCode);
        Assert.Equal(ExitCode.Ok, bundle.Run(
            "add-treatment", bundle.Path, "--name", "reseal", "--budget-category", "resurfacing").ExitCode);

        string[] after = Lines(bundle.Dump());

        Assert.Empty(before.Except(after));
        Assert.Equal(3, after.Except(before).Count());
    }

    [Fact]
    public void A_write_that_changes_nothing_does_not_touch_the_file()
    {
        // Re-running an add that is already correct must not rewrite the workbook. If it
        // did, every idempotent re-run would show up as a modified binary in git.
        using var bundle = TestBundle.FromReferenceModel();
        Assert.Equal(ExitCode.Ok, bundle.Run(
            "add-treatment", bundle.Path, "--name", "reseal", "--budget-category", "resurfacing").ExitCode);

        byte[] afterFirstWrite = Hash(bundle.Path);

        Assert.Equal(ExitCode.Ok, bundle.Run(
            "add-treatment", bundle.Path, "--name", "reseal", "--budget-category", "resurfacing").ExitCode);

        Assert.Equal(afterFirstWrite, Hash(bundle.Path));
    }

    private static string[] Lines(string dump)
        => dump.Split('\n').Select(line => line.TrimEnd('\r')).Where(line => line.Length > 0).ToArray();

    private static byte[] Hash(string path) => SHA256.HashData(File.ReadAllBytes(path));
}
