using System;
using System.IO;
using System.Linq;
using JcassDm.Bundle;
using JcassDm.Cli;
using Xunit;

namespace JcassDm.Tests;

/// <summary>
/// <c>scaffold</c>, and the property it exists to guarantee: the four names cannot disagree.
///
/// <para>Most of these are about what the tool <b>refuses</b>. A generator that emits a correct
/// project on the happy path and a subtly wrong one when somebody passes an odd name has not
/// removed the failure class, it has moved it.</para>
/// </summary>
public class ScaffoldTests
{
    [Fact]
    public void The_four_names_all_come_out_of_the_one_name()
    {
        using TemporaryModel target = TemporaryModel.Empty();
        string folder = Path.Combine(target.Folder, "MyRoadModel");

        ToolResult result = target.Run("scaffold", "MyRoadModel", "--output", folder);
        Assert.Equal(ExitCode.Ok, result.ExitCode);

        // 1. the .csproj filename
        Assert.True(File.Exists(Path.Combine(folder, "MyRoadModel.csproj")));

        // 2. the assembly name, which is name 1 unless <AssemblyName> is set - so it must not be
        Assert.DoesNotContain("<AssemblyName>",
            StripXmlComments(File.ReadAllText(Path.Combine(folder, "MyRoadModel.csproj"))),
            StringComparison.Ordinal);

        // 3. the entry class, in a file of the same name
        string entry = File.ReadAllText(Path.Combine(folder, "Objects", "MyRoadModel.cs"));
        Assert.Contains("class MyRoadModel : DomainModelBase", entry, StringComparison.Ordinal);

        // 4. both meta settings
        using BundleFile bundle = BundleFile.Open(Path.Combine(folder, "domain_model_setup.xlsx"));
        SheetTable meta = bundle.Sheet(SheetSpec.Meta);
        Assert.Equal("MyRoadModel.dll", meta.Text(meta.FindRowByKey("Setting", "main_dll"), "Value"));
        Assert.Equal("MyRoadModel", meta.Text(meta.FindRowByKey("Setting", "main_class"), "Value"));
    }

