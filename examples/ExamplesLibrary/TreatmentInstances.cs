using ExamplesLibrary.Shared;
using JCass_ModelCore.Models;
using JCass_ModelCore.Treatments;

namespace ExamplesLibrary;

/// <summary>
/// PATTERN: treatment-instances. Constructing a <see cref="TreatmentInstance"/> correctly - the
/// single most error-prone thing a domain model does.
///
/// <para>Documentation: <c>docs\patterns\treatment-instances.md</c>. API:
/// <c>docs\framework\api\authoring\TreatmentInstance.md</c>. Where the rates come from:
/// <c>docs\patterns\constants-from-lookups.md</c>.</para>
///
/// <para><b>There is exactly one constructor and it takes eight parameters.</b> Two of them are
/// consecutive <c>double</c>s and three are consecutive <c>string</c>s, so a call with the right
/// values in the wrong order compiles cleanly and produces a wrong model. Swap quantity and unit
/// rate and the cost is wrong by orders of magnitude; swap reason and comment and the export
/// reads as nonsense to whoever asks why an element was treated.</para>
///
/// <para><b>Use named arguments. Every time.</b> They cost nothing and they turn a whole class of
/// silent modelling error into a compile error. Every example in this library does it.</para>
///
/// <para><b>Three things the constructor does not do</b>, each of which has caught somebody:
/// it does not set the suitability score, it does not set the maintenance rank, and it does not
/// calculate the cost. <c>Cost</c> is zero until the framework multiplies quantity by unit rate
/// and applies the present-worth factor, so reading it straight after construction returns 0 and
/// not the treatment's cost.</para>
/// </summary>
public static class TreatmentInstances
{
    /// <summary>
    /// The canonical construction. Read the parameter list against the eight names below and the
    /// call site tells you what it does without a trip to the reference.
    /// </summary>
    /// <param name="segment">The segment the treatment applies to.</param>
    /// <param name="constants">Thresholds and rates read from <c>lookups.xlsx</c>.</param>
    /// <param name="period">Modelling period (1-based). Zero or negative throws.</param>
    public static TreatmentInstance BuildReplace(PipeSegment segment, PipeConstants constants, int period)
    {
        return new TreatmentInstance(
            segment.ElementIndex,                                // 1. element_index
            TreatmentNames.Replace,                              // 2. name - must match a bundle treatment
            period,                                              // 3. period - 1-based
            quantity: segment.LengthMetres,                      // 4. in the unit the rate is priced in
            unitRate: constants.GetUnitRate(TreatmentNames.Replace),  // 5. from lookups, never a literal
            force: false,                                        // 6. bypass the ranking, not the budget
            reason: $"Condition {Math.Round(segment.ConditionGrade, 1)} > " +
                    $"{constants.ReplaceConditionGreaterThan}",  // 7. exported; a modeller reads it
            comment: $"Break rate {Math.Round(segment.BreakRatePerKmYear, 2)}/km/yr");  // 8. free text
    }

    /// <summary>
    /// Quantity and unit rate must agree on units, and nothing checks that they do.
    ///
    /// <para>A patch repair is priced per metre of pipe repaired, not per metre of segment, so
    /// the quantity is the segment's length scaled by the fraction a repair typically covers -
    /// and that fraction comes from <c>lookups.xlsx</c> like every other tunable number. Pass the
    /// whole segment length here instead and the cost is right by construction and wrong by
    /// engineering, which is the harder kind of wrong to notice.</para>
    /// </summary>
    /// <param name="segment">The segment the treatment applies to.</param>
    /// <param name="constants">Thresholds and rates read from <c>lookups.xlsx</c>.</param>
    /// <param name="period">Modelling period (1-based).</param>
    public static TreatmentInstance BuildPatchRepair(PipeSegment segment, PipeConstants constants, int period)
    {
        double repairedLengthMetres = segment.LengthMetres * constants.RepairExtentFraction;

        return new TreatmentInstance(
            segment.ElementIndex,
            TreatmentNames.PatchRepair,
            period,
            quantity: repairedLengthMetres,
            unitRate: constants.GetUnitRate(TreatmentNames.PatchRepair),
            force: false,
            reason: $"Condition {Math.Round(segment.ConditionGrade, 1)} > " +
                    $"{constants.RepairConditionGreaterThan}",
            comment: $"Repairing {Math.Round(repairedLengthMetres, 1)}m of {Math.Round(segment.LengthMetres, 1)}m");
    }

