using System;
using System.Collections.Generic;
using JCass_ModelCore.DomainModels;
using JCass_ModelCore.Treatments;

namespace DomainModelSample.Objects;

/// <summary>
/// Entry point of this domain model. The framework finds this class by reflection, creates one
/// instance of it, and calls the methods below once per element per modelling period.
///
/// <para><b>The name of this class is load-bearing.</b> It must match the .csproj file stem, the
/// assembly name, and <c>meta.main_class</c> in <c>domain_model_setup.xlsx</c> — all four must
/// read <c>DomainModelSample</c>. See "The four-name rule" in README.md before renaming
/// anything. Regular runs and web debug runs resolve this name by different routes, and they only
/// agree when all four strings are identical.</para>
///
/// <para>Keep this class thin. It is a switchboard: it converts the framework's dictionaries into
/// a <see cref="SampleElement"/> and delegates the actual thinking to
/// <see cref="TreatmentTrigger"/> and <see cref="StrategyGenerator"/>. Every modelling rule you
/// add belongs in one of those, not here.</para>
///
/// <para><b>Call order, per period.</b> <c>Initialise</c> runs once for every element before
/// period 1. Then for each period: <c>GetTreatmentCandidates</c> for every element, the optimiser
/// picks winners under the budget, then <c>Reset</c> for treated elements and <c>Increment</c>
/// for untreated ones, then <c>GetTriggeredMaintenance</c>, then
/// <c>DoEndOfPeriodCalculations</c> once for the whole network.</para>
/// </summary>
public class DomainModelSample : DomainModelBase
{
    /// <summary>
    /// Every tunable number this model uses, read from the client's <c>inputs\lookups.xlsx</c>.
    /// Populated once by <see cref="SetupInstance"/> and read-only thereafter.
    /// </summary>
    public Constants Constants { get; private set; } = null!;   // assigned in SetupInstance, before any element is touched

    /// <summary>
    /// Called once, after the framework has loaded lookups and treatment rates but before any
    /// element is touched. Use it to cache anything that is the same for every element.
    ///
    /// <para><b>This is the only place lookups may be read.</b> <c>model.Lookups</c> is populated
    /// by the framework immediately before this call and not before it, so a lookup read from a
    /// constructor or a static initialiser gets an empty dictionary. That ordering guarantee is
    /// the entire reason this method exists.</para>
    /// </summary>
    public override void SetupInstance()
    {
        this.Constants = new Constants(this.model.Lookups);
    }

    /// <summary>
    /// Sets each element's starting state, before period 1. Read the raw input columns, decide the
    /// initial value of every model parameter, and write them all back through the sinks.
    /// </summary>
    /// <param name="iElemIndex">Zero-based index of the element.</param>
    /// <param name="numInputs">Numeric raw input columns, keyed by column name.</param>
    /// <param name="textInputs">Text raw input columns, keyed by column name.</param>
    /// <param name="numModParamValues">Sink for numeric parameter values.</param>
    /// <param name="textModParamValues">Sink for text parameter values.</param>
    public override void Initialise(
        int iElemIndex,
        Dictionary<string, double> numInputs,
        Dictionary<string, string> textInputs,
        Action<string, double> numModParamValues,
        Action<string, string> textModParamValues)
    {
        SampleElement element = ElementFactory.GetElementFromInputData(iElemIndex, numInputs, textInputs);
        element.SetParameterValues(numModParamValues, textModParamValues);
    }

    /// <summary>
    /// Returns the treatments the optimiser may consider for this element in this period. An empty
    /// list means "leave this element alone".
    /// </summary>
    /// <param name="iElemIndex">Zero-based index of the element.</param>
    /// <param name="iPeriod">Modelling period (1-based).</param>
    /// <param name="numInputs">Numeric raw input columns, keyed by column name.</param>
    /// <param name="textInputs">Text raw input columns, keyed by column name.</param>
    /// <param name="numModParamValues">Numeric parameters as at the previous period.</param>
    /// <param name="textModParamValues">Text parameters as at the previous period.</param>
    public override List<TreatmentInstance> GetTreatmentCandidates(
        int iElemIndex,
        int iPeriod,
        Dictionary<string, double> numInputs,
        Dictionary<string, string> textInputs,
        Dictionary<string, double> numModParamValues,
        Dictionary<string, string> textModParamValues)
    {
        SampleElement element = ElementFactory.GetElementFromModelData(
            iElemIndex, numInputs, textInputs, numModParamValues, textModParamValues);

        return StrategyGenerator.GetCandidates(element, this.Constants, iPeriod);
    }

