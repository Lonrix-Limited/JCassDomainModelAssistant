using ExamplesLibrary.Shared;

namespace ExamplesLibrary;

/// <summary>
/// PATTERN: constants-from-lookups. Every tunable number the model uses, read once from the
/// client's <c>inputs\lookups.xlsx</c> at setup and held for the run.
///
/// <para>Documentation: <c>docs\patterns\constants-from-lookups.md</c>.</para>
///
/// <para><b>Why the class exists at all.</b> A number hard-coded in C# is a number the modeller
/// cannot change without a developer, a rebuild and a republish. The same number in
/// <c>lookups.xlsx</c> is one they change themselves on the web app's Tuning page and re-run in
/// minutes. This class is the seam that makes the second thing possible, and it is the reason a
/// model is calibratable rather than only reportable.</para>
///
/// <para><b>Two shapes, and the choice between them is the interesting part.</b> A threshold used
/// in one specific comparison is unpacked into its own property, so a missing row fails at setup
/// and the call site reads as English. A <i>set</i> of values all used the same way - a rate per
/// material, a rate per treatment - is kept whole and resolved by key at the point of use, so
/// adding a material costs a spreadsheet row and nothing in C# at all.</para>
///
/// <para><b>Guard before you index.</b> Every read below goes through a helper that checks first
/// and throws naming the set and the key. That is not defensive habit; it is what turns a typo in
/// a spreadsheet into a message a modeller can act on instead of a bare
/// <see cref="KeyNotFoundException"/> five minutes into a run.</para>
/// </summary>
public class PipeConstants
{
    // Set names, as they appear in the lookup_set_name column of any lkp_ sheet. Named constants
    // rather than inline strings because each one is used in at least two places - the read, and
    // the error message when the read fails.
    private const string ReplaceThresholds = "replace_thresholds";
    private const string RelineThresholds = "reline_thresholds";
    private const string RepairThresholds = "repair_thresholds";
    private const string FlushThresholds = "flush_thresholds";
    private const string DeteriorationRates = "deterioration_rates";
    private const string BreakRateGrowth = "break_rate_growth";
    private const string UnitRates = "unit_rates";
    private const string CostFactors = "cost_factors";
    private const string ScoringWeights = "scoring_weights";

    // Sets kept whole and resolved at the point of use.
    private readonly Dictionary<string, object> _unitRates;
    private readonly Dictionary<string, object> _deteriorationRates;
    private readonly Dictionary<string, object> _breakRateGrowth;

    /// <summary>A segment worse than this condition grade may be considered for replacement.</summary>
    public double ReplaceConditionGreaterThan { get; }

    /// <summary>Breaks per km per year above which replacement is justified on its own.</summary>
    public double ReplaceBreakRateGreaterThan { get; }

    /// <summary>Condition grade a segment is left in after replacement.</summary>
    public double ConditionAfterReplace { get; }

    /// <summary>A segment worse than this condition grade may be considered for relining.</summary>
    public double RelineConditionGreaterThan { get; }

    /// <summary>Above this grade a liner no longer helps - replacement territory.</summary>
    public double RelineConditionAtMost { get; }

    /// <summary>Condition grade a segment is left in after relining.</summary>
    public double ConditionAfterReline { get; }

    /// <summary>A segment worse than this condition grade may be considered for a patch repair.</summary>
    public double RepairConditionGreaterThan { get; }

    /// <summary>Condition is multiplied by this after a patch repair. A repair improves; it does not renew.</summary>
    public double ConditionFactorAfterRepair { get; }

    /// <summary>Condition grade above which flushing is required every period.</summary>
    public double FlushConditionGreaterThan { get; }

    /// <summary>Fraction of a segment's length that a structural repair typically covers.</summary>
    public double RepairExtentFraction { get; }

    /// <summary>Weight given to criticality when scoring how suitable a treatment is.</summary>
    public double CriticalityWeight { get; }

    /// <summary>Weight given to condition when scoring how suitable a treatment is.</summary>
    public double ConditionWeight { get; }

