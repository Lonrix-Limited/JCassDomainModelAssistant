using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace JcassDm.Project;

/// <summary>
/// Which framework build a <c>refs\</c> folder holds, read out of the assemblies themselves.
///
/// <para><b>Never out of a note beside them.</b> A <c>FRAMEWORK-VERSION.txt</c> is written when a
/// folder is refreshed by the script and is simply absent when somebody copied files in by hand -
/// which is the case this exists to catch. The assemblies always carry their own answer: the
/// framework build stamps its git commit on to the informational version, so
/// <c>ProductVersion</c> reads <c>1.0.0+&lt;sha&gt;</c>.</para>
///
/// <para>This mirrors what <c>scripts/check-framework-version.ps1</c> does for the engineer, so
/// that the tool and the script cannot disagree about what a folder holds.</para>
/// </summary>
internal static class FrameworkStamp
{
    /// <summary>
    /// The framework commits present in a <c>refs\</c> folder, distinct and in file order.
    ///
    /// <para>Normally one. <b>More than one means the folder holds assemblies from two framework
    /// releases</b>, which is not harmless: the emitted <c>.csproj</c> references
    /// <c>refs\*.dll</c> with a wildcard, so the leftover is compiled against rather than
    /// ignored.</para>
    ///
    /// <para>Empty when the folder is missing, holds no framework assemblies, or holds assemblies
    /// whose version carries no commit - all of which are "cannot tell", never "up to date".</para>
    /// </summary>
    public static IReadOnlyList<string> ReadCommits(string? refsFolder)
    {
        if (refsFolder is null || !Directory.Exists(refsFolder)) return Array.Empty<string>();

        var commits = new List<string>();
        foreach (string file in Directory.GetFiles(refsFolder, "JCass_*.dll").OrderBy(f => f))
        {
            string? commit = ReadCommit(file);
            if (commit is not null && !commits.Contains(commit, StringComparer.OrdinalIgnoreCase))
            {
                commits.Add(commit);
            }
        }
        return commits;
    }

    /// <summary>The commit stamped on one assembly, or null when it carries none.</summary>
    private static string? ReadCommit(string assemblyPath)
    {
        try
        {
            string? product = FileVersionInfo.GetVersionInfo(assemblyPath).ProductVersion;
            if (string.IsNullOrWhiteSpace(product)) return null;

            int plus = product.IndexOf('+');
            if (plus < 0 || plus == product.Length - 1) return null;

            string commit = product[(plus + 1)..].Trim();
            return commit.Length == 0 ? null : commit;
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }

    /// <summary>The name of the stamp file the framework maintainers write into every <c>refs\</c> folder.</summary>
    public const string StampFileName = "FRAMEWORK-VERSION.txt";

    /// <summary>
    /// The .NET target the assemblies in a <c>refs\</c> folder were built for - <c>net10.0</c>, say -
    /// read from the <c>Build :</c> line of its <c>FRAMEWORK-VERSION.txt</c>.
    ///
    /// <para><b>Read from the stamp, never hard-coded in this tool.</b> The next major .NET move then
    /// needs a refreshed <c>refs\</c> and nothing else: the check follows the references it ships
    /// with. Unlike the commit, the target is not recoverable from the assemblies' version
    /// resource, so the stamp is the only source.</para>
    ///
    /// <para>Null when the folder, the file, the line or a target on it is missing - "cannot tell",
    /// which the caller reports as skipped rather than passed.</para>
    /// </summary>
    public static string? ReadBuildTarget(string? refsFolder)
    {
        if (refsFolder is null) return null;

        string stamp = Path.Combine(refsFolder, StampFileName);
        if (!File.Exists(stamp)) return null;

        try
        {
            foreach (string line in File.ReadLines(stamp))
            {
                string trimmed = line.Trim().TrimStart('﻿');
                if (!trimmed.StartsWith("Build", StringComparison.OrdinalIgnoreCase)) continue;

                int colon = trimmed.IndexOf(':');
                if (colon < 0) continue;

                Match target = Regex.Match(
                    trimmed[(colon + 1)..], @"\bnet\d+\.\d+\b", RegexOptions.IgnoreCase);
                return target.Success ? target.Value.ToLowerInvariant() : null;
            }
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }

        return null;
    }

    /// <summary>
    /// A target framework moniker in the words an engineer knows: <c>net10.0</c> is ".NET 10".
    /// Anything that is not a plain <c>netN.M</c> is returned as written.
    /// </summary>
    public static string DescribeTarget(string targetFramework)
    {
        Match match = Regex.Match(
            targetFramework, @"^net(?<major>\d+)\.(?<minor>\d+)$", RegexOptions.IgnoreCase);
        if (!match.Success) return targetFramework;

        string minor = match.Groups["minor"].Value;
        return minor == "0" ? $".NET {match.Groups["major"].Value}" : $".NET {match.Groups["major"].Value}.{minor}";
    }

    /// <summary>The first seven characters, which is how everything else here prints a commit.</summary>
    public static string Short(string commit)
        => commit.Length <= 7 ? commit : commit[..7];
}
