using System;
using System.Linq;
using JcassDm.Cli;

namespace JcassDm.Verbs;

/// <summary>
/// Checks on the names that go into a bundle.
///
/// <para>All structural, per Locked decision 6 - whether a name is spelled the way the
/// framework can use it, never whether it is a sensible name for a treatment. The web
/// app's Check Setup owns the second question.</para>
///
/// <para>Leading and trailing spaces get their own rule because they are the worst kind of
/// bug this file can prevent: a treatment written into the sheet as <c>"repair "</c> looks
/// identical to <c>"repair"</c> everywhere a person would look, and matches nothing in the
/// C# that has to agree with it.</para>
/// </summary>
internal static class NameRules
{
    /// <summary>
    /// A name destined for a cell that C# has to match exactly. Returns it unchanged, or
    /// fails - it never quietly trims, because a caller that passed a padded name has a bug
    /// somewhere upstream and hiding it here only moves the surprise.
    /// </summary>
    public static string RequireClean(string value, string optionName)
    {
        if (value.Length == 0)
        {
            throw new UsageFailure($"'{optionName}' cannot be empty.");
        }
        if (value.Trim().Length != value.Length)
        {
            throw new UsageFailure(
                $"'{optionName}' is '{value}', with a leading or trailing space." + Environment.NewLine +
                "That would not match the same name in your C#, and the two look identical on screen. " +
                "Pass the name without the space.");
        }
        if (value.Any(char.IsControl))
        {
            throw new UsageFailure($"'{optionName}' contains a tab or line break. Names must be a single plain word or phrase.");
        }
        return value;
    }

    /// <summary>
    /// A <c>main_class</c> value: a C# type name, optionally namespace-qualified. Checked
    /// because the framework resolves the entry class by this string at load time, and an
    /// unloadable class reads as "Domain Model class not found" long after the typo.
    /// </summary>
    public static string RequireTypeName(string value, string optionName)
    {
        RequireClean(value, optionName);

        if (value.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            throw new UsageFailure(
                $"'{optionName}' is the CLASS name, not the file name - drop the .dll. " +
                $"For '{value}' you probably want '{value[..^4]}'.");
        }

        foreach (string segment in value.Split('.'))
        {
            if (segment.Length == 0 || (!char.IsLetter(segment[0]) && segment[0] != '_')
                || segment.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
            {
                throw new UsageFailure(
                    $"'{optionName}' is '{value}', which is not a valid C# class name. " +
                    "Use letters, digits and underscores, starting with a letter; " +
                    "a namespace prefix separated by full stops is allowed.");
            }
        }
        return value;
    }

    /// <summary>
    /// A <c>main_dll</c> value: a file name ending in .dll, with no folder in it.
    /// The extension is required rather than added, because appending it silently would be
    /// this tool guessing at the one string the four-name rule is about.
    /// </summary>
    public static string RequireDllFileName(string value, string optionName)
    {
        RequireClean(value, optionName);

        if (value.Contains('/') || value.Contains('\\'))
        {
            throw new UsageFailure(
                $"'{optionName}' is '{value}'. It is a file name, not a path - the framework looks for it " +
                "beside the bundle.");
        }
        if (!value.EndsWith(".dll", StringComparison.Ordinal))
        {
            throw new UsageFailure(
                $"'{optionName}' is '{value}'. It must be the compiled file name including the extension, " +
                $"e.g. '{value}.dll'.");
        }
        if (value.Length == 4)
        {
            throw new UsageFailure($"'{optionName}' is just '.dll' with no name in front of it.");
        }
        return value;
    }

    /// <summary>A <c>data_type</c> value. Restricted to what the framework understands.</summary>
    public static string RequireDataType(string value, string optionName)
    {
        if (DataTypesInclude(value)) return value;

        throw new UsageFailure(
            $"'{optionName}' is '{value}'. It must be one of: {string.Join(", ", Bundle.DataTypes.Writable)}." +
            Environment.NewLine +
            "This is checked because the framework treats any value that is not 'text' as numeric, " +
            "so a typo produces a numeric column rather than an error.");
    }

    private static bool DataTypesInclude(string value)
        => Bundle.DataTypes.Writable.Any(t => string.Equals(t, value, StringComparison.Ordinal));
}
