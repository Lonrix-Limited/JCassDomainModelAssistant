using System;
using System.Collections.Generic;

namespace DomainModelSample.Objects;

/// <summary>
/// Every tunable number this model uses, read once from the client's <c>inputs\lookups.xlsx</c>
/// at setup and cached here for the run.
///
/// <para><b>This class is the most important thing in the kit to copy.</b> Thresholds and rates
/// live in a spreadsheet, not in C#, so a modeller recalibrates the model by editing
/// <c>lookups.xlsx</c> — or by using the web app's <b>Tuning</b> page, which writes back to that
/// same file. No rebuild, no redeploy, no developer. Hard-code a threshold in C# and you have
/// taken that away from them.</para>
///
/// <para><b>How lookups.xlsx is structured.</b> Any sheet whose name starts with <c>lkp_</c> is
/// read, and all of them are merged into one flat table. Each sheet has three columns that matter
/// — <c>lookup_set_name</c>, <c>setting_key</c>, <c>setting_value</c> — so a value is addressed by
/// the pair (set name, key). The sheet a row lives in is only an organisational convenience; it is
/// not part of the address, so sheets can be reorganised without touching code.</para>
///
/// <para><b>Read them here, not earlier.</b> This object is built from
/// <see cref="DomainModelSample.SetupInstance"/>, which the framework calls after it has loaded
/// lookups and before it touches any element. That ordering is the whole reason
/// <c>SetupInstance</c> exists. Reading a lookup from a constructor or static initialiser that
/// runs earlier gets you an empty dictionary — and it reads as "key not found" rather than "too
/// early", which is a confusing hour to lose.</para>
/// </summary>
public class Constants
{
    /// <summary>Set name in lookups.xlsx holding the repair trigger thresholds.</summary>
    private const string RepairThresholds = "repair_thresholds";

    /// <summary>Set name in lookups.xlsx holding the replace trigger thresholds.</summary>
    private const string ReplaceThresholds = "replace_thresholds";

    /// <summary>
    /// Set name in lookups.xlsx holding the per-treatment unit rates. This is the set the web
    /// app's Tuning page "Treatment Rates" tab edits, so expect its values to change between runs
    /// with no code change.
    /// </summary>
    private const string UnitRates = "unit_rates";

    /// <summary>
    /// The whole unit-rate set, kept rather than unpacked into fixed properties. Rates are looked
    /// up by treatment name at the point of use, so adding a treatment needs a row in
    /// lookups.xlsx and nothing in this class.
    /// </summary>
    private readonly Dictionary<string, object> _unitRates;

    /// <summary>An element older than this many years may be considered for repair.</summary>
    public double RepairAgeGreaterThan { get; }

    /// <summary>Condition must be worse than this for a repair to be worth doing.</summary>
    public double RepairConditionGreaterThan { get; }

    /// <summary>Above this condition a repair no longer helps — replacement territory.</summary>
    public double RepairConditionAtMost { get; }

    /// <summary>An element older than this many years may be considered for replacement.</summary>
    public double ReplaceAgeGreaterThan { get; }

    /// <summary>Condition must be worse than this for a replacement to be triggered.</summary>
    public double ReplaceConditionGreaterThan { get; }

    /// <summary>
    /// Reads every threshold from the model's lookups and holds on to the unit-rate set. Throws
    /// with a message naming the missing set or key if the spreadsheet is incomplete — which is
    /// what you want: a typo in <c>lookups.xlsx</c> fails immediately at setup rather than
    /// silently defaulting and skewing a whole run.
    /// </summary>
    /// <param name="lookupSets">
    /// The model's lookups, keyed by set name then setting key. Pass <c>this.model.Lookups</c>
    /// from <see cref="DomainModelSample.SetupInstance"/>.
    /// </param>
    public Constants(Dictionary<string, Dictionary<string, object>> lookupSets)
    {
        this.RepairAgeGreaterThan = GetNumber(lookupSets, RepairThresholds, "age_gt");
        this.RepairConditionGreaterThan = GetNumber(lookupSets, RepairThresholds, "cond_gt");
        this.RepairConditionAtMost = GetNumber(lookupSets, RepairThresholds, "cond_lte");

        this.ReplaceAgeGreaterThan = GetNumber(lookupSets, ReplaceThresholds, "age_gt");
        this.ReplaceConditionGreaterThan = GetNumber(lookupSets, ReplaceThresholds, "cond_gt");

        _unitRates = GetSet(lookupSets, UnitRates);
    }

    /// <summary>
    /// Returns the unit rate for a treatment, keyed by the treatment's name.
    ///
    /// <para>Looked up on demand rather than unpacked in the constructor, so a new treatment needs
    /// a row in <c>lookups.xlsx</c> and a constant on <see cref="TreatmentNames"/> — and nothing
    /// at all in this class.</para>
    /// </summary>
    /// <param name="treatmentName">One of the <see cref="TreatmentNames"/> constants.</param>
    /// <exception cref="Exception">The treatment has no rate in the <c>unit_rates</c> set.</exception>
    public double GetUnitRate(string treatmentName)
    {
        if (!_unitRates.ContainsKey(treatmentName))
        {
            throw new Exception(
                $"Unit rate for Treatment '{treatmentName}' not found in lookup set '{UnitRates}'.");
        }

        return Convert.ToDouble(_unitRates[treatmentName]);
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
    ///
    /// <para>The conversion is not incidental: <c>setting_value</c> arrives as text regardless of
    /// how the cell looks in Excel, so a raw cast would throw an unhelpful
    /// <see cref="InvalidCastException"/> here.</para>
    /// </summary>
    /// <param name="lookupSets">The model's lookups.</param>
    /// <param name="setName">Value of the <c>lookup_set_name</c> column.</param>
    /// <param name="settingKey">Value of the <c>setting_key</c> column.</param>
    private static double GetNumber(
        Dictionary<string, Dictionary<string, object>> lookupSets,
        string setName,
        string settingKey)
    {
        Dictionary<string, object> set = GetSet(lookupSets, setName);

        if (!set.ContainsKey(settingKey))
        {
            throw new Exception($"Setting '{settingKey}' not found in lookup set '{setName}'.");
        }

        return Convert.ToDouble(set[settingKey]);
    }
}
