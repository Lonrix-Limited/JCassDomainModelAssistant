using System;
using System.IO;

namespace JcassDm.Project;

/// <summary>
/// Finds the Domain Model Assistant checkout that <c>jcass-dm.exe</c> is sitting in, so the
/// scaffolder can seed a new project's <c>refs\</c> from it.
///
/// <para>Everything here is best-effort and nothing is required. The tool works standalone -
/// somebody may well have copied the exe on to a machine by itself - and in that case a scaffold
/// still produces a complete, correct project that simply cannot be compiled until framework
/// reference assemblies are put beside it. The scaffolder says so plainly rather than failing,
/// because a partial answer with an explanation beats a refusal.</para>
/// </summary>
internal static class AssistantLayout
{
    /// <summary>
    /// The Assistant repository root, or null when the exe is not inside one.
    ///
    /// <para>Walks up from the executable looking for the two folders that together identify it.
    /// Two rather than one, because <c>refs</c> alone is a common enough folder name to match by
    /// accident and seeding a project from the wrong folder would be near-impossible to
    /// diagnose.</para>
    /// </summary>
    public static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "refs"))
                && Directory.Exists(Path.Combine(directory.FullName, "reference-model")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        return null;
    }

    /// <summary>The canonical <c>refs\</c> folder, or null when it cannot be found or is empty.</summary>
    public static string? FindRefsFolder()
    {
        string? root = FindRepositoryRoot();
        if (root is null) return null;

        string refs = Path.Combine(root, "refs");
        if (!Directory.Exists(refs)) return null;
        if (Directory.GetFiles(refs, "*.dll").Length == 0) return null;

        return refs;
    }

    /// <summary>The reference model's bundle, which <c>--from-sample</c> copies. Null when absent.</summary>
    public static string? FindSampleBundle()
    {
        string? root = FindRepositoryRoot();
        if (root is null) return null;

        string bundle = Path.Combine(
            root, "reference-model", "DomainModelSample", ModelProject.BundleFileName);
        return File.Exists(bundle) ? bundle : null;
    }
}