    /// <summary>
    /// A forced treatment - one that policy or safety requires regardless of how it ranks.
    ///
    /// <para><b><c>force: true</c> bypasses the ranking, not the budget.</b> In an MCDA model a
    /// forced treatment gets the maximum rank parameter; in a benefit-cost model forced
    /// strategies are separated out and funded ahead of the ranked ones. What it never does is
    /// create money. Use it for interventions that are not optional, not to push through a
    /// treatment the model would otherwise reject - the second is how a model stops being
    /// evidence and starts being an argument.</para>
    /// </summary>
    /// <param name="segment">The segment the treatment applies to.</param>
    /// <param name="constants">Thresholds and rates read from <c>lookups.xlsx</c>.</param>
    /// <param name="period">Modelling period (1-based).</param>
    /// <param name="mandateReason">Why this is not optional. Goes straight to the export.</param>
    public static TreatmentInstance BuildForcedReplace(
        PipeSegment segment,
        PipeConstants constants,
        int period,
        string mandateReason)
    {
        return new TreatmentInstance(
            segment.ElementIndex,
            TreatmentNames.Replace,
            period,
            quantity: segment.LengthMetres,
            unitRate: constants.GetUnitRate(TreatmentNames.Replace),
            force: true,
            reason: mandateReason,
            comment: "Forced - not subject to ranking");
    }

    /// <summary>
    /// A follow-up treatment scheduled beyond this period, and the check that has to go with it.
    ///
    /// <para><b>A treatment placed beyond the last modelled period is discarded in silence.</b>
    /// The framework's append is wrapped in <c>if (treatment.TreatmentPeriod &lt;= model.NPeriods)</c>
    /// with no <c>else</c>: not recorded, not costed, not warned about. A model that schedules a
    /// ten-year follow-up in a ten-period run loses it and reports nothing, so the forecast
    /// quietly assumes work that was never funded.</para>
    ///
    /// <para>So compare against <c>model.NPeriods</c> yourself and decide what should happen.
    /// Returning <c>null</c> - "there is no follow-up within the horizon" - is usually right;
    /// what is never right is scheduling it and assuming it happened.</para>
    /// </summary>
    /// <param name="segment">The segment the treatment applies to.</param>
    /// <param name="constants">Thresholds and rates read from <c>lookups.xlsx</c>.</param>
    /// <param name="frameworkModel">The framework model, for <c>NPeriods</c>.</param>
    /// <param name="currentPeriod">Period the first treatment falls in (1-based).</param>
    /// <param name="waitPeriods">Periods to wait before the follow-up.</param>
    /// <returns>The follow-up, or <c>null</c> if it would fall outside the modelling horizon.</returns>
    public static TreatmentInstance? BuildFollowUpReline(
        PipeSegment segment,
        PipeConstants constants,
        ModelBase frameworkModel,
        int currentPeriod,
        int waitPeriods)
    {
        int followUpPeriod = currentPeriod + waitPeriods;

        if (followUpPeriod > frameworkModel.NPeriods)
        {
            return null;
        }

        return new TreatmentInstance(
            segment.ElementIndex,
            TreatmentNames.Reline,
            followUpPeriod,
            quantity: segment.LengthMetres,
            unitRate: constants.GetUnitRate(TreatmentNames.Reline),
            force: false,
            reason: $"Follow-up {waitPeriods} periods after the patch repair",
            comment: "none");
    }

    /// <summary>
    /// The one check the framework cannot do for you before the run: that a treatment name is
    /// actually defined in the bundle.
    ///
    /// <para>A name with no matching treatment type does not fail where the instance was created.
    /// The name is used as a dictionary key whenever a cost is allocated or a row is exported, so
    /// it fails later, during costing or export, by which point the message is about a dictionary
    /// rather than about a typo in a trigger.</para>
    ///
    /// <para>In practice <c>jcass-dm check</c> catches this before you upload, by comparing the
    /// <c>TreatmentNames</c> constants against the bundle's <c>treatments</c> sheet. This is the
    /// belt-and-braces version for a name assembled at run time rather than written as a
    /// constant.</para>
    /// </summary>
    /// <param name="treatmentName">The name about to be used.</param>
    /// <param name="frameworkModel">The framework model, for its treatment types.</param>
    public static void AssertTreatmentIsDefined(string treatmentName, ModelBase frameworkModel)
    {
        if (!frameworkModel.TreatmentTypes.ContainsKey(treatmentName))
        {
            throw new Exception(
                $"Treatment '{treatmentName}' is not defined in the treatments sheet of " +
                "domain_model_setup.xlsx. Defined treatments: " +
                string.Join(", ", frameworkModel.TreatmentTypes.Keys.Order()) + ".");
        }
    }
}
