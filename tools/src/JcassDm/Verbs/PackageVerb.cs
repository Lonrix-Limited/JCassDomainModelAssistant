using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JcassDm.Cli;
using JcassDm.Project;

namespace JcassDm.Verbs;

/// <summary>
/// <c>jcass-dm package [--project &lt;path&gt;] [--output &lt;path to the .zip&gt;] [--force]</c>
///
/// <para>Builds the zip the web Debug Model page wants: source only, and opening straight to the
/// <c>.csproj</c>.</para>
///
/// <para><b>Both halves of that are mistakes people make by hand, and both cost an afternoon.</b>
/// Zipping the <i>folder</i> rather than its <i>contents</i> puts everything one level deep and
/// F5 fails with "No .csproj file found at workspace root" - which reads as a problem with the
/// project rather than with the zip. And including <c>refs\</c> overwrites part of the workspace's
/// own staged framework assemblies with reference assemblies that cannot be executed, so the
/// failure arrives later still, at F5, looking like nothing to do with a zip.</para>
///
/// <para><c>bin\</c> and <c>obj\</c> are filtered by the upload anyway; they are left out here so
/// that what the engineer sends is what they meant to send.</para>
/// </summary>
internal static class PackageVerb
{
    /// <summary>Folders never included, and why each one matters.</summary>
    private static readonly (string Folder, string Reason)[] Excluded =
    {
        ("refs", "the debug workspace stages its own, and these cannot be executed"),
        ("bin", "build output"),
        ("obj", "build output"),
        (".git", "version control"),
        (".vs", "editor state"),
        (".idea", "editor state"),
    };

    public static int Run(ArgumentSet args, TextWriter output)
    {
        string projectPath = args.Optional("--project") ?? ".";
        string? outputOption = args.Optional("--output");
        bool force = args.Flag("--force");
        args.CheckForUnknownOptions();

        ModelProject project = ModelProject.Open(projectPath);

        string zipPath = Path.GetFullPath(
            outputOption ?? Path.Combine(project.Folder, project.ProjectStem + "_for_debug.zip"));

        if (File.Exists(zipPath) && !force)
        {
            throw new ConflictFailure(
                $"'{zipPath}' already exists. Nothing was written." + Environment.NewLine +
                "Re-run with --force to replace it, or give --output another path.");
        }

        var entries = CollectEntries(project, zipPath).ToList();
        RequireProjectAtRoot(entries, project);

        Directory.CreateDirectory(Path.GetDirectoryName(zipPath)!);

        // Written to a temporary file and moved into place, so an interrupted run cannot leave a
        // truncated zip that looks finished and fails halfway through an upload.
        string temporary = zipPath + ".jcass-dm-partial";
        try
        {
            if (File.Exists(temporary)) File.Delete(temporary);

            using (var archive = new ZipArchive(File.Create(temporary), ZipArchiveMode.Create))
            {
                foreach (PackageEntry entry in entries)
                {
                    archive.CreateEntryFromFile(entry.SourcePath, entry.EntryName, CompressionLevel.Optimal);
                }
            }

            File.Move(temporary, zipPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                try { File.Delete(temporary); } catch (IOException) { /* best effort */ }
            }
        }

        WriteSummary(output, project, zipPath, entries);
        return ExitCode.Ok;
    }

    private static IEnumerable<PackageEntry> CollectEntries(ModelProject project, string zipPath)
    {
        foreach (string path in Directory.EnumerateFiles(project.Folder, "*", SearchOption.AllDirectories)
                     .OrderBy(p => p, StringComparer.Ordinal))
        {
            // A zip written into the project folder must not include itself, nor the last one.
            if (string.Equals(Path.GetFullPath(path), zipPath, StringComparison.OrdinalIgnoreCase)) continue;
            if (path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) continue;

            string relative = Path.GetRelativePath(project.Folder, path);
            if (ModelProject.IsIgnoredPath(relative)) continue;

            // Forward slashes: the zip format specifies them, and the sidecar unpacks on Linux.
            yield return new PackageEntry(path, relative.Replace('\\', '/'));
        }
    }

    /// <summary>
    /// Asserts the property the whole verb exists for, against the entries about to be written
    /// rather than against the folder they came from.
    ///
    /// <para>Checking the output rather than the input is the point: it is the assertion that
    /// would have caught the mistake this verb replaces, and it costs nothing.</para>
    /// </summary>
    private static void RequireProjectAtRoot(IReadOnlyList<PackageEntry> entries, ModelProject project)
    {
        var rootProjects = entries
            .Where(e => !e.EntryName.Contains('/', StringComparison.Ordinal))
            .Where(e => e.EntryName.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (rootProjects.Count == 1) return;

        throw new BundleFailure(
            $"The zip would not open straight to a .csproj, so it was not written. " +
            $"Found {rootProjects.Count} at the top level." + Environment.NewLine +
            "This is a bug in jcass-dm rather than a problem with " + project.Folder + " - " +
            "please report it.");
    }

    private static void WriteSummary(
        TextWriter output, ModelProject project, string zipPath, IReadOnlyList<PackageEntry> entries)
    {
        var bySize = new FileInfo(zipPath);

        output.WriteLine($"packaged   {entries.Count} file{(entries.Count == 1 ? "" : "s")} " +
                         $"({bySize.Length / 1024.0:0.#} KB)");
        output.WriteLine($"           {zipPath}");
        output.WriteLine();
        output.WriteLine("It opens straight to:");
        foreach (PackageEntry entry in entries.Where(e => !e.EntryName.Contains('/', StringComparison.Ordinal)))
        {
            output.WriteLine("  " + entry.EntryName);
        }

        var folders = entries
            .Where(e => e.EntryName.Contains('/', StringComparison.Ordinal))
            .Select(e => e.EntryName[..e.EntryName.IndexOf('/', StringComparison.Ordinal)] + "/")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(f => f, StringComparer.Ordinal);
        foreach (string folder in folders)
        {
            output.WriteLine("  " + folder);
        }

        var leftOut = Excluded
            .Where(x => Directory.Exists(Path.Combine(project.Folder, x.Folder)))
            .ToList();

        if (leftOut.Count > 0)
        {
            output.WriteLine();
            output.WriteLine("Left out:");
            foreach ((string folder, string reason) in leftOut)
            {
                output.WriteLine($"  {folder}\\  - {reason}");
            }
        }

        if (!project.HasBundle)
        {
            output.WriteLine();
            output.WriteLine($"warning    There is no {ModelProject.BundleFileName} in this project, so it is not in");
            output.WriteLine("           the zip either. A debug run derives the DLL and class names from the .csproj");
            output.WriteLine("           and will start without it, but a normal run reads it and will not.");
        }

        output.WriteLine();
        output.WriteLine("Upload this on the web app's Debug Model page, then initialise the workspace.");
    }
}

/// <summary>One file going into the zip.</summary>
/// <param name="SourcePath">Absolute path on disk.</param>
/// <param name="EntryName">Path inside the zip, forward-slashed and relative to the project root.</param>
internal sealed record PackageEntry(string SourcePath, string EntryName);
