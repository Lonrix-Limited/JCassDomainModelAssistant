using System;
using System.Linq;
using JcassDm.Cli;

namespace JcassDm.Project;

/// <summary>
/// The one name a domain model has.
///
/// <para><b>There is deliberately only one.</b> Four strings have to be identical for a model to
/// load - the <c>.csproj</c> filename, the assembly name it implies, the entry class, and
/// <c>meta.main_dll</c> / <c>meta.main_class</c> in the bundle - and they disagree only because a
/// person can change one without changing the others. Everything downstream of this type derives
/// all four from <see cref="Value"/>, so there is no argument a caller can pass that makes them
/// disagree. If you ever find yourself adding an option that sets one of the four
/// independently, that is the failure this type exists to make impossible.</para>
///
/// <para>The element class name is <i>not</i> one of the four and does get its own option: it is
/// an ordinary C# type name that nothing outside the project resolves by string.</para>
/// </summary>
public sealed class ModelName
{
    private ModelName(string value)
    {
        this.Value = value;
    }

    /// <summary>The name itself, e.g. <c>MyRoadModel</c>.</summary>
    public string Value { get; }

    /// <summary>The <c>.csproj</c> file name. Name #1 of four.</summary>
    public string ProjectFileName => this.Value + ".csproj";

    /// <summary>
    /// The assembly name. Name #2 of four - inherited from #1 because <c>&lt;AssemblyName&gt;</c>
    /// is never written into a scaffolded project.
    /// </summary>
    public string AssemblyName => this.Value;

    /// <summary>The entry class name. Name #3 of four.</summary>
    public string ClassName => this.Value;

    /// <summary>The file the entry class lives in.</summary>
    public string ClassFileName => this.Value + ".cs";

    /// <summary>The <c>meta.main_dll</c> value. Name #4a of four.</summary>
    public string MainDll => this.Value + ".dll";

    /// <summary>The <c>meta.main_class</c> value. Name #4b of four.</summary>
    public string MainClass => this.Value;

    /// <summary>The default namespace for a scaffolded project. Not part of the four-name rule.</summary>
    public string DefaultNamespace => this.Value + ".Objects";

    /// <summary>
    /// Validates a name and returns it, or fails with something the caller can act on.
    ///
    /// <para>The rules are the intersection of three things the name has to be at once: a legal
    /// C# type name, a legal file stem on Windows and Linux, and something a person will type the
    /// same way twice. Anything that satisfies all three is allowed; nothing is silently
    /// corrected, because a name quietly changed here reappears as a mismatch nobody can see.</para>
    /// </summary>
    /// <param name="value">The candidate name.</param>
    /// <param name="what">How to describe it in a failure message, e.g. "model name".</param>
    public static ModelName Parse(string value, string what = "model name")
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new UsageFailure($"The {what} cannot be empty.");
        }
        if (value.Trim().Length != value.Length)
        {
            throw new UsageFailure(
                $"The {what} is '{value}', with a leading or trailing space. " +
                "Four different files have to carry this name and they are compared exactly, " +
                "so a space that nobody can see would break the model. Pass it without.");
        }
        if (value.Contains('.'))
        {
            throw new UsageFailure(
                $"The {what} is '{value}'. It must be a single name with no full stops - it becomes " +
                "the .csproj filename, the assembly name and the class name all at once, and only a " +
                "class name may be qualified. Use the namespace option if you want a dotted namespace.");
        }
        if (value.Any(c => c == '/' || c == '\\'))
        {
            throw new UsageFailure(
                $"The {what} is '{value}'. It is a name, not a path. " +
                "Use --output to say where the project folder goes.");
        }
        if (!char.IsLetter(value[0]) && value[0] != '_')
        {
            throw new UsageFailure(
                $"The {what} is '{value}', which is not a valid C# class name: it must start with a " +
                "letter or an underscore.");
        }
        if (value.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
        {
            throw new UsageFailure(
                $"The {what} is '{value}', which is not a valid C# class name. " +
                "Use letters, digits and underscores only - no spaces, hyphens or punctuation.");
        }
        if (IsReservedWindowsName(value))
        {
            // Cheap to check and impossible to diagnose afterwards: Windows refuses to create
            // CON.csproj, and the failure arrives as an unhelpful IO error.
            throw new UsageFailure(
                $"The {what} is '{value}', which Windows reserves as a device name. " +
                "The .csproj file could not be created. Pick another name.");
        }

        return new ModelName(value);
    }

    /// <summary>Formats the name for messages.</summary>
    public override string ToString() => this.Value;

    private static bool IsReservedWindowsName(string value)
    {
        string[] reserved =
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
        };
        return reserved.Contains(value, StringComparer.OrdinalIgnoreCase);
    }
}