    /// <summary>
    /// Returns routine maintenance for this element in this period, or <c>null</c> if none is due.
    /// Maintenance is applied after the optimiser has run and does not compete for capital budget.
    /// </summary>
    /// <param name="ielem">Zero-based index of the element.</param>
    /// <param name="iPeriod">Modelling period (1-based).</param>
    /// <param name="numInputs">Numeric raw input columns, keyed by column name.</param>
    /// <param name="textInputs">Text raw input columns, keyed by column name.</param>
    /// <param name="numModParamValues">Numeric parameters as at the previous period.</param>
    /// <param name="textModParamValues">Text parameters as at the previous period.</param>
    public override TreatmentInstance GetTriggeredMaintenance(
        int ielem,
        int iPeriod,
        Dictionary<string, double> numInputs,
        Dictionary<string, string> textInputs,
        Dictionary<string, double> numModParamValues,
        Dictionary<string, string> textModParamValues)
    {
        SampleElement element = ElementFactory.GetElementFromModelData(
            ielem, numInputs, textInputs, numModParamValues, textModParamValues);

        // The framework's caller treats this result as nullable (see ModelBase.GetTriggeredMaintenance),
        // but the abstract signature it overrides is not annotated as such. Returning null! is the
        // documented way to say "no maintenance" until the base signature is updated.
        return TreatmentTrigger.GetTriggeredMaintenance(element, this.Constants, iPeriod)!;
    }

    /// <summary>
    /// Advances an element by one period when it received no treatment. This is the deterioration
    /// step, and it is called for the great majority of elements in any period.
    /// </summary>
    /// <param name="iElemIndex">Zero-based index of the element.</param>
    /// <param name="iPeriod">Modelling period (1-based).</param>
    /// <param name="numInputs">Numeric raw input columns, keyed by column name.</param>
    /// <param name="textInputs">Text raw input columns, keyed by column name.</param>
    /// <param name="currentNumModParamValues">Numeric parameters as at the previous period.</param>
    /// <param name="currentTextModParamValues">Text parameters as at the previous period.</param>
    /// <param name="numModParamValues">Sink for this period's numeric parameter values.</param>
    /// <param name="textModParamValues">Sink for this period's text parameter values.</param>
    public override void Increment(
        int iElemIndex,
        int iPeriod,
        Dictionary<string, double> numInputs,
        Dictionary<string, string> textInputs,
        Dictionary<string, double> currentNumModParamValues,
        Dictionary<string, string> currentTextModParamValues,
        Action<string, double> numModParamValues,
        Action<string, string> textModParamValues)
    {
        SampleElement element = ElementFactory.GetElementFromModelData(
            iElemIndex, numInputs, textInputs, currentNumModParamValues, currentTextModParamValues);

        element.Increment();
        element.SetParameterValues(numModParamValues, textModParamValues);
    }

    /// <summary>
    /// Applies the effect of a treatment the optimiser selected, then writes the resulting state
    /// back. Called instead of <see cref="Increment"/> for treated elements.
    /// </summary>
    /// <param name="treatment">The treatment that was selected for this element.</param>
    /// <param name="iElemIndex">Zero-based index of the element.</param>
    /// <param name="iPeriod">Modelling period (1-based).</param>
    /// <param name="numInputs">Numeric raw input columns, keyed by column name.</param>
    /// <param name="textInputs">Text raw input columns, keyed by column name.</param>
    /// <param name="currentNumModParamValues">Numeric parameters as at the previous period.</param>
    /// <param name="currentTextModParamValues">Text parameters as at the previous period.</param>
    /// <param name="numModParamValues">Sink for this period's numeric parameter values.</param>
    /// <param name="textModParamValues">Sink for this period's text parameter values.</param>
    public override void Reset(
        TreatmentInstance treatment,
        int iElemIndex,
        int iPeriod,
        Dictionary<string, double> numInputs,
        Dictionary<string, string> textInputs,
        Dictionary<string, double> currentNumModParamValues,
        Dictionary<string, string> currentTextModParamValues,
        Action<string, double> numModParamValues,
        Action<string, string> textModParamValues)
    {
        SampleElement element = ElementFactory.GetElementFromModelData(
            iElemIndex, numInputs, textInputs, currentNumModParamValues, currentTextModParamValues);

        element.Reset(treatment.TreatmentName);
        element.SetParameterValues(numModParamValues, textModParamValues);
    }

    /// <summary>
    /// Called once at the end of each period, after every element has been processed. Use it for
    /// network-level work that needs the whole population — rankings, percentiles, proportions
    /// over a threshold — and store the result on this instance for the next period to read.
    ///
    /// <para>Anything you store here without indexing by period is overwritten every period. That
    /// is usually what you want; when it is not, key your dictionary by <paramref name="iPeriod"/>.</para>
    /// </summary>
    /// <param name="iPeriod">Modelling period (1-based) that has just finished.</param>
    public override void DoEndOfPeriodCalculations(int iPeriod)
    {
        // This model needs no network-level calculations.
    }
}
