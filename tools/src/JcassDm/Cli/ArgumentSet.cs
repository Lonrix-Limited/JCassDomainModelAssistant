using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JcassDm.Cli;

/// <summary>
/// The parsed command line for one verb: a single positional argument (the bundle
/// path) plus <c>--option value</c> pairs and <c>--flag</c> switches.
///
/// <para><b>Unknown options are an error, never ignored.</b> That is the whole reason
/// this class exists rather than a dictionary lookup at each call site. An agent that
/// types <c>--budget_category</c> instead of <c>--budget-category</c> must be told, not
/// quietly handed a row with a blank budget category - which is a treatment that is
/// silently never funded, discovered a fortnight later in someone's forecast.</para>
///
/// <para>Verbs declare the options they accept via <see cref="Declare"/>, then read them.
/// <see cref="CheckForUnknownOptions"/> runs once the declarations are in.</para>
/// </summary>
public sealed class ArgumentSet
{
    private readonly Dictionary<string, string?> _options = new(StringComparer.Ordinal);
    private readonly HashSet<string> _declared = new(StringComparer.Ordinal);
    private readonly List<string> _positionals = new();

    private ArgumentSet() { }

    /// <summary>The verb, e.g. "dump". Empty when no verb was given.</summary>
    public string Verb { get; private set; } = string.Empty;

    /// <summary>Positional arguments after the verb, in order.</summary>
    public IReadOnlyList<string> Positionals => this._positionals;

    /// <summary>
    /// Splits <paramref name="args"/> into a verb, positionals and options. Accepts both
    /// <c>--name value</c> and <c>--name=value</c>; a <c>--flag</c> with no value is stored
    /// with a null value and read back through <see cref="Flag"/>.
    /// </summary>
    public static ArgumentSet Parse(string[] args)
    {
        var set = new ArgumentSet();
        if (args.Length == 0) return set;

        set.Verb = args[0];

        for (int i = 1; i < args.Length; i++)
        {
            string arg = args[i];

            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                set._positionals.Add(arg);
                continue;
            }

            string name;
            string? value;

            int equals = arg.IndexOf('=', StringComparison.Ordinal);
            if (equals >= 0)
            {
                name = arg[..equals];
                value = arg[(equals + 1)..];
            }
            else
            {
                name = arg;
                // A value follows unless the next token is another option or there is no next
                // token. This is what lets --force sit anywhere in the line.
                bool nextIsValue = i + 1 < args.Length
                    && !args[i + 1].StartsWith("--", StringComparison.Ordinal);
                value = nextIsValue ? args[++i] : null;
            }

            if (name.Length <= 2)
            {
                throw new UsageFailure("Found '--' on its own. Options look like --name value.");
            }

            if (set._options.ContainsKey(name))
            {
                throw new UsageFailure(
                    $"Option '{name}' was given more than once. Give it once, with the value you want.");
            }

            set._options[name] = value;
        }

        return set;
    }

    /// <summary>
    /// Declares an option name as recognised by the current verb. Reading an option
    /// declares it, so this is only needed for names a verb accepts but may not read.
    /// </summary>
    public void Declare(params string[] names)
    {
        foreach (string name in names) this._declared.Add(name);
    }

    /// <summary>
    /// Fails if the caller passed an option the verb does not recognise. Call this after
    /// every option has been read or declared, and before doing any work.
    /// </summary>
    public void CheckForUnknownOptions()
    {
        var unknown = this._options.Keys.Where(k => !this._declared.Contains(k)).OrderBy(k => k, StringComparer.Ordinal).ToList();
        if (unknown.Count == 0) return;

        string known = string.Join(", ", this._declared.OrderBy(k => k, StringComparer.Ordinal));
        throw new UsageFailure(
            $"Unrecognised option{(unknown.Count > 1 ? "s" : "")}: {string.Join(", ", unknown)}." +
            Environment.NewLine +
            $"'{this.Verb}' accepts: {known}");
    }

    /// <summary>True when the named switch was present. Declares the name.</summary>
    public bool Flag(string name)
    {
        this._declared.Add(name);
        if (!this._options.TryGetValue(name, out string? value)) return false;

        // --force true / --force false are accepted because somebody will write them.
        if (value is null) return true;
        if (bool.TryParse(value, out bool parsed)) return parsed;

        throw new UsageFailure($"Option '{name}' is a switch. Pass it on its own, not as '{name} {value}'.");
    }

    /// <summary>
    /// The value of an optional option, or null when absent. Declares the name, and any
    /// aliases, so <c>--display-name</c> and <c>--model-name</c> can mean the same thing.
    /// </summary>
    public string? Optional(string name, params string[] aliases)
    {
        this._declared.Add(name);
        foreach (string alias in aliases) this._declared.Add(alias);

        string? found = null;
        string? foundUnder = null;

        foreach (string candidate in new[] { name }.Concat(aliases))
        {
            if (!this._options.TryGetValue(candidate, out string? value)) continue;
            if (value is null)
            {
                throw new UsageFailure($"Option '{candidate}' needs a value, e.g. {candidate} <value>.");
            }
            if (foundUnder is not null)
            {
                throw new UsageFailure(
                    $"'{foundUnder}' and '{candidate}' are the same option. Give one of them.");
            }
            found = value;
            foundUnder = candidate;
        }

        return found;
    }

    /// <summary>The value of a required option. Declares the name and any aliases.</summary>
    public string Required(string name, params string[] aliases)
    {
        string? value = this.Optional(name, aliases);
        if (value is null)
        {
            throw new UsageFailure($"'{this.Verb}' needs {name} <value>.");
        }
        if (value.Length == 0)
        {
            throw new UsageFailure($"Option '{name}' was given an empty value.");
        }
        return value;
    }

    /// <summary>An optional numeric option, parsed invariantly so a comma decimal separator cannot change meaning between machines.</summary>
    public double? OptionalNumber(string name)
    {
        string? raw = this.Optional(name);
        if (raw is null) return null;

        if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            throw new UsageFailure($"Option '{name}' expects a number, but got '{raw}'. Use a full stop as the decimal separator.");
        }
        return value;
    }

    /// <summary>An optional integer option.</summary>
    public int? OptionalInteger(string name)
    {
        string? raw = this.Optional(name);
        if (raw is null) return null;

        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
        {
            throw new UsageFailure($"Option '{name}' expects a whole number, but got '{raw}'.");
        }
        return value;
    }

    /// <summary>
    /// The bundle path: the one positional argument every verb in this tool takes.
    /// </summary>
    public string BundlePath()
    {
        if (this._positionals.Count == 0)
        {
            throw new UsageFailure($"'{this.Verb}' needs the path to a domain_model_setup.xlsx file.");
        }
        if (this._positionals.Count > 1)
        {
            throw new UsageFailure(
                $"'{this.Verb}' takes one bundle path, but got {this._positionals.Count}: " +
                string.Join(", ", this._positionals.Select(p => $"'{p}'")) + "." + Environment.NewLine +
                "A path containing spaces needs quoting.");
        }
        return this._positionals[0];
    }
}
