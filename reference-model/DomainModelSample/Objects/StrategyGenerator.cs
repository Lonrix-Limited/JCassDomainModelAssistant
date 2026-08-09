using System.Collections.Generic;
using JCass_ModelCore.Treatments;

namespace DomainModelSample.Objects;

/// <summary>
/// Turns "this element is due for work" into the set of options the optimiser gets to choose
/// between. <see cref="TreatmentTrigger"/> answers *whether* and *what*; this class answers
/// *what else could we do instead*.
///
/// <para>Why this is a separate file: giving the optimiser a single candidate reduces it to a
/// yes/no funding decision. Giving it two or more — a cheap holding action alongside a permanent
/// fix — is what lets it trade elements off against each other under a budget, and it is what
/// the benefit-cost models need in order to have anything to compare.</para>
///
/// <para>Note the division of labour with the framework. This model returns candidates for the
/// <em>current period only</em>. The framework's own <c>TreatmentStrategyGenerator</c> takes those
/// candidates and rolls each one forward into multi-period strategies (do it now, do it in three
/// years, do nothing) before the benefit-cost optimiser scores them. You do not write that part.</para>
/// </summary>
public static class StrategyGenerator
{
    /// <summary>
    /// Returns every treatment the optimiser may consider for this element in this period.
    /// Returns an empty list — never <c>null</c> — when nothing is triggered.
    /// </summary>
    /// <param name="element">The element under consideration.</param>
    /// <param name="constants">Thresholds and rates read from lookups.xlsx.</param>
    /// <param name="iPeriod">Modelling period (1-based).</param>
    public static List<TreatmentInstance> GetCandidates(SampleElement element, Constants constants, int iPeriod)
    {
        List<TreatmentInstance> candidates = new List<TreatmentInstance>();

        TreatmentInstance? triggered = TreatmentTrigger.GetTriggeredTreatment(element, constants, iPeriod);
        if (triggered is null) return candidates;

        candidates.Add(triggered);

        // An element that only needs a repair could always be replaced instead. Offering both
        // lets the optimiser decide whether the permanent fix is worth its extra cost. An element
        // already bad enough to need replacing gets no second option — a repair would not help it.
        if (triggered.TreatmentName == TreatmentNames.Repair)
        {
            candidates.Add(BuildReplaceAlternative(element, constants, iPeriod));
        }

        return candidates;
    }

    /// <summary>
    /// Builds the "replace it properly instead" alternative for an element that only triggered a
    /// repair. Priced the same way a triggered replacement would be, so the optimiser is comparing
    /// like with like.
    /// </summary>
    /// <param name="element">The element under consideration.</param>
    /// <param name="constants">Thresholds and rates read from lookups.xlsx.</param>
    /// <param name="iPeriod">Modelling period (1-based).</param>
    private static TreatmentInstance BuildReplaceAlternative(SampleElement element, Constants constants, int iPeriod)
    {
        TreatmentInstance alternative = new TreatmentInstance(
            element.ElementIndex,
            TreatmentNames.Replace,
            iPeriod,
            quantity: element.AreaSquareMetre,
            unitRate: element.GetReplacementRate() * constants.GetUnitRate(TreatmentNames.Replace),
            force: false,
            reason: "Alternative to the triggered repair",
            comment: "none");

        alternative.TreatmentSuitabilityScore = element.ObjectiveValue;
        return alternative;
    }
}