    /// <summary>
    /// Reads every threshold from the model's lookups and holds on to the sets that are resolved
    /// later. Throws, naming the missing set or key, if the spreadsheet is incomplete.
    /// </summary>
    /// <param name="lookupSets">
    /// The model's lookups, keyed by set name then setting key. Pass <c>this.model.Lookups</c>
    /// from <c>SetupInstance</c> - see <c>docs\patterns\constants-from-lookups.md</c> for why
    /// that is the earliest place this may be built.
    /// </param>
    public PipeConstants(Dictionary<string, Dictionary<string, object>> lookupSets)
    {
        this.ReplaceConditionGreaterThan = GetNumber(lookupSets, ReplaceThresholds, "cond_gt");
        this.ReplaceBreakRateGreaterThan = GetNumber(lookupSets, ReplaceThresholds, "break_rate_gt");
        this.ConditionAfterReplace = GetNumber(lookupSets, ReplaceThresholds, "cond_after");

        this.RelineConditionGreaterThan = GetNumber(lookupSets, RelineThresholds, "cond_gt");
        this.RelineConditionAtMost = GetNumber(lookupSets, RelineThresholds, "cond_lte");
        this.ConditionAfterReline = GetNumber(lookupSets, RelineThresholds, "cond_after");

        this.RepairConditionGreaterThan = GetNumber(lookupSets, RepairThresholds, "cond_gt");
        this.ConditionFactorAfterRepair = GetNumber(lookupSets, RepairThresholds, "cond_factor");

        this.FlushConditionGreaterThan = GetNumber(lookupSets, FlushThresholds, "cond_gt");

        this.RepairExtentFraction = GetNumber(lookupSets, CostFactors, "repair_extent_fraction");

        this.CriticalityWeight = GetNumber(lookupSets, ScoringWeights, "criticality_weight");
        this.ConditionWeight = GetNumber(lookupSets, ScoringWeights, "condition_weight");

        _unitRates = GetSet(lookupSets, UnitRates);
        _deteriorationRates = GetSet(lookupSets, DeteriorationRates);
        _breakRateGrowth = GetSet(lookupSets, BreakRateGrowth);
    }

    /// <summary>
    /// Condition grade points added per year for a material. Higher means faster decay.
    /// </summary>
    /// <param name="materialType">Value of the segment's <c>material</c> input column.</param>
    public double GetDeteriorationRate(string materialType)
        => Resolve(_deteriorationRates, materialType, DeteriorationRates);

    /// <summary>
    /// Annual multiplicative growth in break rate for a material.
    /// </summary>
    /// <param name="materialType">Value of the segment's <c>material</c> input column.</param>
    public double GetBreakRateGrowth(string materialType)
        => Resolve(_breakRateGrowth, materialType, BreakRateGrowth);

    /// <summary>
    /// Cost per metre for a treatment, keyed by the treatment's name.
    ///
    /// <para>Resolved on demand rather than unpacked in the constructor, so a new treatment needs
    /// a row in <c>lookups.xlsx</c> and a constant on <see cref="TreatmentNames"/> - and nothing
    /// in this class. This is the set the Tuning page's Treatment Rates tab edits, so expect its
    /// values to change between runs with no code change at all.</para>
    /// </summary>
    /// <param name="treatmentName">One of the <see cref="TreatmentNames"/> constants.</param>
    public double GetUnitRate(string treatmentName) => Resolve(_unitRates, treatmentName, UnitRates);

    /// <summary>
    /// Reads one value out of a set that was kept rather than unpacked.
    ///
    /// <para><b>This is the guard idiom, and it is a rule rather than a nicety.</b> Check
    /// membership, then throw naming both the set and the key. Indexing straight into the
    /// dictionary gives a <see cref="KeyNotFoundException"/> that names nothing, and returning a
    /// default instead would be worse still - a run that completes with a silently wrong
    /// number.</para>
    /// </summary>
    /// <param name="set">The lookup set, already fetched.</param>
    /// <param name="key">Value of the <c>setting_key</c> column to read.</param>
    /// <param name="setName">Name of the set, for the failure message.</param>
    private static double Resolve(Dictionary<string, object> set, string key, string setName)
    {
        if (!set.ContainsKey(key))
        {
            throw new Exception($"'{key}' has no value in lookup set '{setName}' in lookups.xlsx.");
        }

        // Convert, never cast. setting_value arrives as TEXT whatever the cell looks like in
        // Excel, so (double)set[key] throws an InvalidCastException that says nothing useful
        // about a spreadsheet.
        return Convert.ToDouble(set[key]);
    }

    /// <summary>
    /// Fetches a whole lookup set, failing with the set name if it is absent.
    /// </summary>
    /// <param name="lookupSets">The model's lookups.</param>
    /// <param name="setName">Value of the <c>lookup_set_name</c> column to fetch.</param>
    private static Dictionary<string, object> GetSet(
        Dictionary<string, Dictionary<string, object>> lookupSets,
        string setName)
    {
        if (!lookupSets.ContainsKey(setName))
        {
            throw new Exception($"Lookup set '{setName}' not found in lookups.xlsx.");
        }

        return lookupSets[setName];
    }

    /// <summary>
    /// Reads one numeric setting, failing with the set and key names if it is absent.
    /// </summary>
    /// <param name="lookupSets">The model's lookups.</param>
    /// <param name="setName">Value of the <c>lookup_set_name</c> column.</param>
    /// <param name="settingKey">Value of the <c>setting_key</c> column.</param>
    private static double GetNumber(
        Dictionary<string, Dictionary<string, object>> lookupSets,
        string setName,
        string settingKey)
        => Resolve(GetSet(lookupSets, setName), settingKey, setName);
}
