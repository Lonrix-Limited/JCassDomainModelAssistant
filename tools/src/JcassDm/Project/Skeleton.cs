using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JcassDm.Cli;

namespace JcassDm.Project;

/// <summary>
/// The canonical file set a scaffolded model gets, and the templates behind it.
///
/// <para><b>Why a whole skeleton rather than a bare entry class.</b> A survey of every Cassandra
/// domain model that is not commercially restricted found them all converging on the same file
/// set, and it maps one to one onto the framework's per-period stages. An engineer who has that
/// shape always knows where a change belongs, and so does the agent helping them. A bare entry
/// class leaves both of them to invent a structure, and they invent a different one each
/// time.</para>
///
/// <para><b>Where the reference model differs, the majority wins.</b> <c>DomainModelSample</c>
/// folds initialise, increment and reset into its element class and calls its trigger
/// <c>TreatmentTrigger</c>, singular; every other model separates the three and writes
/// <c>TreatmentsTrigger</c>. The sample is shaped for reading in ten minutes, which is a
/// different job from being a template.</para>
/// </summary>
internal static class Skeleton
{
    /// <summary>The folder the C# files go in. <c>Objects\</c> is what two of the three models use.</summary>
    public const string SourceFolder = "Objects";

    /// <summary>The default element class name when <c>--element</c> is not given.</summary>
    public const string DefaultElementName = "ModelElement";

    /// <summary>
    /// The ten C# files, in the order they are described to the user - stage order, not
    /// alphabetical, because the point of the set is that it maps onto the run.
    /// </summary>
    public static IReadOnlyList<SkeletonFile> Files(ModelName model, string elementName) => new[]
    {
        new SkeletonFile("EntryClass.cs", $"{model.ClassName}.cs", "entry class - the switchboard", Shared: true),
        new SkeletonFile("Constants.cs", "Constants.cs", "every tunable number, read from lookups.xlsx"),
        new SkeletonFile("TreatmentNames.cs", "TreatmentNames.cs", "treatment name constants, shared with the bundle"),
        new SkeletonFile("Element.cs", $"{elementName}.cs", "what an element is: state carried between periods"),
        new SkeletonFile("ElementFactory.cs", $"{elementName}Factory.cs", "framework dictionaries -> element; all input column names"),
        new SkeletonFile("Initialiser.cs", "Initialiser.cs", "stage 1: starting state, before period 1"),
        new SkeletonFile("TreatmentsTrigger.cs", "TreatmentsTrigger.cs", "stage 2: what work is due, and what it costs"),
        new SkeletonFile("Incrementer.cs", "Incrementer.cs", "stage 4a: decay when nothing is done"),
        new SkeletonFile("Resetter.cs", "Resetter.cs", "stage 4b: recovery when something is"),
        new SkeletonFile("RoutineMaintenance.cs", "RoutineMaintenance.cs", "stage 5: work outside the budget"),
    };

    /// <summary>
    /// Reads a template and substitutes the three placeholders.
    ///
    /// <para>Only three exist, and two of them are the same string: the model name is the one
    /// name a scaffolded project has, and everything the four-name rule covers is derived from
    /// it here rather than being passed in separately.</para>
    /// </summary>
    /// <param name="variantFolder">"skeleton", "sample" or "shared".</param>
    /// <param name="templateName">Template file name without ".template", e.g. "Constants.cs".</param>
    /// <param name="model">The model name.</param>
    /// <param name="elementName">The element class name.</param>
    /// <param name="namespaceName">The namespace to declare.</param>
    public static string Render(
        string variantFolder,
        string templateName,
        ModelName model,
        string elementName,
        string namespaceName)
    {
        string template = ReadTemplate(variantFolder, templateName);

        return template
            .Replace("{{MODEL}}", model.Value, StringComparison.Ordinal)
            .Replace("{{ELEMENT}}", elementName, StringComparison.Ordinal)
            .Replace("{{NAMESPACE}}", namespaceName, StringComparison.Ordinal);
    }

    /// <summary>
    /// Loads one embedded template. Fails loudly rather than emitting an empty file: a missing
    /// template is a defect in the tool, and a scaffolded project with a zero-byte source file in
    /// it would be diagnosed as the engineer's problem.
    /// </summary>
    private static string ReadTemplate(string variantFolder, string templateName)
    {
        string resource = $"JcassDm.Templates.{variantFolder}.{templateName}.template";

        Assembly assembly = typeof(Skeleton).Assembly;
        using Stream? stream = assembly.GetManifestResourceStream(resource);
        if (stream is null)
        {
            string available = string.Join(", ", assembly.GetManifestResourceNames().OrderBy(n => n, StringComparer.Ordinal));
            throw new InvalidOperationException(
                $"Template '{resource}' is not embedded in jcass-dm. Embedded: {available}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

/// <summary>One file the scaffolder emits.</summary>
/// <param name="TemplateName">Template file name without ".template", e.g. "Constants.cs".</param>
/// <param name="FileName">File name written into <c>Objects\</c>.</param>
/// <param name="Purpose">One line for the summary the scaffolder prints.</param>
/// <param name="Shared">True when the template is the same for an empty scaffold and --from-sample.</param>
internal sealed record SkeletonFile(string TemplateName, string FileName, string Purpose, bool Shared = false);
