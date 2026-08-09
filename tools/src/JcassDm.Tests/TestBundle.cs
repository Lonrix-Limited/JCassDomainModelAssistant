using System;
using System.IO;
using System.Text;
using JcassDm;

namespace JcassDm.Tests;

/// <summary>
/// A throwaway copy of the reference model's bundle, plus a way to run the tool against it
/// and capture what it printed.
///
/// <para><b>The reference model's own bundle is never opened for writing by a test.</b> It
/// is committed content that the documentation, the walkthrough and S6's scaffolder all
/// depend on; a test that mutated it would leave the repository dirty in a way that looks
/// like somebody's uncommitted work. <see cref="SourcePath"/> is read once, copied, and not
/// touched again - and <see cref="ReferenceBundleGuard"/> checks that stayed true.</para>
/// </summary>
public sealed class TestBundle : IDisposable
{
    private readonly string _folder;

    private TestBundle(string folder, string path)
    {
        this._folder = folder;
        this.Path = path;
    }

    /// <summary>Path to this copy of the bundle.</summary>
    public string Path { get; }

    /// <summary>The reference model's bundle. Read-only as far as the tests are concerned.</summary>
    public static string SourcePath => System.IO.Path.Combine(
        RepoRoot(), "reference-model", "DomainModelSample", "domain_model_setup.xlsx");

    /// <summary>Copies the reference bundle into a fresh temporary folder.</summary>
    public static TestBundle FromReferenceModel()
    {
        string folder = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), "jcass-dm-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);

        string path = System.IO.Path.Combine(folder, "domain_model_setup.xlsx");
        File.Copy(SourcePath, path);
        return new TestBundle(folder, path);
    }

    /// <summary>An empty folder beside the bundle, for building deliberately broken workbooks.</summary>
    public string PathFor(string fileName) => System.IO.Path.Combine(this._folder, fileName);

    /// <summary>Runs the tool exactly as the executable would, and captures both streams.</summary>
    public ToolResult Run(params string[] args)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        int exitCode = Program.Run(args, output, error);
        return new ToolResult(exitCode, output.ToString(), error.ToString());
    }

    /// <summary>Convenience: dump this bundle and return stdout. Fails the run's exit code silently by design - callers assert on it.</summary>
    public string Dump()
    {
        ToolResult result = this.Run("dump", this.Path);
        return result.Output;
    }

    public void Dispose()
    {
        try { Directory.Delete(this._folder, recursive: true); }
        catch (IOException) { /* a leftover temp folder is not worth failing a test over */ }
    }

    /// <summary>
    /// Walks up from the test assembly until it finds the repository. Beats a relative path
    /// full of "..", which breaks the moment the project moves or the target framework changes.
    /// </summary>
    internal static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(System.IO.Path.Combine(directory.FullName, "reference-model"))
                && Directory.Exists(System.IO.Path.Combine(directory.FullName, "tools")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new InvalidOperationException(
            $"Could not find the repository root above {AppContext.BaseDirectory}.");
    }
}

/// <summary>What one invocation of the tool produced.</summary>
/// <param name="ExitCode">Process exit code - see <see cref="JcassDm.Cli.ExitCode"/>.</param>
/// <param name="Output">Everything written to stdout.</param>
/// <param name="Error">Everything written to stderr.</param>
public sealed record ToolResult(int ExitCode, string Output, string Error)
{
    /// <summary>Both streams together, for assertions that do not care which one carried the message.</summary>
    public string All => this.Output + this.Error;
}
