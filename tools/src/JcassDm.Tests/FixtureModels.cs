using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JcassDm.Tests;

/// <summary>
/// The deliberately broken models in <c>tools/fixtures/</c>, and a way to work on a throwaway
/// copy of one.
///
/// <para><b>Every <c>check</c> rule has a fixture that trips it.</b> A rule that has never been
/// seen to fail has never been tested - it may be reading the wrong column, or looking at a file
/// that is not there, and it would report OK either way. Testing only the healthy case would
/// prove the output formatting and nothing else.</para>
///
/// <para>The fixtures carry no <c>refs/</c>, so they do not compile - and they do not need to.
/// <c>check</c> reads C# as text. Keeping them source-only holds them to a few kilobytes each in
/// a repository that already commits a 37 MB executable.</para>
/// </summary>
public static class FixtureModels
{
    /// <summary>Every fixture folder name, and the one thing each is designed to break.</summary>
    public static readonly IReadOnlyDictionary<string, string> All =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["healthy"] = "nothing - the control, and the only one check should pass",
            ["names-disagree"] = "the .csproj was renamed and the entry class was not",
            ["parameter-not-written"] = "par_obj is on the parameters sheet and not in SetParameterValues",
            ["treatment-not-in-bundle"] = "TreatmentNames.Reseal has no row on the treatments sheet",
            ["treatment-not-in-code"] = "the treatments sheet declares 'overlay' and no C# mentions it",
            ["missing-reset-arm"] = "'replace' is triggered and funded, and Resetter has no case for it",
            ["two-csproj"] = "two .csproj files at the root, which stops a debug run before it starts",
            ["assembly-name-set"] = "<AssemblyName> is set to something other than the .csproj stem",
            ["blank-budget-category"] = "RMaint has no budget_category, so the run throws at setup naming the treatment",
        };

    /// <summary>Absolute path to a fixture, as committed. Read-only as far as the tests care.</summary>
    public static string PathTo(string name) => Path.Combine(TestBundle.RepoRoot(), "tools", "fixtures", name);

    /// <summary>
    /// Copies a fixture into a temporary folder so a test can rename, package or otherwise
    /// mutate it without touching committed content.
    /// </summary>
    public static TemporaryModel Copy(string name) => TemporaryModel.From(PathTo(name));
}

/// <summary>A throwaway copy of a model folder, deleted when the test finishes with it.</summary>
public sealed class TemporaryModel : IDisposable
{
    private TemporaryModel(string folder)
    {
        this.Folder = folder;
    }

    /// <summary>Absolute path to the copy.</summary>
    public string Folder { get; }

    /// <summary>An absolute path inside the copy.</summary>
    public string PathTo(params string[] parts) => Path.Combine(new[] { this.Folder }.Concat(parts).ToArray());

    /// <summary>Copies <paramref name="source"/> into a fresh temporary folder.</summary>
    public static TemporaryModel From(string source)
    {
        string folder = Path.Combine(Path.GetTempPath(), "jcass-dm-tests", Guid.NewGuid().ToString("N"));
        CopyDirectory(source, folder);
        return new TemporaryModel(folder);
    }

    /// <summary>An empty temporary folder, for scaffolding into.</summary>
    public static TemporaryModel Empty()
    {
        string folder = Path.Combine(Path.GetTempPath(), "jcass-dm-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return new TemporaryModel(folder);
    }

    /// <summary>Runs the tool against this copy and captures both streams.</summary>
    public ToolResult Run(params string[] args)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        int exitCode = JcassDm.Program.Run(args, output, error);
        return new ToolResult(exitCode, output.ToString(), error.ToString());
    }

    /// <summary>
    /// A single hash over every file in the folder - names and contents. Comparing this either
    /// side of a failed operation is how "nothing was half-written" gets asserted.
    /// </summary>
    public string Fingerprint()
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var lines = new List<string>();

        foreach (string path in Directory.EnumerateFiles(this.Folder, "*", SearchOption.AllDirectories)
                     .OrderBy(p => p, StringComparer.Ordinal))
        {
            string relative = Path.GetRelativePath(this.Folder, path).Replace('\\', '/');
            string hash = Convert.ToHexString(sha.ComputeHash(File.ReadAllBytes(path)));
            lines.Add($"{relative} {hash}");
        }

        return string.Join("\n", lines);
    }

    public void Dispose()
    {
        try
        {
            // Anything the test made read-only has to be cleared, or the delete fails and the
            // temporary folders pile up across a run.
            foreach (string path in Directory.EnumerateFiles(this.Folder, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }
            Directory.Delete(this.Folder, recursive: true);
        }
        catch (Exception)
        {
            // A leftover temporary folder is not worth failing a test over.
        }
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);

        foreach (string path in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(source, path);
            string destination = Path.Combine(target, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(path, destination);
        }
    }
}
