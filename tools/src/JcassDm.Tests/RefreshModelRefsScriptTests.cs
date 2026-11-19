using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace JcassDm.Tests;

/// <summary>
/// <c>scripts/refresh-model-refs.ps1</c> and the engineer's project file.
///
/// <para><b>The script may say the project's target is wrong; it may change it only with
/// permission.</b> Permission is the <c>-UpdateTargetFramework</c> switch or a "y" at a prompt.
/// These tests run it the way an AI assistant does - no console to answer on - which is the case
/// that must leave the file alone, byte for byte. A script that rewrote the engineer's file
/// unasked would break the rule the agent itself follows.</para>
///
/// <para>Runs PowerShell as a child process: <c>pwsh</c> where it is installed (CI, on Linux),
/// otherwise Windows PowerShell. Skipped when neither is found rather than passed.</para>
/// </summary>
public class RefreshModelRefsScriptTests
{
    [PowerShellFact]
    public void Without_permission_the_project_file_is_byte_identical()
    {
        using TemporaryModel model = FixtureModels.Copy("target-framework-behind");
        string csproj = model.PathTo("FixtureModel.csproj");
        byte[] before = File.ReadAllBytes(csproj);

        ScriptResult result = RunScript(model);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(before, File.ReadAllBytes(csproj));
        Assert.Contains("was NOT changed", result.Output, StringComparison.Ordinal);
        Assert.Contains("jcass-dm.exe check", result.Output, StringComparison.Ordinal);
    }

    [PowerShellFact]
    public void With_permission_only_the_target_value_changes()
    {
        using TemporaryModel model = FixtureModels.Copy("target-framework-behind");
        string csproj = model.PathTo("FixtureModel.csproj");
        byte[] before = File.ReadAllBytes(csproj);
        string expectedTarget = ExpectedTarget();

        ScriptResult result = RunScript(model, "-UpdateTargetFramework");

        Assert.Equal(0, result.ExitCode);

        // Byte-level: the original with exactly one substitution, so encoding, BOM and line
        // endings are all proved unchanged, not just the text.
        byte[] expected = Replace(before,
            System.Text.Encoding.UTF8.GetBytes("<TargetFramework>net9.0</TargetFramework>"),
            System.Text.Encoding.UTF8.GetBytes($"<TargetFramework>{expectedTarget}</TargetFramework>"));
        Assert.Equal(expected, File.ReadAllBytes(csproj));
    }

    [PowerShellFact]
    public void Two_project_files_are_never_edited_even_with_permission()
    {
        // An Explorer copy sorts BEFORE the real file ("FixtureModel - Copy" < "FixtureModel."),
        // so picking the first one would edit the copy and report success.
        using TemporaryModel model = FixtureModels.Copy("target-framework-behind");
        string csproj = model.PathTo("FixtureModel.csproj");
        string copy = model.PathTo("FixtureModel - Copy.csproj");
        File.Copy(csproj, copy);
        byte[] before = File.ReadAllBytes(csproj);

        ScriptResult result = RunScript(model, "-UpdateTargetFramework");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(before, File.ReadAllBytes(csproj));
        Assert.Equal(before, File.ReadAllBytes(copy));
        Assert.Contains("changed none of them", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Now rebuild your model", result.Output, StringComparison.Ordinal);
    }

    [PowerShellFact]
    public void A_project_already_on_the_right_target_hears_nothing_about_it()
    {
        using TemporaryModel model = FixtureModels.Copy("healthy");
        string csproj = model.PathTo("FixtureModel.csproj");
        byte[] before = File.ReadAllBytes(csproj);

        ScriptResult result = RunScript(model);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(before, File.ReadAllBytes(csproj));
        Assert.DoesNotContain("YOUR PROJECT TARGETS", result.Output, StringComparison.Ordinal);
        Assert.Contains("Now rebuild your model", result.Output, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------------------

    private sealed record ScriptResult(int ExitCode, string Output);

    /// <summary>The target the Assistant's own references are stamped with.</summary>
    private static string ExpectedTarget()
    {
        string? target = JcassDm.Project.FrameworkStamp.ReadBuildTarget(Path.Combine(TestBundle.RepoRoot(), "refs"));
        Assert.False(target is null, "The Assistant's refs\\FRAMEWORK-VERSION.txt names no build target.");
        return target!;
    }

    private static ScriptResult RunScript(TemporaryModel model, params string[] extra)
    {
        string shell = PowerShellFactAttribute.FindPowerShell()
            ?? throw new InvalidOperationException("No PowerShell; the attribute should have skipped this test.");

        string script = Path.Combine(TestBundle.RepoRoot(), "scripts", "refresh-model-refs.ps1");
        var start = new ProcessStartInfo(shell)
        {
            RedirectStandardInput = true,   // no console to answer on, as for an AI assistant
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (string arg in new[] { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass",
                     "-File", script, "-Project", model.Folder }.Concat(extra))
        {
            start.ArgumentList.Add(arg);
        }

        using Process process = Process.Start(start)!;
        process.StandardInput.Close();
        string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        process.WaitForExit();
        return new ScriptResult(process.ExitCode, output);
    }

    private static byte[] Replace(byte[] haystack, byte[] needle, byte[] replacement)
    {
        for (int i = 0; i <= haystack.Length - needle.Length; i++)
        {
            if (!haystack.AsSpan(i, needle.Length).SequenceEqual(needle)) continue;
            return haystack[..i].Concat(replacement).Concat(haystack[(i + needle.Length)..]).ToArray();
        }
        throw new InvalidOperationException("The target line was not found in the fixture's project file.");
    }
}

/// <summary>
/// A fact that needs a PowerShell to run a script with, and is skipped - visibly, not passed -
/// on a machine that has none.
/// </summary>
public sealed class PowerShellFactAttribute : FactAttribute
{
    public PowerShellFactAttribute()
    {
        if (FindPowerShell() is null) this.Skip = "No PowerShell found on PATH.";
    }

    /// <summary><c>pwsh</c> if it is installed, otherwise Windows PowerShell on Windows; null when neither is.</summary>
    public static string? FindPowerShell()
    {
        string[] names = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new[] { "pwsh.exe", "powershell.exe" }
            : new[] { "pwsh" };

        string[] path = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        foreach (string name in names)
        {
            foreach (string folder in path)
            {
                string candidate = Path.Combine(folder, name);
                if (File.Exists(candidate)) return candidate;
            }
        }
        return null;
    }
}