    [Fact]
    public void There_is_no_option_that_sets_one_of_the_four_on_its_own()
    {
        // The whole argument for a generator over a rename. If any of these is ever accepted,
        // the tool can emit a mismatch again and the failure class is back.
        using TemporaryModel target = TemporaryModel.Empty();

        foreach (string option in new[] { "--assembly-name", "--main-dll", "--main-class", "--class" })
        {
            ToolResult result = target.Run(
                "scaffold", "Model", "--output", Path.Combine(target.Folder, "x"), option, "Other");

            Assert.Equal(ExitCode.UsageError, result.ExitCode);
            Assert.Contains("Unrecognised option", result.All, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void The_canonical_skeleton_is_one_file_per_stage()
    {
        using TemporaryModel target = TemporaryModel.Empty();
        string folder = Path.Combine(target.Folder, "MyRoadModel");

        target.Run("scaffold", "MyRoadModel", "--output", folder, "--element", "RoadSegment");

        var expected = new[]
        {
            "Constants.cs", "Incrementer.cs", "Initialiser.cs", "MyRoadModel.cs",
            "Resetter.cs", "RoadSegment.cs", "RoadSegmentFactory.cs", "RoutineMaintenance.cs",
            "TreatmentNames.cs", "TreatmentsTrigger.cs",
        };

        var actual = Directory.GetFiles(Path.Combine(folder, "Objects"))
            .Select(Path.GetFileName)
            .OrderBy(n => n, StringComparer.Ordinal);

        Assert.Equal(expected.OrderBy(n => n, StringComparer.Ordinal), actual);
    }

    [Fact]
    public void The_stubs_carry_no_modelling_numbers()
    {
        // Locked decision 17 binds the scaffolder. A plausible default is worse than a hole,
        // because a hole gets filled and a default gets shipped - and the shape a stub models is
        // the shape the engineer copies for the next twenty thresholds.
        using TemporaryModel target = TemporaryModel.Empty();
        string folder = Path.Combine(target.Folder, "MyRoadModel");

        target.Run("scaffold", "MyRoadModel", "--output", folder);

        foreach (string file in Directory.GetFiles(Path.Combine(folder, "Objects")))
        {
            string code = StripComments(File.ReadAllText(file));

            Assert.DoesNotContain("const double", code, StringComparison.Ordinal);
            Assert.DoesNotContain("const int", code, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void An_empty_scaffold_and_its_bundle_agree_with_each_other()
    {
        using TemporaryModel target = TemporaryModel.Empty();
        string folder = Path.Combine(target.Folder, "MyRoadModel");

        target.Run("scaffold", "MyRoadModel", "--output", folder);
        ToolResult check = target.Run("check", "--project", folder);

        Assert.Equal(ExitCode.Ok, check.ExitCode);
    }

    [Fact]
    public void From_sample_carries_the_reference_model_treatments()
    {
        using TemporaryModel target = TemporaryModel.Empty();
        string folder = Path.Combine(target.Folder, "Walking");

        ToolResult result = target.Run("scaffold", "Walking", "--output", folder, "--from-sample");
        Assert.Equal(ExitCode.Ok, result.ExitCode);

        string names = File.ReadAllText(Path.Combine(folder, "Objects", "TreatmentNames.cs"));
        Assert.Contains("\"repair\"", names, StringComparison.Ordinal);
        Assert.Contains("\"replace\"", names, StringComparison.Ordinal);
        Assert.Contains("\"RMaint\"", names, StringComparison.Ordinal);

        // ...and its meta names the new model, not the one it was copied from.
        using BundleFile bundle = BundleFile.Open(Path.Combine(folder, "domain_model_setup.xlsx"));
        SheetTable meta = bundle.Sheet(SheetSpec.Meta);
        Assert.Equal("Walking.dll", meta.Text(meta.FindRowByKey("Setting", "main_dll"), "Value"));
        Assert.Equal("Walking", meta.Text(meta.FindRowByKey("Setting", "main_class"), "Value"));
    }

    [Fact]
    public void From_sample_reads_every_number_out_of_lookups()
    {
        // The walking skeleton is the artefact the engineer KEEPS. Shipping it with a hard-coded
        // threshold teaches the shape they will copy for their own.
        using TemporaryModel target = TemporaryModel.Empty();
        string folder = Path.Combine(target.Folder, "Walking");

        target.Run("scaffold", "Walking", "--output", folder, "--from-sample");

        foreach (string name in new[] { "TreatmentsTrigger.cs", "Incrementer.cs", "Resetter.cs", "RoutineMaintenance.cs" })
        {
            string code = StripComments(File.ReadAllText(Path.Combine(folder, "Objects", name)));

            Assert.DoesNotContain("const double", code, StringComparison.Ordinal);
            Assert.DoesNotContain("const int", code, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void The_element_class_is_named_by_its_own_option()
    {
        using TemporaryModel target = TemporaryModel.Empty();
        string folder = Path.Combine(target.Folder, "MyRoadModel");

        target.Run("scaffold", "MyRoadModel", "--output", folder, "--element", "RoadSegment");

        Assert.True(File.Exists(Path.Combine(folder, "Objects", "RoadSegment.cs")));
        Assert.True(File.Exists(Path.Combine(folder, "Objects", "RoadSegmentFactory.cs")));
    }

    [Fact]
    public void The_element_may_not_be_given_the_models_name()
    {
        using TemporaryModel target = TemporaryModel.Empty();

        ToolResult result = target.Run(
            "scaffold", "MyRoadModel", "--output", Path.Combine(target.Folder, "x"),
            "--element", "MyRoadModel");

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
    }

    [Theory]
    [InlineData("My Road Model", "spaces")]
    [InlineData("My.Road.Model", "full stops")]
    [InlineData("9Lives", "leading digit")]
    [InlineData("My-Road-Model", "hyphens")]
    [InlineData("CON", "a reserved Windows device name")]
    [InlineData("../Escape", "a path")]
    public void A_name_that_cannot_be_all_four_things_is_refused(string name, string why)
    {
        using TemporaryModel target = TemporaryModel.Empty();

        ToolResult result = target.Run("scaffold", name, "--output", Path.Combine(target.Folder, "x"));

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.False(Directory.Exists(Path.Combine(target.Folder, "x")), $"nothing should be written for {why}");
    }

    [Fact]
    public void Scaffolding_over_an_existing_model_is_refused()
    {
        using TemporaryModel existing = FixtureModels.Copy("healthy");
        string fingerprint = existing.Fingerprint();

        ToolResult result = existing.Run("scaffold", "MyRoadModel", "--output", existing.Folder);

        Assert.Equal(ExitCode.UsageError, result.ExitCode);
        Assert.Equal(fingerprint, existing.Fingerprint());
        // ...and it points at the verb that does handle an existing model.
        Assert.Contains("jcass-dm rename", result.All, StringComparison.Ordinal);
    }

    [Fact]
    public void A_failed_scaffold_leaves_no_half_written_folder()
    {
        using TemporaryModel target = TemporaryModel.Empty();
        string folder = Path.Combine(target.Folder, "Half");

        // --from-sample needs the reference bundle. Point the element at an invalid name so the
        // run fails after the folder exists but before it holds a usable model.
        ToolResult result = target.Run("scaffold", "Half", "--output", folder, "--element", "not a name");

        Assert.NotEqual(ExitCode.Ok, result.ExitCode);
        Assert.False(Directory.Exists(folder));
    }

    private static string StripComments(string code)
    {
        code = System.Text.RegularExpressions.Regex.Replace(code, @"//[^\n]*", string.Empty);
        return System.Text.RegularExpressions.Regex.Replace(code, @"/\*.*?\*/", string.Empty,
            System.Text.RegularExpressions.RegexOptions.Singleline);
    }

    private static string StripXmlComments(string xml)
        => System.Text.RegularExpressions.Regex.Replace(xml, "<!--.*?-->", string.Empty,
            System.Text.RegularExpressions.RegexOptions.Singleline);
}
