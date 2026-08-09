using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JcassDm.Cli;

namespace JcassDm.Project;

/// <summary>
/// A domain model project on disk, as much of it as can be worked out without a compiler.
///
/// <para>Shared by <c>check</c>, <c>rename</c> and <c>package</c>, which all need the same four
/// facts: where the <c>.csproj</c> is, what the entry class is called and which file holds it,
/// where the bundle is, and which source files there are.</para>
///
/// <para><b>Nothing here parses C#.</b> The scanning is regular expressions over source text,
/// which is enough to find a class declaration and a method call and is honest about being a
/// heuristic. A real parser would need Roslyn, which would treble the size of a tool that has to
/// be committed to a public repository - and the failure mode of the cheap version is a check
/// that reports something it cannot see, not a check that silently passes. <c>check</c> says so
/// in its own output.</para>
/// </summary>
internal sealed class ModelProject
{
    /// <summary>The bundle file every domain model carries, beside its .csproj.</summary>
    public const string BundleFileName = "domain_model_setup.xlsx";

    /// <summary>Folders that are build output or tooling rather than model source.</summary>
    private static readonly string[] IgnoredFolders = { "bin", "obj", ".git", ".vs", "refs", ".idea" };

    private ModelProject(string folder, string projectFilePath, IReadOnlyList<SourceFile> sources)
    {
        this.Folder = folder;
        this.ProjectFilePath = projectFilePath;
        this.Sources = sources;
    }

    /// <summary>Absolute path to the project folder - the one that gets zipped.</summary>
    public string Folder { get; }

    /// <summary>Absolute path to the single <c>.csproj</c> at the project root.</summary>
    public string ProjectFilePath { get; }

    /// <summary>The <c>.csproj</c> file's stem. Name #1 of four, and the one a debug run trusts.</summary>
    public string ProjectStem => Path.GetFileNameWithoutExtension(this.ProjectFilePath);

    /// <summary>Every <c>.cs</c> file that is part of the model, build output excluded.</summary>
    public IReadOnlyList<SourceFile> Sources { get; }

    /// <summary>Absolute path to the bundle, whether or not it exists.</summary>
    public string BundlePath => Path.Combine(this.Folder, BundleFileName);

    /// <summary>True when the bundle is where it should be.</summary>
    public bool HasBundle => File.Exists(this.BundlePath);

    /// <summary>
    /// Opens the project rooted at <paramref name="folder"/>.
    ///
    /// <para>Fails when there is no <c>.csproj</c> at the root, or more than one. Both are
    /// genuinely fatal rather than pedantic: the debug workspace refuses to guess between two
    /// project files, so a second one - a stray copy, an experiment - stops F5 working with a
    /// message about the workspace rather than about the project.</para>
    /// </summary>
    public static ModelProject Open(string folder)
    {
        string full = ResolveFolder(folder);
        IReadOnlyList<string> projectFiles = ProjectFilesIn(full);

        if (projectFiles.Count > 1)
        {
            throw new UsageFailure(DescribeTooManyProjectFiles(full, projectFiles));
        }

        return new ModelProject(full, projectFiles[0], ReadSources(full));
    }

    /// <summary>
    /// Opens a project that may have more than one <c>.csproj</c>, taking the first alphabetically.
    ///
    /// <para>Only <c>check</c> uses this, and the reason is worth stating: <c>check</c> is the
    /// first thing somebody runs on a model they have just inherited, so refusing to look at the
    /// most obviously broken case would be exactly backwards. It reports the extra project file
    /// as a problem and carries on with the rest of the rules. <c>rename</c> and <c>package</c>
    /// still refuse, because both would have to guess which project they were acting on.</para>
    /// </summary>
    /// <param name="folder">The project folder.</param>
    /// <param name="allProjectFiles">Every <c>.csproj</c> found at the root, in order.</param>
    public static ModelProject OpenForDiagnosis(string folder, out IReadOnlyList<string> allProjectFiles)
    {
        string full = ResolveFolder(folder);
        allProjectFiles = ProjectFilesIn(full);
        return new ModelProject(full, allProjectFiles[0], ReadSources(full));
    }

    /// <summary>The message <c>Open</c> fails with, shared so <c>check</c> can print the same words.</summary>
    public static string DescribeTooManyProjectFiles(string folder, IReadOnlyList<string> projectFiles)
        => $"{projectFiles.Count} .csproj files at the top level of {folder}:" + Environment.NewLine +
           string.Join(Environment.NewLine, projectFiles.Select(p => "  - " + Path.GetFileName(p))) +
           Environment.NewLine + Environment.NewLine +
           "A domain model must have exactly one. A web debug run refuses to guess between two, " +
           "so this stops F5 working. Delete or move the one you do not want.";

