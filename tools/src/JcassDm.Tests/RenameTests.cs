using System;
using System.IO;
using System.Linq;
using JcassDm.Bundle;
using JcassDm.Cli;
using Xunit;

namespace JcassDm.Tests;

/// <summary>
/// <c>rename</c>, and the promise that makes it safe to use: all four names change together, or
/// nothing changes at all.
///
/// <para>The atomicity tests matter more than the happy path. A half-renamed model is worse than
/// the mismatch it was meant to fix - the engineer no longer knows which state they are in, and
/// neither does the agent helping them - so "it failed and put everything back" has to be proved
/// rather than assumed from reading the code.</para>
/// </summary>
public class RenameTests
{
    [Fact]
    public void All_four_names_change_together()
    {
        using TemporaryModel model = FixtureModels.Copy("names-disagree");

        ToolResult result = model.Run("rename", "InheritedRoadModel", "--project", model.Folder);
        Assert.Equal(ExitCode.Ok, result.ExitCode);

        Assert.True(File.Exists(model.PathTo("InheritedRoadModel.csproj")));
        Assert.True(File.Exists(model.PathTo("Objects", "InheritedRoadModel.cs")));
        Assert.False(File.Exists(model.PathTo("Objects", "FixtureModel.cs")));

        string entry = File.ReadAllText(model.PathTo("Objects", "InheritedRoadModel.cs"));
        Assert.Contains("class InheritedRoadModel : DomainModelBase", entry, StringComparison.Ordinal);

        using BundleFile bundle = BundleFile.Open(model.PathTo("domain_model_setup.xlsx"));
        SheetTable meta = bundle.Sheet(SheetSpec.Meta);
        Assert.Equal("InheritedRoadModel.dll", meta.Text(meta.FindRowByKey("Setting", "main_dll"), "Value"));
        Assert.Equal("InheritedRoadModel", meta.Text(meta.FindRowByKey("Setting", "main_class"), "Value"));
    }

    [Fact]
    public void The_renamed_model_passes_check()
    {
        using TemporaryModel model = FixtureModels.Copy("names-disagree");

        Assert.Equal(ExitCode.CheckFailed, model.Run("check", "--project", model.Folder).ExitCode);

        model.Run("rename", "InheritedRoadModel", "--project", model.Folder);

        Assert.Equal(ExitCode.Ok, model.Run("check", "--project", model.Folder).ExitCode);
    }

    [Fact]
    public void An_interrupted_rename_leaves_the_model_exactly_as_it_was()
    {
        using TemporaryModel model = FixtureModels.Copy("names-disagree");
        string before = model.Fingerprint();

        // The bundle is the LAST thing rename writes, so making it unwritable fails the run after
        // the source edits and the file moves have already happened. That is the state the
        // all-or-nothing promise is actually about.
        string bundle = model.PathTo("domain_model_setup.xlsx");
        File.SetAttributes(bundle, FileAttributes.ReadOnly);

        ToolResult result;
        try
        {
            result = model.Run("rename", "InheritedRoadModel", "--project", model.Folder);
        }
        finally
        {
            File.SetAttributes(bundle, FileAttributes.Normal);
        }

        Assert.NotEqual(ExitCode.Ok, result.ExitCode);
        Assert.Equal(before, model.Fingerprint());
    }

    [Fact]
    public void An_interrupted_rename_says_so_rather_than_reporting_a_tool_bug()
    {
        // A restore that throws out of Dispose replaces "the model has been put back" with a
        // stack trace and exit 9, which reads as "jcass-dm is broken" at the exact moment
        // somebody needs to be told their model is fine.
        using TemporaryModel model = FixtureModels.Copy("names-disagree");

        string bundle = model.PathTo("domain_model_setup.xlsx");
        File.SetAttributes(bundle, FileAttributes.ReadOnly);

        ToolResult result;
        try
        {
            result = model.Run("rename", "InheritedRoadModel", "--project", model.Folder);
        }
        finally
        {
            File.SetAttributes(bundle, FileAttributes.Normal);
        }

        Assert.NotEqual(ExitCode.ToolFailure, result.ExitCode);
        Assert.Contains("put back exactly as it was", result.All, StringComparison.Ordinal);
        Assert.DoesNotContain("Nothing is half-renamed.\n   at ", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void A_rename_on_to_an_existing_file_is_refused_before_anything_is_written()
    {
        using TemporaryModel model = FixtureModels.Copy("names-disagree");
        File.WriteAllText(model.PathTo("Objects", "Taken.cs"), "// in the way");
        File.Move(model.PathTo("InheritedRoadModel.csproj"), model.PathTo("Taken.csproj"));

        string before = model.Fingerprint();
        ToolResult result = model.Run("rename", "Taken", "--project", model.Folder);

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Equal(before, model.Fingerprint());
    }

    [Fact]
    public void A_model_already_carrying_the_name_is_left_alone()
    {
        using TemporaryModel model = FixtureModels.Copy("healthy");
        string before = model.Fingerprint();

        ToolResult result = model.Run("rename", "FixtureModel", "--project", model.Folder);

        Assert.Equal(ExitCode.Ok, result.ExitCode);
        Assert.Contains("unchanged", result.Output, StringComparison.Ordinal);
        Assert.Equal(before, model.Fingerprint());
    }

    [Fact]
    public void The_namespace_is_left_alone_by_default_and_the_output_says_why()
    {
        using TemporaryModel model = FixtureModels.Copy("names-disagree");

        ToolResult result = model.Run("rename", "InheritedRoadModel", "--project", model.Folder);

        Assert.Contains("namespace FixtureModel.Objects;",
            File.ReadAllText(model.PathTo("Objects", "Constants.cs")), StringComparison.Ordinal);

        Assert.Contains("NOT one of the four names", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void The_namespace_moves_when_it_is_asked_to()
    {
        using TemporaryModel model = FixtureModels.Copy("names-disagree");

        model.Run("rename", "InheritedRoadModel", "--project", model.Folder, "--namespace");

        foreach (string file in Directory.GetFiles(model.PathTo("Objects")))
        {
            Assert.Contains("namespace InheritedRoadModel.Objects;",
                File.ReadAllText(file), StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Renaming_a_model_with_two_project_files_is_refused()
    {
        // check reports this and carries on; rename must not, because it would have to guess
        // which project file it was acting on.
        using TemporaryModel model = FixtureModels.Copy("two-csproj");
        string before = model.Fingerprint();

        ToolResult result = model.Run("rename", "Whatever", "--project", model.Folder);

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Equal(before, model.Fingerprint());
    }

    [Fact]
    public void A_model_with_no_entry_class_is_refused_with_a_pointer_to_check()
    {
        using TemporaryModel model = FixtureModels.Copy("healthy");
        string entry = model.PathTo("Objects", "FixtureModel.cs");
        File.WriteAllText(entry,
            File.ReadAllText(entry).Replace(": DomainModelBase", ": object", StringComparison.Ordinal));

        string before = model.Fingerprint();
        ToolResult result = model.Run("rename", "Whatever", "--project", model.Folder);

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Equal(before, model.Fingerprint());
        Assert.Contains("jcass-dm check", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void Renaming_does_not_touch_files_that_do_not_mention_the_name()
    {
        using TemporaryModel model = FixtureModels.Copy("names-disagree");

        string untouched = model.PathTo("Objects", "TreatmentNames.cs");
        string before = File.ReadAllText(untouched);

        model.Run("rename", "InheritedRoadModel", "--project", model.Folder);

        Assert.Equal(before, File.ReadAllText(untouched));
    }
}
