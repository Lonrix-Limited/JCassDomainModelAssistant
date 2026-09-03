using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JcassDm.Cli;
using Xunit;

namespace JcassDm.Tests;

/// <summary>
/// <c>check</c>'s framework-reference rule: is this model compiling against the same framework
/// the Assistant beside it documents?
///
/// <para><b>Every scaffolded model keeps a private copy of the reference assemblies</b>, made when
/// it was scaffolded, and nothing on the maintainer side can reach it. Re-downloading the Assistant
/// refreshes its own <c>refs\</c> and leaves the model on the previous framework, silently. Before
/// this rule existed the only thing standing between an engineer and that state was their having
/// read one step of an update page they see twice a year.</para>
///
/// <para><b>The stale case is the one that matters, so it is the one that is faked.</b> There is
/// only ever one framework build in this repository, so a test cannot produce a genuinely older
/// one. It patches the commit string in a copied assembly's version resource instead - a
/// same-length substitution that leaves every offset in the file valid, and that
/// <c>FileVersionInfo</c> reads back exactly as Windows would read a real one. Without it the
/// warning branch would ship untested, which is the same as shipping it broken.</para>
/// </summary>
public class FrameworkReferenceTests
{
    private const string FakeCommit = "a1b2c3d4e5f60718293a4b5c6d7e8f9012345678";

    [Fact]
    public void A_model_on_the_same_framework_as_the_assistant_passes()
    {
        using TemporaryModel model = FixtureModels.Copy("healthy");
        SeedRefs(model, patchCommit: null);

        ToolResult result = Check(model);

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("same as this Assistant", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_model_behind_the_assistant_is_told_so_and_told_what_to_run()
    {
        using TemporaryModel model = FixtureModels.Copy("healthy");
        SeedRefs(model, patchCommit: FakeCommit);

        ToolResult result = Check(model);

        Assert.Contains("framework reference", result.Output, StringComparison.Ordinal);
        Assert.Contains(FakeCommit[..7], result.Output, StringComparison.Ordinal);
        Assert.Contains("refresh-model-refs.ps1", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_stale_reference_is_a_note_and_does_not_fail_the_check()
    {
        // Deliberate. A stale reference still builds and still runs, so refusing over it would
        // block work for something that is not wrong yet - and a check that blocks work is a
        // check somebody stops running.
        using TemporaryModel model = FixtureModels.Copy("healthy");
        SeedRefs(model, patchCommit: FakeCommit);

        ToolResult result = Check(model);

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.DoesNotContain("PROBLEM", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Assemblies_from_two_framework_releases_in_one_folder_are_called_out()
    {
        // What a hand copy produces: files dropped in over whatever was already there. The
        // .csproj references refs\*.dll with a wildcard, so the leftover is compiled against
        // rather than ignored, which is why this is reported separately from simply being behind.
        using TemporaryModel model = FixtureModels.Copy("healthy");
        SeedRefs(model, patchCommit: FakeCommit, patchOnlyOne: true);

        ToolResult result = Check(model);

        Assert.Contains("different framework builds", result.Output, StringComparison.Ordinal);
        Assert.Contains("refresh-model-refs.ps1", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void A_model_with_no_refs_folder_is_skipped_and_told_how_to_fix_it()
    {
        // The fixtures are source-only, so this is the state they are all in. A skip rather than
        // a pass: nothing was compared, and saying otherwise would be the quiet no-op that these
        // tests exist to prevent.
        using TemporaryModel model = FixtureModels.Copy("healthy");

        ToolResult result = Check(model);

        Assert.Contains("SKIPPED", result.Output, StringComparison.Ordinal);
        Assert.Contains("framework reference", result.Output, StringComparison.Ordinal);
        Assert.Contains("refresh-model-refs.ps1", result.Output, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------------------

    private static ToolResult Check(TemporaryModel model)
        => model.Run("check", "--project", model.Folder);

    /// <summary>
    /// Copies the Assistant's reference assemblies into the model's <c>refs\</c>, optionally
    /// rewriting the framework commit stamped on them.
    /// </summary>
    /// <param name="patchCommit">
    /// Null to leave them as they are, so the model reads as current. A 40-character hex string to
    /// make the model look like it was scaffolded from an older release.
    /// </param>
    /// <param name="patchOnlyOne">
    /// Patch a single assembly rather than all of them, producing the mixed-release folder a hand
    /// copy leaves behind.
    /// </param>
    private static void SeedRefs(TemporaryModel model, string? patchCommit, bool patchOnlyOne = false)
    {
        string source = Path.Combine(TestBundle.RepoRoot(), "refs");
        string target = model.PathTo("refs");
        Directory.CreateDirectory(target);

        string realCommit = RealCommit(source);
        bool patched = false;

        foreach (string file in Directory.GetFiles(source, "JCass_*.dll").OrderBy(f => f, StringComparer.Ordinal))
        {
            string destination = Path.Combine(target, Path.GetFileName(file));
            File.Copy(file, destination, overwrite: true);

            if (patchCommit is null) continue;
            if (patchOnlyOne && patched) continue;

            PatchCommit(destination, realCommit, patchCommit);
            patched = true;
        }
    }

    /// <summary>The commit the Assistant's own reference assemblies carry.</summary>
    private static string RealCommit(string refsFolder)
    {
        string first = Directory.GetFiles(refsFolder, "JCass_*.dll").OrderBy(f => f, StringComparer.Ordinal).First();
        string? product = FileVersionInfo.GetVersionInfo(first).ProductVersion;
        int plus = product?.IndexOf('+') ?? -1;

        Assert.True(plus > 0, $"refs\\ assemblies carry no framework commit: ProductVersion was '{product}'.");
        return product![(plus + 1)..];
    }

    /// <summary>
    /// Rewrites the commit in an assembly's Win32 version resource.
    ///
    /// <para>Both encodings are replaced: the resource itself is UTF-16, and the same string is
    /// present once as ASCII in the managed metadata. Same length in, same length out, so nothing
    /// in the file moves.</para>
    /// </summary>
    private static void PatchCommit(string assemblyPath, string oldCommit, string newCommit)
    {
        Assert.Equal(oldCommit.Length, newCommit.Length);

        byte[] bytes = File.ReadAllBytes(assemblyPath);
        int replacements =
            Replace(bytes, Encoding.Unicode.GetBytes(oldCommit), Encoding.Unicode.GetBytes(newCommit)) +
            Replace(bytes, Encoding.ASCII.GetBytes(oldCommit), Encoding.ASCII.GetBytes(newCommit));

        Assert.True(replacements > 0, $"Could not find the commit string in {Path.GetFileName(assemblyPath)}.");
        File.WriteAllBytes(assemblyPath, bytes);
    }

    private static int Replace(byte[] haystack, byte[] needle, byte[] replacement)
    {
        int count = 0;
        for (int i = 0; i <= haystack.Length - needle.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j]) { match = false; break; }
            }
            if (!match) continue;

            Array.Copy(replacement, 0, haystack, i, replacement.Length);
            i += needle.Length - 1;
            count++;
        }
        return count;
    }
}
