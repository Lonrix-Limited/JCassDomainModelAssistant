using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JcassDm.Project;

/// <summary>
/// What can be learned about a domain model by reading its source as text.
///
/// <para><b>This is a heuristic and it says so.</b> Reading C# properly would mean Roslyn, which
/// would multiply the size of a tool that has to be committed into a public repository, for
/// findings a compiler already gives you. Everything here is deliberately shaped so that the
/// failure mode is <i>missing</i> a problem rather than inventing one: where a pattern cannot be
/// recognised, <see cref="Confidence"/> records that and <c>check</c> reports the rule as skipped
/// instead of passing it. A check that quietly turns into a no-op is worse than no check.</para>
/// </summary>
internal sealed class SourceFacts
{
    private SourceFacts() { }

    /// <summary>
    /// Treatment names declared as <c>const string</c> on a class called <c>TreatmentNames</c>,
    /// mapped from the constant's identifier to its value.
    /// </summary>
    public IReadOnlyDictionary<string, string> TreatmentNameConstants { get; private init; }
        = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>Every <c>const string</c> in the project, by identifier. Used to resolve set names.</summary>
    public IReadOnlyDictionary<string, string> StringConstants { get; private init; }
        = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>Every string literal anywhere in the project's source.</summary>
    public IReadOnlySet<string> AllStringLiterals { get; private init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>String literals written inside a <c>SetParameterValues</c> method body.</summary>
    public IReadOnlySet<string> ParametersWritten { get; private init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>True when at least one <c>SetParameterValues</c> method was found.</summary>
    public bool FoundSetParameterValues { get; private init; }

    /// <summary>Treatment names appearing as a <c>switch</c> case label anywhere in the project.</summary>
    public IReadOnlySet<string> TreatmentsWithCaseArm { get; private init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>True when a <c>switch</c> over something ending in <c>TreatmentName</c> was found.</summary>
    public bool FoundTreatmentSwitch { get; private init; }

    /// <summary>Lookup set names the code asks for, resolved through <see cref="StringConstants"/> where needed.</summary>
    public IReadOnlySet<string> LookupSetsUsed { get; private init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Lookup set arguments that could not be resolved to a literal, for honest reporting.</summary>
    public IReadOnlyList<string> UnresolvedLookupSetArguments { get; private init; } = Array.Empty<string>();

    /// <summary>
    /// Reads every source file once and extracts everything <c>check</c> needs.
    /// </summary>
    public static SourceFacts Read(IReadOnlyList<SourceFile> sources)
    {
        var stringConstants = new Dictionary<string, string>(StringComparer.Ordinal);
        var treatmentConstants = new Dictionary<string, string>(StringComparer.Ordinal);
        var allLiterals = new HashSet<string>(StringComparer.Ordinal);
        var parameters = new HashSet<string>(StringComparer.Ordinal);
        var caseArms = new HashSet<string>(StringComparer.Ordinal);
        var lookupSets = new HashSet<string>(StringComparer.Ordinal);
        var unresolved = new List<string>();
        bool foundSetParameterValues = false;
        bool foundTreatmentSwitch = false;

        // Pass one: constants, because later passes resolve identifiers through them.
        foreach (SourceFile source in sources)
        {
            string code = StripCommentsKeepingLength(source.Text);

            foreach (Match match in Regex.Matches(
                code, @"\bconst\s+string\s+(?<name>[A-Za-z_]\w*)\s*=\s*""(?<value>[^""]*)"""))
            {
                stringConstants[match.Groups["name"].Value] = match.Groups["value"].Value;
            }

            if (Regex.IsMatch(code, @"\bclass\s+TreatmentNames\b"))
            {
                foreach (Match match in Regex.Matches(
                    code, @"\bconst\s+string\s+(?<name>[A-Za-z_]\w*)\s*=\s*""(?<value>[^""]*)"""))
                {
                    treatmentConstants[match.Groups["name"].Value] = match.Groups["value"].Value;
                }
            }
        }

        // Pass two: everything else.
        foreach (SourceFile source in sources)
        {
            string code = StripCommentsKeepingLength(source.Text);

            foreach (string literal in Literals(code)) allLiterals.Add(literal);

            foreach (string body in MethodBodies(code, "SetParameterValues"))
            {
                foundSetParameterValues = true;
                foreach (string literal in Literals(body)) parameters.Add(literal);
            }

            if (Regex.IsMatch(code, @"\bswitch\s*\([^)]*TreatmentName"))
            {
                foundTreatmentSwitch = true;
            }

            foreach (Match match in Regex.Matches(code, @"\bcase\s+(?<label>[^:{}]+?)\s*:"))
            {
                string label = match.Groups["label"].Value.Trim();
                string? resolved = ResolveStringExpression(label, stringConstants);
                if (resolved is not null) caseArms.Add(resolved);
            }

            foreach ((string argument, bool isLiteral) in LookupSetArguments(code))
            {
                if (isLiteral)
                {
                    lookupSets.Add(argument);
                    continue;
                }

                string? resolved = ResolveStringExpression(argument, stringConstants);
                if (resolved is not null) lookupSets.Add(resolved);
                else if (LooksLikeASetName(argument)) unresolved.Add(argument);
            }
        }

        return new SourceFacts
        {
            StringConstants = stringConstants,
            TreatmentNameConstants = treatmentConstants,
            AllStringLiterals = allLiterals,
            ParametersWritten = parameters,
            FoundSetParameterValues = foundSetParameterValues,
            TreatmentsWithCaseArm = caseArms,
            FoundTreatmentSwitch = foundTreatmentSwitch,
            LookupSetsUsed = lookupSets,
            UnresolvedLookupSetArguments = unresolved.Distinct(StringComparer.Ordinal).OrderBy(a => a, StringComparer.Ordinal).ToList(),
        };
    }

    /// <summary>
    /// How treatment names can be recognised in this project. Two shapes exist in the corpus and
    /// they support different amounts of checking.
    /// </summary>
    public TreatmentNameStyle TreatmentStyle => this.TreatmentNameConstants.Count > 0
        ? TreatmentNameStyle.Constants
        : TreatmentNameStyle.Literals;

    /// <summary>True when the treatment name appears anywhere the code could produce it.</summary>
    public bool MentionsTreatment(string treatmentName)
        => this.TreatmentNameConstants.Values.Contains(treatmentName, StringComparer.Ordinal)
           || this.AllStringLiterals.Contains(treatmentName);

    // -----------------------------------------------------------------------------
    // Text scanning
    // -----------------------------------------------------------------------------

    /// <summary>
    /// Blanks out comments and the insides of string literals that are not being collected,
    /// keeping the text the same length so that positions still line up.
    ///
    /// <para>Comments matter here: a scaffolded project's stubs carry commented-out examples,
    /// and a check that read <c>// case TreatmentNames.Repair:</c> as a real case arm would pass
    /// a model that has none.</para>
    /// </summary>
    private static string StripCommentsKeepingLength(string text)
    {
        var result = text.ToCharArray();
        int i = 0;

        while (i < result.Length)
        {
            char c = result[i];

            if (c == '/' && i + 1 < result.Length && result[i + 1] == '/')
            {
                while (i < result.Length && result[i] != '\n') { result[i] = ' '; i++; }
                continue;
            }
            if (c == '/' && i + 1 < result.Length && result[i + 1] == '*')
            {
                while (i < result.Length && !(result[i] == '*' && i + 1 < result.Length && result[i + 1] == '/'))
                {
                    if (result[i] != '\n') result[i] = ' ';
                    i++;
                }
                if (i < result.Length) { result[i] = ' '; i++; }
                if (i < result.Length) { result[i] = ' '; i++; }
                continue;
            }
            if (c == '"')
            {
                // Skip over the literal without touching it - the literal collectors need it.
                i++;
                while (i < result.Length && result[i] != '"')
                {
                    if (result[i] == '\\') i++;
                    i++;
                }
                i++;
                continue;
            }

            i++;
        }

        return new string(result);
    }

    private static IEnumerable<string> Literals(string code)
    {
        foreach (Match match in Regex.Matches(code, @"""(?<value>(?:[^""\\\n]|\\.)*)"""))
        {
            string value = match.Groups["value"].Value;
            if (value.Length == 0) continue;
            yield return value;
        }
    }

    /// <summary>
    /// Returns the body of every method with the given name, by matching braces from the opening
    /// one. Crude, and adequate: the alternative is a parser, and a mis-matched brace here means
    /// a rule reports as skipped rather than as passed.
    /// </summary>
    private static IEnumerable<string> MethodBodies(string code, string methodName)
    {
        foreach (Match match in Regex.Matches(code, $@"\b{Regex.Escape(methodName)}\s*\("))
        {
            int open = code.IndexOf('{', match.Index);
            if (open < 0) continue;

            // A method declaration's brace follows its parameter list; a call site's does not,
            // so anything with a semicolon in between is a call and is skipped.
            string between = code[(match.Index)..open];
            if (between.Contains(';', StringComparison.Ordinal)) continue;

            int depth = 0;
            for (int i = open; i < code.Length; i++)
            {
                if (code[i] == '{') depth++;
                else if (code[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        yield return code[open..(i + 1)];
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Finds the arguments that name a lookup set, across the shapes the corpus actually uses:
    /// the framework's own two helpers, direct indexing of <c>Lookups</c>, and the
    /// <c>Constants</c> pattern's private helpers.
    /// </summary>
    private static IEnumerable<(string Argument, bool IsLiteral)> LookupSetArguments(string code)
    {
        // model.GetLookupValueNumber("set", "key") / GetLookupValueText("set", "key")
        foreach (Match match in Regex.Matches(code, @"\bGetLookupValue(?:Number|Text)\s*\(\s*(?<arg>[^,)]+)"))
        {
            yield return Classify(match.Groups["arg"].Value);
        }

        // Lookups["set"]
        foreach (Match match in Regex.Matches(code, @"\bLookups\s*\[\s*(?<arg>[^\]]+)\]"))
        {
            yield return Classify(match.Groups["arg"].Value);
        }

        // GetSet(lookupSets, SetName) / GetNumber(lookupSets, SetName, "key") / GetText(...)
        foreach (Match match in Regex.Matches(
            code, @"\bGet(?:Set|Number|Text)\s*\(\s*[A-Za-z_]\w*\s*,\s*(?<arg>[^,)]+)"))
        {
            yield return Classify(match.Groups["arg"].Value);
        }
    }

    /// <summary>
    /// Whether an unresolved argument is worth reporting as "could not be worked out".
    ///
    /// <para>The <c>Constants</c> pattern's own helpers pass their <c>setName</c> parameter
    /// straight through to <c>GetSet</c>, so every project that follows it has a call site whose
    /// argument is a parameter rather than a set name. Reporting those buries the one case the
    /// note exists for - a genuine set name the tool could not follow - in plumbing, and a note
    /// that always fires is a note nobody reads.</para>
    ///
    /// <para>The test is the C# naming convention that every model in the corpus follows:
    /// constants are PascalCase, parameters and locals are camelCase. It is a heuristic, and the
    /// cost of it being wrong is one genuinely-unfollowable set name going unmentioned in a check
    /// that already says it reads C# as text.</para>
    /// </summary>
    private static bool LooksLikeASetName(string argument)
    {
        string identifier = argument.Contains('.', StringComparison.Ordinal)
            ? argument[(argument.LastIndexOf('.') + 1)..]
            : argument;

        return identifier.Length > 0 && char.IsUpper(identifier[0]);
    }

    private static (string Argument, bool IsLiteral) Classify(string raw)
    {
        string trimmed = raw.Trim();
        if (trimmed.StartsWith('"') && trimmed.EndsWith('"') && trimmed.Length >= 2)
        {
            return (trimmed[1..^1], true);
        }
        return (trimmed, false);
    }

    /// <summary>
    /// Turns an expression that ought to be a constant string into its value: a literal, a bare
    /// identifier, or a qualified one such as <c>TreatmentNames.Repair</c>. Returns null when it
    /// is something else, which the caller reports rather than guesses at.
    /// </summary>
    private static string? ResolveStringExpression(string expression, IReadOnlyDictionary<string, string> constants)
    {
        string trimmed = expression.Trim();
        if (trimmed.Length == 0) return null;

        if (trimmed.StartsWith('"') && trimmed.EndsWith('"') && trimmed.Length >= 2)
        {
            return trimmed[1..^1];
        }

        string identifier = trimmed.Contains('.', StringComparison.Ordinal)
            ? trimmed[(trimmed.LastIndexOf('.') + 1)..]
            : trimmed;

        return constants.TryGetValue(identifier, out string? value) ? value : null;
    }
}

/// <summary>How a project spells its treatment names, and therefore how much can be checked.</summary>
internal enum TreatmentNameStyle
{
    /// <summary>A <c>TreatmentNames</c> class of constants. Both directions can be checked.</summary>
    Constants,

    /// <summary>Bare string literals. Only bundle-to-code can be checked; the reverse is unknowable.</summary>
    Literals,
}
