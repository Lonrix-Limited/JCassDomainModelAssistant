using ExamplesLibrary.Shared;
using JCass_ModelCore.Models;
using JCass_ModelCore.Treatments;

namespace ExamplesLibrary;

/// <summary>
/// PATTERN: candidate-strategies. What the optimiser gets to choose between - and who builds it.
///
/// <para>Documentation: <c>docs\patterns\candidate-strategies.md</c>. API:
/// <c>docs\framework\api\authoring\StrategySetupInfo.md</c> and
/// <c>docs\framework\api\authoring\TreatmentInstance.md</c>.</para>
///
/// <para><b>You return candidates. The framework builds the strategies.</b> This is the thing to
/// get straight before writing anything, because the natural assumption is the opposite. In a
/// benefit-cost run the framework calls <c>GetTreatmentCandidates</c> for each element, then
/// hands the list to its own strategy generator, which rolls each candidate forward into
/// multi-period strategies - do it now, do it in three periods, do nothing - and scores them. A
/// domain model does not assemble a <c>TreatmentStrategy</c>; that type is on the framework's
/// "recognise it, do not construct it" list for exactly this reason.</para>
///
/// <para><b>So the leverage is entirely in what the candidate list contains.</b> Return one
/// candidate and the optimiser has a yes/no funding decision. Return two - a cheap holding action
/// alongside the permanent fix - and it can trade elements against each other under a budget,
/// which is the whole reason to run an optimiser rather than a sorted list.</para>
///
/// <para><b>Return an empty list, never <c>null</c>, when nothing is due.</b> For most elements
/// in most periods that is the right answer, and it is not a failure.</para>
/// </summary>
public static class CandidateStrategies
{
    /// <summary>
    /// Returns every treatment the optimiser may consider for this segment in this period.
    ///
    /// <para>Read the order. Each rule is a small method that adds at most one candidate, and the
    /// alternative is offered last because it inspects what the earlier rules already added. That
    /// shape - one <c>Add...IfValid</c> per treatment, composed in a readable sequence - is what
    /// every working model converges on, and it is worth keeping when the rules get complicated,
    /// because the alternative is a nest of conditions no reviewer can check.</para>
    /// </summary>
    /// <param name="segment">The segment under test.</param>
    /// <param name="constants">Thresholds and rates read from <c>lookups.xlsx</c>.</param>
    /// <param name="subModels">Sub-models built at setup, for the suitability curves.</param>
    /// <param name="period">Modelling period (1-based).</param>
    public static List<TreatmentInstance> GetCandidates(
        PipeSegment segment,
        PipeConstants constants,
        PipeSubModels subModels,
        int period)
    {
        List<TreatmentInstance> candidates = new List<TreatmentInstance>();

        AddPatchRepairIfValid(segment, constants, subModels, period, candidates);
        AddRelineIfValid(segment, constants, subModels, period, candidates);
        AddReplaceIfValid(segment, constants, subModels, period, candidates);
        AddReplaceAsAlternativeIfValid(segment, constants, subModels, period, candidates);

        return candidates;
    }

    /// <summary>Adds a patch repair when the segment is bad enough to need one but no worse.</summary>
    private static void AddPatchRepairIfValid(
        PipeSegment segment,
        PipeConstants constants,
        PipeSubModels subModels,
        int period,
        List<TreatmentInstance> candidates)
    {
        if (segment.ConditionGrade <= constants.RepairConditionGreaterThan) return;
        if (segment.ConditionGrade > constants.RelineConditionGreaterThan) return;

        TreatmentInstance treatment = TreatmentInstances.BuildPatchRepair(segment, constants, period);
        treatment.TreatmentSuitabilityScore =
            TreatmentSuitabilityScoring.GetRelineScore(segment, constants, subModels);

        candidates.Add(treatment);
    }

    /// <summary>Adds a relining when the segment is in the band a liner can still help.</summary>
    private static void AddRelineIfValid(
        PipeSegment segment,
        PipeConstants constants,
        PipeSubModels subModels,
        int period,
        List<TreatmentInstance> candidates)
    {
        if (segment.ConditionGrade <= constants.RelineConditionGreaterThan) return;
        if (segment.ConditionGrade > constants.RelineConditionAtMost) return;

        TreatmentInstance treatment = new TreatmentInstance(
            segment.ElementIndex,
            TreatmentNames.Reline,
            period,
            quantity: segment.LengthMetres,
            unitRate: constants.GetUnitRate(TreatmentNames.Reline),
            force: false,
            reason: $"Condition {Math.Round(segment.ConditionGrade, 1)} in the relinable band",
            comment: "none");

        treatment.TreatmentSuitabilityScore =
            TreatmentSuitabilityScoring.GetRelineScore(segment, constants, subModels);

        candidates.Add(treatment);
    }

