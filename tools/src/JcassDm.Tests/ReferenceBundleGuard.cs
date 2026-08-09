using System.IO;
using System.Security.Cryptography;
using JcassDm.Cli;
using Xunit;

namespace JcassDm.Tests;

/// <summary>
/// The reference model's own bundle is committed content. Nothing here may change it.
///
/// <para>Worth a test of its own rather than trusting the fixture, because the way this
/// breaks is somebody writing a quick test that passes <c>TestBundle.SourcePath</c> straight
/// to a write verb - which would pass, silently modify a tracked file, and leave the
/// repository looking like it had uncommitted work in it.</para>
/// </summary>
public class ReferenceBundleGuard
{
    [Fact]
    public void The_reference_bundle_is_where_the_tests_expect_it()
    {
        Assert.True(
            File.Exists(TestBundle.SourcePath),
            $"The reference model's bundle should be at {TestBundle.SourcePath}");
    }

    [Fact]
    public void The_reference_bundle_is_a_valid_bundle()
    {
        // Also a check on the reference model itself: if the sample the documentation is
        // built around stopped being well-formed, this is where it should show up.
        using var bundle = TestBundle.FromReferenceModel();

        ToolResult result = bundle.Run("dump", TestBundle.SourcePath);

        Assert.Equal(ExitCode.Ok, result.ExitCode);
    }

    [Fact]
    public void Running_the_verbs_against_a_copy_leaves_the_original_untouched()
    {
        byte[] before = SHA256.HashData(File.ReadAllBytes(TestBundle.SourcePath));

        using (var bundle = TestBundle.FromReferenceModel())
        {
            bundle.Run("add-treatment", bundle.Path, "--name", "reseal", "--budget-category", "resurfacing");
            bundle.Run("add-parameter", bundle.Path, "--name", "par_iri", "--min", "0", "--max", "10");
            bundle.Run("add-input-header", bundle.Path, "--column", "traffic_count", "--type", "number");
            bundle.Run("set-meta", bundle.Path, "--main-class", "MyRoadModel", "--force");
        }

        Assert.Equal(before, SHA256.HashData(File.ReadAllBytes(TestBundle.SourcePath)));
    }
}
