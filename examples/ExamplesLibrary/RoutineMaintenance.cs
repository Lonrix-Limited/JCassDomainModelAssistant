using ExamplesLibrary.Shared;
using JCass_ModelCore.Models;
using JCass_ModelCore.Treatments;

namespace ExamplesLibrary;

/// <summary>
/// PATTERN: routine-maintenance. Work that happens whether or not there is capital budget for it.
///
/// <para>Documentation: <c>docs\patterns\routine-maintenance.md</c>. API:
/// <c>docs\framework\api\authoring\TreatmentInstance.md</c>, property <c>RankParamSimple</c>.
/// Where the thresholds come from: <c>docs\patterns\constants-from-lookups.md</c>.</para>
///
/// <para><b>Where it sits in the run.</b> Maintenance is returned from
/// <c>GetTriggeredMaintenance</c>, which the framework calls <i>after</i> the optimiser has
/// chosen and funded capital treatments. It does not compete with them. It is charged against its
/// own budget category, sorted by <c>RankParamSimple</c>, and funded down the list.</para>
///
/// <para><b>Why modelling it matters more than it looks.</b> Routine maintenance is usually the
/// largest single consequence of deferring renewal - defer the replacement and the flushing,
/// jetting and emergency repairs go up every year afterwards. A model that leaves it out makes
/// doing nothing look free, which is the exact conclusion an asset-management model exists to
/// disprove.</para>
///
/// <para><b>Return <c>null</c> when none is due</b>, which for most elements in most periods is
/// the answer.</para>
/// </summary>
public static class RoutineMaintenance
{
    /// <summary>
    /// Returns the routine maintenance due on this segment in this period, or <c>null</c>.
    ///
    /// <para><b>The quantity is not automatically the element's size.</b> Flushing effort scales
    /// with how bad the segment is as much as with how long it is, so the quantity here is a
    /// condition-weighted length rather than the raw length. Whatever you choose, it must be in
    /// the unit the rate in <c>lookups.xlsx</c> is priced in - the two are multiplied and nothing
    /// anywhere checks that they agree.</para>
    ///
    /// <para><b>Set the priority.</b> <c>RankParamSimple</c> is the only control over what gets
    /// done first when the maintenance budget is short; left at zero the order is whatever the
    /// element loop happened to produce. See <c>TreatmentSuitabilityScoring</c>.</para>
    ///
    /// <para><b>Its budget category still has to exist.</b> Maintenance is charged like any other
    /// treatment, so its <c>budget_category</c> in the bundle needs a matching column in the
    /// client's <c>inputs\budgets.xlsx</c>. That one the framework does check at setup, and it
    /// names the treatment - so this failure is loud, unlike the multi-category case.</para>
    /// </summary>
    /// <param name="segment">The segment under test.</param>
    /// <param name="constants">Thresholds and rates read from <c>lookups.xlsx</c>.</param>
    /// <param name="period">Modelling period (1-based).</param>
    /// <returns>The maintenance treatment, or <c>null</c> if none is due.</returns>
    public static TreatmentInstance? GetTriggeredMaintenance(
        PipeSegment segment,
        PipeConstants constants,
        int period)
    {
        if (segment.ConditionGrade <= constants.FlushConditionGreaterThan) return null;

        // Metres of pipe flushed, weighted by how far past the threshold the segment is. The
        // weighting is a ratio of two values that both come from lookups, so it stays tunable
        // without any literal appearing here.
        double severity = segment.ConditionGrade / constants.FlushConditionGreaterThan;
        double quantity = segment.LengthMetres * severity;

        TreatmentInstance maintenance = new TreatmentInstance(
            segment.ElementIndex,
            TreatmentNames.Flush,
            period,
            quantity: quantity,
            unitRate: constants.GetUnitRate(TreatmentNames.Flush),
            force: false,
            reason: $"Condition {Math.Round(segment.ConditionGrade, 1)} > " +
                    $"{constants.FlushConditionGreaterThan}",
            comment: $"Severity factor {Math.Round(severity, 2)}");

        TreatmentSuitabilityScoring.SetMaintenancePriority(maintenance, segment, constants);

        return maintenance;
    }

    /// <summary>
    /// The other half of modelling maintenance: what a capital treatment does to it.
    ///
    /// <para>A segment that has just been relined or replaced should not carry its old
    /// maintenance history into the next period - it is, for maintenance purposes, a new pipe.
    /// Reset the state in the resetter, not in the maintenance trigger, so that the trigger stays
    /// a pure function of the segment's current condition.</para>
    ///
    /// <para><b>This is easy to forget and it does not error.</b> Miss it and treated segments go
    /// on generating maintenance at their pre-treatment rate for the rest of the run, which makes
    /// renewal look less worthwhile than it is - a bias in exactly the direction that is hardest
    /// to notice, because it makes the model conservative rather than absurd.</para>
    /// </summary>
    /// <param name="segment">The segment that has just been treated.</param>
    /// <param name="constants">Thresholds read from <c>lookups.xlsx</c>.</param>
    /// <param name="treatment">The treatment that was applied.</param>
    public static void ResetMaintenanceState(
        PipeSegment segment,
        PipeConstants constants,
        TreatmentInstance treatment)
    {
        switch (treatment.TreatmentName)
        {
            case TreatmentNames.Replace:
                segment.BreakRatePerKmYear = 0;
                segment.ConditionGrade = constants.ConditionAfterReplace;
                break;

            case TreatmentNames.Reline:
            case TreatmentNames.RelineWithRepairs:
                // A liner suppresses breaks without renewing the host pipe, so condition improves
                // to the relined value rather than to as-new.
                segment.BreakRatePerKmYear = 0;
                segment.ConditionGrade = constants.ConditionAfterReline;
                break;

            case TreatmentNames.PatchRepair:
                // A patch improves; it does not renew. Break rate is reduced, not cleared.
                segment.ConditionGrade *= constants.ConditionFactorAfterRepair;
                segment.BreakRatePerKmYear *= constants.ConditionFactorAfterRepair;
                break;

            case TreatmentNames.Flush:
                // Maintenance, not a capital treatment. It changes nothing structural.
                break;

            default:
                // An arm per treatment, and a default that throws rather than doing nothing.
                // A treatment added to the trigger but not here would otherwise be funded,
                // applied, and have no effect at all - the element deteriorates on as if the
                // money had never been spent, and nothing reports it.
                throw new Exception(
                    $"No reset defined for treatment '{treatment.TreatmentName}'. " +
                    "Every treatment the trigger can return needs an arm here.");
        }
    }

    /// <summary>
    /// Reading the project's configured maintenance treatment name, rather than assuming it.
    ///
    /// <para>The framework knows which treatment is "the" routine maintenance one - it is named
    /// in the model's meta setup and exposed as
    /// <c>model.Configuration.RoutineMaintenanceTreatmentName</c>. Comparing against that instead
    /// of against your own constant is how post-processing and the framework's own reporting stay
    /// in agreement with the model about which spending was maintenance.</para>
    /// </summary>
    /// <param name="treatment">Any treatment.</param>
    /// <param name="frameworkModel">The framework model, for its configuration.</param>
    public static bool IsRoutineMaintenance(TreatmentInstance treatment, ModelBase frameworkModel)
        => treatment.IsRoutineMaintenance(frameworkModel.Configuration);
}