    /// <summary>Adds a replacement when the segment is beyond what a liner can fix.</summary>
    private static void AddReplaceIfValid(
        PipeSegment segment,
        PipeConstants constants,
        PipeSubModels subModels,
        int period,
        List<TreatmentInstance> candidates)
    {
        bool conditionTriggers = segment.ConditionGrade > constants.ReplaceConditionGreaterThan;
        bool breakRateTriggers = segment.BreakRatePerKmYear > constants.ReplaceBreakRateGreaterThan;

        if (!conditionTriggers && !breakRateTriggers) return;

        TreatmentInstance treatment = TreatmentInstances.BuildReplace(segment, constants, period);
        treatment.TreatmentSuitabilityScore =
            TreatmentSuitabilityScoring.GetReplaceScore(segment, constants, subModels);

        candidates.Add(treatment);
    }

    /// <summary>
    /// Offers "replace it properly instead" alongside a triggered holding action.
    ///
    /// <para><b>This method is the pattern.</b> Everything above it decides whether work is due;
    /// this one decides whether the optimiser gets a choice about how. A segment already bad
    /// enough to have triggered a replacement on its own account gets no second option, because
    /// there is no genuine alternative to offer - adding one would only pad the strategy count
    /// and slow the run down.</para>
    /// </summary>
    private static void AddReplaceAsAlternativeIfValid(
        PipeSegment segment,
        PipeConstants constants,
        PipeSubModels subModels,
        int period,
        List<TreatmentInstance> candidates)
    {
        bool holdingActionTriggered = candidates.Exists(
            c => c.TreatmentName == TreatmentNames.PatchRepair || c.TreatmentName == TreatmentNames.Reline);

        bool replaceAlreadyOffered = candidates.Exists(c => c.TreatmentName == TreatmentNames.Replace);

        if (!holdingActionTriggered || replaceAlreadyOffered) return;

        TreatmentInstance treatment = TreatmentInstances.BuildReplace(segment, constants, period);
        treatment.Reason = "Alternative to the triggered holding action";
        treatment.TreatmentSuitabilityScore =
            TreatmentSuitabilityScoring.GetReplaceScore(segment, constants, subModels);

        candidates.Add(treatment);
    }

    /// <summary>
    /// Reading the project's strategy definitions, for a model that has them.
    ///
    /// <para><b>Only some projects define strategies</b>, in the setup's strategies sheet, and
    /// <c>model.StrategiesSetupData</c> is empty in the ones that do not. Where they exist, each
    /// entry names a first treatment and up to three follow-ups with wait periods, and a domain
    /// model reads them to decide which of the project's defined strategies are worth offering on
    /// a given element.</para>
    ///
    /// <para><b>Reading them is still not building them.</b> What you return is a candidate for
    /// the first treatment; the framework's generator handles the rollout, including the wait
    /// periods. This method returns the names it would offer rather than a list of strategies,
    /// which is the honest shape of the decision.</para>
    ///
    /// <para><b>Order matters and the first force wins.</b> The setup lists strategies in
    /// priority order, and where one is marked as forced the convention in the working models is
    /// to stop there rather than offer anything below it - a forced strategy is a decision
    /// already taken, so continuing to offer alternatives to it is a contradiction.</para>
    /// </summary>
    /// <param name="segment">The segment under test.</param>
    /// <param name="constants">Thresholds read from <c>lookups.xlsx</c>.</param>
    /// <param name="frameworkModel">The framework model, for <c>StrategiesSetupData</c>.</param>
    /// <returns>The first-treatment names this segment should be offered, in priority order.</returns>
    public static List<string> GetApplicableStrategyTreatments(
        PipeSegment segment,
        PipeConstants constants,
        ModelBase frameworkModel)
    {
        List<string> firstTreatments = new List<string>();

        foreach (StrategySetupInfo strategy in frameworkModel.StrategiesSetupData)
        {
            if (!IsStrategyApplicable(strategy, segment, constants)) continue;

            firstTreatments.Add(strategy.FirstTreatment);

            if (strategy.ForceFirstTreatment) break;
        }

        return firstTreatments;
    }

    /// <summary>
    /// Whether one defined strategy suits this segment. Domain rules go here, and this is the
    /// only part of the strategy story a domain model actually owns.
    /// </summary>
    /// <param name="strategy">One entry from the project's strategies sheet.</param>
    /// <param name="segment">The segment under test.</param>
    /// <param name="constants">Thresholds read from <c>lookups.xlsx</c>.</param>
    private static bool IsStrategyApplicable(
        StrategySetupInfo strategy,
        PipeSegment segment,
        PipeConstants constants)
    {
        // A lining strategy is pointless on a segment already past the relinable band, however
        // the project has defined it.
        if (strategy.FirstTreatment == TreatmentNames.Reline &&
            segment.ConditionGrade > constants.RelineConditionAtMost)
        {
            return false;
        }

        // Nothing is worth doing on a segment still in good condition.
        return segment.ConditionGrade > constants.RepairConditionGreaterThan;
    }
}
