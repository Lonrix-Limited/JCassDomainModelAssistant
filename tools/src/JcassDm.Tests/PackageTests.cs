using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JcassDm.Cli;
using Xunit;

namespace JcassDm.Tests;

/// <summary>
/// <c>package</c>, and the two mistakes it exists to make impossible.
///
/// <para>Both cost an afternoon when they happen by hand, and both fail somewhere that does not
/// name the zip: a folder-level zip fails at F5 with "No .csproj file found at workspace root",
/// and an included <c>refs\</c> overwrites part of the workspace's own staged framework
/// assemblies with reference assemblies that cannot be executed.</para>
/// </summary>
public class PackageTests
{
    [Fact]
    public void The_zip_opens_straight_to_the_csproj()
    {
        using TemporaryModel model = FixtureModels.Copy("healthy");
        string zip = model.PathTo("out.zip");

        ToolResult result = model.Run("package", "--project", model.Folder, "--output", zip);
        Assert.Equal(ExitCode.Ok, result.ExitCode);

        string[] entries = EntriesIn(zip);

        // The property in full: a .csproj at the TOP level, not one level down inside a folder
        // named after the project.
        Assert.Contains("FixtureModel.csproj", entries);
        Assert.DoesNotContain(entries, e => e.StartsWith("FixtureModel/", StringComparison.Ordinal));
    }

    [Fact]
    public void Refs_bin_and_obj_are_left_out()
    {
        using TemporaryModel model = FixtureModels.Copy("healthy");

        Directory.CreateDirectory(model.PathTo("refs"));
        File.WriteAllText(model.PathTo("refs", "JCass_ModelCore.dll"), "not really a dll");
        Directory.CreateDirectory(model.PathTo("bin", "Debug"));
        File.WriteAllText(model.PathTo("bin", "Debug", "FixtureModel.dll"), "build output");
        Directory.CreateDirectory(model.PathTo("obj"));
        File.WriteAllText(model.PathTo("obj", "project.assets.json"), "{}");

        string zip = model.PathTo("out.zip");
        model.Run("package", "--project", model.Folder, "--output", zip);

        string[] entries = EntriesIn(zip);

        Assert.DoesNotContain(entries, e => e.StartsWith("refs/", StringComparison.Ordinal));
        Assert.DoesNotContain(entries, e => e.StartsWith("bin/", StringComparison.Ordinal));
        Assert.DoesNotContain(entries, e => e.StartsWith("obj/", StringComparison.Ordinal));
    }

    [Fact]
    public void The_source_and_the_bundle_are_in_it()
    {
        using TemporaryModel model = FixtureModels.Copy("healthy");
        string zip = model.PathTo("out.zip");

        model.Run("package", "--project", model.Folder, "--output", zip);
        string[] entries = EntriesIn(zip);

        Assert.Contains("domain_model_setup.xlsx", entries);
        Assert.Contains("Objects/Constants.cs", entries);
        Assert.Equal(10, entries.Count(e => e.StartsWith("Objects/", StringComparison.Ordinal)));
    }

    [Fact]
    public void Entry_names_use_forward_slashes()
    {
        // The zip format specifies them, and the debug sidecar unpacks on Linux. A backslash
        // there produces a single file with a slash in its name rather than a folder.
        using TemporaryModel model = FixtureModels.Copy("healthy");
        string zip = model.PathTo("out.zip");

        model.Run("package", "--project", model.Folder, "--output", zip);

        Assert.All(EntriesIn(zip), e => Assert.DoesNotContain('\\', e));
    }

    [Fact]
    public void A_zip_written_into_the_project_folder_does_not_contain_itself()
    {
        using TemporaryModel model = FixtureModels.Copy("healthy");

        ToolResult result = model.Run("package", "--project", model.Folder);
        Assert.Equal(ExitCode.Ok, result.ExitCode);

        string zip = model.PathTo("FixtureModel_for_debug.zip");
        Assert.True(File.Exists(zip));
        Assert.DoesNotContain(EntriesIn(zip), e => e.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void An_existing_zip_is_not_replaced_without_force()
    {
        using TemporaryModel model = FixtureModels.Copy("healthy");
        string zip = model.PathTo("out.zip");
        File.WriteAllText(zip, "somebody else's file");

        ToolResult result = model.Run("package", "--project", model.Folder, "--output", zip);

        Assert.Equal(ExitCode.Conflict, result.ExitCode);
        Assert.Equal("somebody else's file", File.ReadAllText(zip));

        Assert.Equal(ExitCode.Ok,
            model.Run("package", "--project", model.Folder, "--output", zip, "--force").ExitCode);
    }

    [Fact]
    public void Packaging_a_model_with_two_project_files_is_refused()
    {
        using TemporaryModel model = FixtureModels.Copy("two-csproj");

        ToolResult result = model.Run("package", "--project", model.Folder);

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
    }

    [Fact]
    public void The_output_says_what_was_left_out()
    {
        // Silent exclusion is how somebody spends an afternoon wondering where refs\ went.
        using TemporaryModel model = FixtureModels.Copy("healthy");
        Directory.CreateDirectory(model.PathTo("refs"));
        File.WriteAllText(model.PathTo("refs", "x.dll"), "x");

        ToolResult result = model.Run("package", "--project", model.Folder, "--output", model.PathTo("out.zip"));

        Assert.Contains("Left out:", result.Output, StringComparison.Ordinal);
        Assert.Contains("refs\\", result.Output, StringComparison.Ordinal);
    }

    private static string[] EntriesIn(string zip)
    {
        using var archive = ZipFile.OpenRead(zip);
        return archive.Entries.Select(e => e.FullName).OrderBy(n => n, StringComparer.Ordinal).ToArray();
    }
}
