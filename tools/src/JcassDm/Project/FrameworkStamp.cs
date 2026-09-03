using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

    /// <summary>The first seven characters, which is how everything else here prints a commit.</summary>
    public static string Short(string commit)
        => commit.Length <= 7 ? commit : commit[..7];
}