    private static string ResolveFolder(string folder)
    {
        string full = Path.GetFullPath(folder);

        if (File.Exists(full) && full.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            // Somebody passed the .csproj rather than the folder. Obvious intent; accept it.
            full = Path.GetDirectoryName(full)!;
        }

        if (!Directory.Exists(full))
        {
            throw new UsageFailure(
                $"No folder at: {full}" + Environment.NewLine +
                "Point --project at your model's folder - the one holding the .csproj.");
        }

        return full;
    }

    private static IReadOnlyList<string> ProjectFilesIn(string folder)
    {
        var projectFiles = Directory.GetFiles(folder, "*.csproj", SearchOption.TopDirectoryOnly)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        if (projectFiles.Count == 0)
        {
            throw new UsageFailure(
                $"No .csproj file in {folder}." + Environment.NewLine +
                "A domain model project has exactly one, at the top level, beside " +
                BundleFileName + ".");
        }

        return projectFiles;
    }

    /// <summary>
    /// The value of <c>&lt;AssemblyName&gt;</c> in the <c>.csproj</c>, or null when it is unset -
    /// which is the correct state and what a scaffolded project has.
    /// </summary>
    public string? DeclaredAssemblyName()
    {
        string text = File.ReadAllText(this.ProjectFilePath);

        // Strip XML comments first. The scaffolded csproj carries a comment explaining why the
        // element is absent, and matching the word inside it would report the opposite.
        text = Regex.Replace(text, "<!--.*?-->", string.Empty, RegexOptions.Singleline);

        Match match = Regex.Match(text, @"<AssemblyName>\s*(?<name>[^<]*?)\s*</AssemblyName>",
            RegexOptions.IgnoreCase);
        return match.Success ? match.Groups["name"].Value : null;
    }

    /// <summary>
    /// Finds the entry class: the one deriving from <c>DomainModelBase</c>.
    ///
    /// <para>Returns null when there is none to be found, which <c>check</c> reports rather than
    /// throwing - a model mid-edit may genuinely not have one, and a diagnostic that refuses to
    /// run on a broken model is a diagnostic nobody can use.</para>
    /// </summary>
    public EntryClass? FindEntryClass()
    {
        foreach (SourceFile source in this.Sources)
        {
            Match match = Regex.Match(
                source.Text,
                @"(?<!//[^\n]*)\bclass\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*:\s*DomainModelBase\b");
            if (match.Success)
            {
                return new EntryClass(match.Groups["name"].Value, source);
            }
        }
        return null;
    }

    /// <summary>Every class name that derives from <c>DomainModelBase</c>, for the "more than one" case.</summary>
    public IReadOnlyList<string> AllEntryClassNames()
    {
        var names = new List<string>();
        foreach (SourceFile source in this.Sources)
        {
            foreach (Match match in Regex.Matches(
                source.Text,
                @"\bclass\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*:\s*DomainModelBase\b"))
            {
                names.Add(match.Groups["name"].Value);
            }
        }
        return names;
    }

    /// <summary>The namespaces declared across the project's source, deduplicated and ordered.</summary>
    public IReadOnlyList<string> Namespaces()
    {
        var found = new SortedSet<string>(StringComparer.Ordinal);
        foreach (SourceFile source in this.Sources)
        {
            foreach (Match match in Regex.Matches(
                source.Text, @"^\s*namespace\s+(?<ns>[A-Za-z_][A-Za-z0-9_.]*)", RegexOptions.Multiline))
            {
                found.Add(match.Groups["ns"].Value);
            }
        }
        return found.ToList();
    }

    /// <summary>Re-reads the sources from disk, for use after a write.</summary>
    public ModelProject Reload() => Open(this.Folder);

    /// <summary>True when <paramref name="relativePath"/> is build output or tooling.</summary>
    public static bool IsIgnoredPath(string relativePath)
    {
        string[] segments = relativePath.Split('/', '\\');
        return segments.Any(s => IgnoredFolders.Contains(s, StringComparer.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<SourceFile> ReadSources(string folder)
    {
        var sources = new List<SourceFile>();
        foreach (string path in Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories)
                     .OrderBy(p => p, StringComparer.Ordinal))
        {
            string relative = Path.GetRelativePath(folder, path);
            if (IsIgnoredPath(relative)) continue;

            sources.Add(new SourceFile(path, relative, File.ReadAllText(path)));
        }
        return sources;
    }
}

/// <summary>One C# file in the project, read once.</summary>
/// <param name="Path">Absolute path.</param>
/// <param name="RelativePath">Path relative to the project folder, for messages.</param>
/// <param name="Text">The file's contents.</param>
internal sealed record SourceFile(string Path, string RelativePath, string Text);

/// <summary>The class the framework loads, and the file it was found in.</summary>
/// <param name="Name">The class name - name #3 of four.</param>
/// <param name="Source">The file holding it.</param>
internal sealed record EntryClass(string Name, SourceFile Source);
