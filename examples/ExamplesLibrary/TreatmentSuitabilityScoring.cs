using ExamplesLibrary.Shared;
using JCass_ModelCore.Models;
using JCass_ModelCore.Treatments;

namespace ExamplesLibrary;

/// <summary>
/// PATTERN: treatment-suitability-scoring. Telling the optimiser how badly each candidate is
/// wanted.
///
/// <para>Documentation: <c>docs\patterns\treatment-suitability-scoring.md</c>. API:
/// <c>docs\framework\api\authoring\TreatmentInstance.md</c>, properties
/// <c>TreatmentSuitabilityScore</c> and <c>RankParamSimple</c>. Where the weights come from:
/// <c>docs\patterns\constants-from-lookups.md</c>.</para>
///
/// <para><b>Two properties, two different mechanisms, and both fail silently when left
/// unset.</b></para>
///
/// <para><c>TreatmentSuitabilityScore</c> is what an MCDA model ranks capital candidates by.
/// Leave it at its default of zero and the candidate is never preferred over one that has a
/// score, and nothing anywhere reports that it was passed over. The run completes; the treatment
/// simply never happens.</para>
///
/// <para><c>RankParamSimple</c> is the equivalent for routine maintenance, and it works
/// differently: maintenance is not optimised, it is sorted by this value descending and funded
/// down the list until the maintenance budget runs out. Left at zero every candidate compares
/// equal, so what gets funded is decided by the order elements happen to be processed in - which
/// is stable enough between runs to look deliberate, and is not.</para>
///
/// <para><b>Why a separate class.</b> Scoring is a modelling decision in its own right, it is
/// used from more than one trigger, and it is the thing a reviewer most wants to read on its own.
/// Three of the four working models keep it in its own file for that reason.</para>
/// </summary>
public static class TreatmentSuitabilityScoring
{
    /// <summary>
    /// Score for a replacement candidate.
    ///
    /// <para><b>Score the element's need, not the treatment's merit.</b> The optimiser is
    /// deciding which elements to spend on, so a score that varies with the treatment rather than
    /// with the element makes every element look equally urgent and the ranking stops meaning
    /// anything. Here the two curves differ in shape - replacement climbs with need, relining
    /// peaks in the middle band - but both are driven by the same underlying rank.</para>
    /// </summary>
    /// <param name="segment">The segment being scored.</param>
    /// <param name="constants">Weights read from <c>lookups.xlsx</c>.</param>
    /// <param name="subModels">The suitability curves built at setup.</param>
    public static double GetReplaceScore(
        PipeSegment segment,
        PipeConstants constants,
        PipeSubModels subModels)
    {
        double need = GetNeedRank(segment, constants);
        return subModels.ReplaceSuitabilityCurve.GetValue(need);
    }

    /// <summary>
    /// Score for a relining or patch-repair candidate. Same input rank, different curve.
    /// </summary>
    /// <param name="segment">The segment being scored.</param>
    /// <param name="constants">Weights read from <c>lookups.xlsx</c>.</param>
    /// <param name="subModels">The suitability curves built at setup.</param>
    public static double GetRelineScore(
        PipeSegment segment,
        PipeConstants constants,
        PipeSubModels subModels)
    {
        double need = GetNeedRank(segment, constants);
        return subModels.RelineSuitabilityCurve.GetValue(need);
    }

    /// <summary>
    /// Combines condition and criticality into a single 0-100 need rank.
    ///
    /// <para><b>The weights come from <c>lookups.xlsx</c>, and this is exactly the kind of number
    /// a modeller changes.</b> "How much should consequence-of-failure count relative to
    /// condition?" is the question a calibration workshop argues about for an hour; it must not
    /// require a developer to answer. The <i>scales</i> - condition grade running 1 to 5, the
    /// rank running 0 to 100 - are structural and stay in C#.</para>
    /// </summary>
    /// <param name="segment">The segment being scored.</param>
    /// <param name="constants">Weights read from <c>lookups.xlsx</c>.</param>
    private static double GetNeedRank(PipeSegment segment, PipeConstants constants)
    {
        // Condition grade runs 1 (as new) to 5 (failed): structural, part of the data's
        // definition rather than a calibration choice.
        const double worstGrade = 5.0;
        const double bestGrade = 1.0;
        const double rankScaleMaximum = 100.0;

        double conditionFraction = (segment.ConditionGrade - bestGrade) / (worstGrade - bestGrade);
        conditionFraction = Math.Clamp(conditionFraction, 0.0, 1.0);

        double weightedTotal = constants.ConditionWeight * conditionFraction
                             + constants.CriticalityWeight * segment.CriticalityScore;

        double weightSum = constants.ConditionWeight + constants.CriticalityWeight;

        if (weightSum <= 0)
        {
            // Both weights zeroed would divide by zero and produce NaN scores on every candidate,
            // which the optimiser would treat as unrankable rather than as an error. Say so here.
            throw new Exception(
                "condition_weight and criticality_weight in lookup set 'scoring_weights' are both " +
                "zero, so no candidate can be ranked. Set at least one of them above zero.");
        }

        return rankScaleMaximum * weightedTotal / weightSum;
    }

    /// <summary>
    /// Sets the maintenance priority on a routine maintenance treatment.
    ///
    /// <para><b>Set this whenever maintenance can be budget-constrained</b>, which in practice
    /// means almost always. It is the entire control a domain model has over what gets done first
    /// when maintenance money is short; the framework does nothing cleverer than sort on it.</para>
    ///
    /// <para>Anything that expresses urgency in the domain will do - severity, a condition index,
    /// exposure, cost-effectiveness. Here it is the same need rank the capital candidates use,
    /// which keeps one definition of "urgent" across the model.</para>
    /// </summary>
    /// <param name="maintenance">The maintenance treatment, already constructed.</param>
    /// <param name="segment">The segment it applies to.</param>
    /// <param name="constants">Weights read from <c>lookups.xlsx</c>.</param>
    public static void SetMaintenancePriority(
        TreatmentInstance maintenance,
        PipeSegment segment,
        PipeConstants constants)
    {
        maintenance.RankParamSimple = GetNeedRank(segment, constants);
    }

    /// <summary>
    /// The configured floor a capital candidate must clear to be worth offering at all.
    ///
    /// <para>The framework exposes this as
    /// <c>model.Configuration.MinimumTreatmentSuitabilityScoreAllowed</c>, set in the project's
    /// configuration rather than in code. Checking it in the trigger - and returning early rather
    /// than adding the candidate - keeps candidates the model would never fund out of the
    /// strategy rollout entirely, which is worth real time on a large network.</para>
    /// </summary>
    /// <param name="score">Score from one of the curves above.</param>
    /// <param name="frameworkModel">The framework model, for its configuration.</param>
    /// <returns>True if the candidate clears the floor and is worth offering.</returns>
    public static bool IsWorthOffering(double score, ModelBase frameworkModel)
        => score > frameworkModel.Configuration.MinimumTreatmentSuitabilityScoreAllowed;
}
