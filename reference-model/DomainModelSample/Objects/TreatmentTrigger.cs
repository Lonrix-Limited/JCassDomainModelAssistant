using System;
using JCass_ModelCore.Treatments;

namespace DomainModelSample.Objects;

/// <summary>
/// Decides whether an element is due for work, and builds the <see cref="TreatmentInstance"/>
/// that says so. This is the heart of a domain model: everything else moves state around, this
/// is where the engineering judgement lives.
///
/// <para>Every threshold and rate used here arrives via <see cref="Constants"/>, which read them
/// from the client's <c>inputs\lookups.xlsx</c>. Nothing in this file is a magic number a modeller
/// would have to ask a developer to change — that is deliberate, and it is the pattern to keep
/// when you extend this. See <see cref="Constants"/> for why.</para>
/// </summary>
public static class TreatmentTrigger
{
    /// <summary>
    /// Condition above which routine maintenance is needed every period.
    ///
    /// <para><b>DELIBERATE COUNTER-EXAMPLE — do not copy this shape.</b> It is a trigger
    /// threshold, so by the rule it belongs in <c>inputs\lookups.xlsx</c> and should be read
    /// through <see cref="Constants"/> like every other threshold in this file. It is left as a
    /// <c>const</c> on purpose, as the contrast that makes the rule visible: as written, moving it
    /// from 50 to 45 needs a developer, a rebuild and a republish, where every threshold on
    /// <see cref="Constants"/> is something the modeller changes themselves on the Tuning page.
    /// Moving it is the reader's first exercise — README section 7.</para>
    /// </summary>
    public const double RoutineMaintenanceConditionGreaterThan = 50;

    /// <summary>
    /// Returns the single most appropriate treatment for this element in this period, or
    /// <c>null</c> if nothing is triggered. Repair is tested before replace, so an element in the
    /// repairable band never generates a replace candidate.
    /// </summary>
    /// <param name="element">The element under test.</param>
    /// <param name="constants">Thresholds and rates read from lookups.xlsx.</param>
    /// <param name="iPeriod">Modelling period (1-based).</param>
    public static TreatmentInstance? GetTriggeredTreatment(SampleElement element, Constants constants, int iPeriod)
    {
        if (element.Age > constants.RepairAgeGreaterThan
            && element.ConditionRating > constants.RepairConditionGreaterThan
            && element.ConditionRating <= constants.RepairConditionAtMost)
        {
            return Build(
                element,
                TreatmentNames.Repair,
                element.GetRepairRate(),
                constants,
                iPeriod,
                $"Age > {constants.RepairAgeGreaterThan} and condition in " +
                $"({constants.RepairConditionGreaterThan}, {constants.RepairConditionAtMost}]");
        }

        if (element.Age > constants.ReplaceAgeGreaterThan
            && element.ConditionRating > constants.ReplaceConditionGreaterThan)
        {
            return Build(
                element,
                TreatmentNames.Replace,
                element.GetReplacementRate(),
                constants,
                iPeriod,
                $"Age > {constants.ReplaceAgeGreaterThan} and condition > {constants.ReplaceConditionGreaterThan}");
        }

        return null;
    }

    /// <summary>
    /// Returns routine maintenance for this element in this period, or <c>null</c> if none is due.
    /// Routine maintenance is applied outside the optimiser — it is not a candidate that competes
    /// for capital budget.
    /// </summary>
    /// <param name="element">The element under test.</param>
    /// <param name="constants">Thresholds and rates read from lookups.xlsx.</param>
    /// <param name="iPeriod">Modelling period (1-based).</param>
    public static TreatmentInstance? GetTriggeredMaintenance(SampleElement element, Constants constants, int iPeriod)
    {
        if (element.ConditionRating <= RoutineMaintenanceConditionGreaterThan) return null;

        // Maintenance effort scales with how bad the element is, not with its size, so the
        // quantity here is a condition-derived measure rather than the element's area.
        double quantity = Math.Sqrt(element.ConditionRating);

        return Build(
            element,
            TreatmentNames.RoutineMaintenance,
            baseRate: 1.0,
            constants: constants,
            iPeriod: iPeriod,
            reason: $"Condition > {RoutineMaintenanceConditionGreaterThan}",
            quantity: quantity);
    }

    /// <summary>
    /// Assembles a <see cref="TreatmentInstance"/> and gives it the element's objective value as
    /// its suitability score. The optimiser ranks candidates on that score, so a treatment with no
    /// score set will never be chosen ahead of one that has one.
    ///
    /// <para>Cost is <c>Quantity × UnitRate</c>. The unit rate is the element's material-derived
    /// base rate multiplied by the rate for this treatment from <c>lookups.xlsx</c>, so a modeller
    /// can escalate all repair costs by 10% from the Tuning page without touching code.</para>
    /// </summary>
    /// <param name="element">Element the treatment applies to.</param>
    /// <param name="treatmentName">One of the <see cref="TreatmentNames"/> constants.</param>
    /// <param name="baseRate">Cost per unit before the lookup rate, derived from the element's material.</param>
    /// <param name="constants">Thresholds and rates read from lookups.xlsx.</param>
    /// <param name="iPeriod">Modelling period (1-based).</param>
    /// <param name="reason">Human-readable trigger reason; ends up in the outputs.</param>
    /// <param name="quantity">Quantity treated. Defaults to the element's area in square metres.</param>
    private static TreatmentInstance Build(
        SampleElement element,
        string treatmentName,
        double baseRate,
        Constants constants,
        int iPeriod,
        string reason,
        double? quantity = null)
    {
        TreatmentInstance treatment = new TreatmentInstance(
            element.ElementIndex,
            treatmentName,
            iPeriod,
            quantity: quantity ?? element.AreaSquareMetre,
            unitRate: baseRate * constants.GetUnitRate(treatmentName),
            force: false,
            reason: reason,
            comment: "none");

        treatment.TreatmentSuitabilityScore = element.ObjectiveValue;
        return treatment;
    }
}
