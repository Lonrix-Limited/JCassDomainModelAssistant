using System;
using System.IO;
using JcassDm.Cli;
using JcassDm.Project;
using JcassDm.Verbs;
using Xunit;

namespace JcassDm.Tests;

/// <summary>
/// <c>check</c>'s target-framework rule: does the model target the .NET the Assistant's reference
/// assemblies were built for?
///
/// <para><b>It fails the check rather than noting it</b>, unlike the framework-reference rule beside
/// it. A model on an older .NET than its references cannot build - the compiler stops with CS1705 -
/// and a note would let an agent carry on into that error and start editing references by hand.</para>
///
/// <para><b>The expected target is read from the Assistant's <c>refs\FRAMEWORK-VERSION.txt</c></b>,
/// never written into the tool, so the next .NET move needs a refresh of <c>refs\</c> and nothing
/// else. One test below proves that by stamping a target that does not exist yet.</para>
/// </summary>
public class TargetFrameworkTests
{
    private const string Rule = "target framework";

    [Fact]
    public void A_model_on_an_older_target_fails_the_check_and_names_the_rule()
    {
        ToolResult result = Check("target-framework-behind");

        Assert.Equal(ExitCode.CheckFailed, result.ExitCode);
        Assert.Contains("PROBLEM: " + Rule, result.Output, StringComparison.Ordinal);
        Assert.Contains("net9.0", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void The_message_says_published_models_keep_running_and_how_to_fix_it()
    {
        // What the engineer reads. Each phrase answers a question they would otherwise email
        // support with: is my live model broken, do I need to install anything, what do I change.
        ToolResult result = Check("target-framework-behind");

        Assert.Contains("keeps", result.Output, StringComparison.Ordinal);
        Assert.Contains("--list-sdks", result.Output, StringComparison.Ordinal);
        Assert.Contains("CS1705", result.Output, StringComparison.Ordinal);
        Assert.Contains("refresh-model-refs.ps1", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_model_on_the_same_target_as_the_references_passes()
    {
        ToolResult result = Check("healthy");

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("same as this Assistant's references", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void The_expected_target_comes_from_the_stamp_not_from_the_tool()
    {
        // A target no Assistant has shipped yet. If the tool had net10.0 written into it, the
        // healthy model would pass here; it must fail, naming the stamp's target.
        using TemporaryModel model = FixtureModels.Copy("healthy");
        using TemporaryModel refs = AssistantRefs("Build            : Release, net11.0");

        string output = RunRule(model, refs.Folder, out bool failed);

        Assert.True(failed);
        Assert.Contains(".NET 11 (net11.0)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void No_stamp_is_skipped_not_passed()
    {
        using TemporaryModel model = FixtureModels.Copy("target-framework-behind");
        using TemporaryModel refs = TemporaryModel.Empty();

        string output = RunRule(model, refs.Folder, out bool failed);

        Assert.False(failed);
        Assert.Contains("SKIPPED", output, StringComparison.Ordinal);
        Assert.Contains(FrameworkStamp.StampFileName, output, StringComparison.Ordinal);
        Assert.DoesNotContain("OK", output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_stamp_whose_build_line_names_no_target_is_skipped()
    {
        using TemporaryModel model = FixtureModels.Copy("target-framework-behind");
        using TemporaryModel refs = AssistantRefs("Build            : Release");

        string output = RunRule(model, refs.Folder, out bool failed);

        Assert.False(failed);
        Assert.Contains("SKIPPED", output, StringComparison.Ordinal);
    }

    [Fact]
    public void No_assistant_refs_folder_is_skipped_and_says_why()
    {
        using TemporaryModel model = FixtureModels.Copy("target-framework-behind");

        string output = RunRule(model, assistantRefs: null, out bool failed);

        Assert.False(failed);
        Assert.Contains("SKIPPED", output, StringComparison.Ordinal);
        Assert.Contains("copied out on its own", output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_project_with_no_single_target_is_skipped()
    {
        using TemporaryModel model = FixtureModels.Copy("healthy");
        string csproj = model.PathTo("FixtureModel.csproj");
        File.WriteAllText(csproj, File.ReadAllText(csproj).Replace(
            "<TargetFramework>net10.0</TargetFramework>", "<TargetFrameworks>net9.0;net10.0</TargetFrameworks>"));
        using TemporaryModel refs = AssistantRefs("Build            : Release, net10.0");

        string output = RunRule(model, refs.Folder, out bool failed);

        Assert.False(failed);
        Assert.Contains("SKIPPED", output, StringComparison.Ordinal);
        Assert.Contains("<TargetFramework>net10.0</TargetFramework>", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Old_references_and_an_old_target_are_both_reported()
    {
        // The engineer who downloaded the new Assistant and has done nothing yet. They need both
        // findings: the target line to change, and the refs folder to refresh.
        using TemporaryModel model = FixtureModels.Copy("target-framework-behind");
        FrameworkReferenceTests.SeedRefs(model, patchCommit: "a1b2c3d4e5f60718293a4b5c6d7e8f9012345678");

        ToolResult result = model.Run("check", "--project", model.Folder);

        Assert.Equal(ExitCode.CheckFailed, result.ExitCode);
        Assert.Contains("PROBLEM: " + Rule, result.Output, StringComparison.Ordinal);
        Assert.Contains("NOTE: framework reference", result.Output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("net10.0", ".NET 10")]
    [InlineData("net9.0", ".NET 9")]
    [InlineData("net8.1", ".NET 8.1")]
    [InlineData("netstandard2.0", "netstandard2.0")]
    public void Targets_are_described_in_the_words_an_engineer_knows(string tfm, string expected)
    {
        Assert.Equal(expected, FrameworkStamp.DescribeTarget(tfm));
    }

    [Fact]
    public void The_build_target_is_read_from_a_real_stamp_layout()
    {
        // The stamp as the framework maintainers' script writes it, BOM and all.
        using TemporaryModel refs = AssistantRefs(
            "Framework commit : 0123456789abcdef0123456789abcdef01234567\nBuild            : Release, net10.0",
            withBom: true);

        Assert.Equal("net10.0", FrameworkStamp.ReadBuildTarget(refs.Folder));
    }

    // ---------------------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------------------

    private static ToolResult Check(string fixture)
    {
        using TemporaryModel model = FixtureModels.Copy(fixture);
        return model.Run("check", "--project", model.Folder);
    }

    /// <summary>Runs the rule alone against a refs folder of the test's choosing.</summary>
    private static string RunRule(TemporaryModel model, string? assistantRefs, out bool failed)
    {
        var report = new CheckReport();
        CheckVerb.CheckTargetFramework(ModelProject.Open(model.Folder), assistantRefs, report);

        var output = new StringWriter();
        report.Write(output);
        failed = report.HasProblems;
        return output.ToString();
    }

    /// <summary>A stand-in for the Assistant's refs folder, holding only a stamp.</summary>
    private static TemporaryModel AssistantRefs(string stampBody, bool withBom = false)
    {
        TemporaryModel refs = TemporaryModel.Empty();
        File.WriteAllText(
            refs.PathTo(FrameworkStamp.StampFileName),
            "Juno Cassandra framework - REFERENCE ASSEMBLIES in this folder.\n\n" + stampBody + "\n",
            new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: withBom));
        return refs;
    }
}
