using System;

namespace DomainModelSample.Objects;

/// <summary>
/// One asset in the network, as this domain model sees it. This is the class you will change
/// most: it holds the state that carries from one modelling period to the next, and the rules
/// that move that state forward (<see cref="Increment"/>) or snap it back after a treatment
/// (<see cref="Reset"/>).
///
/// <para>Nothing here is framework machinery. The framework never sees this type — it hands the
/// domain model dictionaries of raw inputs and parameter values, and <see cref="ElementFactory"/>
/// turns those into one of these. Working in plain objects rather than dictionaries is a
/// convention, not a requirement, but it is what keeps the modelling rules readable.</para>
/// </summary>
public class SampleElement
{
    private double _objectiveValue;

    /// <summary>Zero-based index of this element in the model's input data.</summary>
    public int ElementIndex { get; set; }

    /// <summary>Identifier from the <c>element_name</c> input column. Carried for reporting only.</summary>
    public string ElementName { get; set; } = string.Empty;

    /// <summary>Condition on a 0 (good) to 100 (poor) scale. Deteriorates upward.</summary>
    public double ConditionRating { get; set; }

    /// <summary>Material from the <c>material</c> input column. Drives both deterioration and cost.</summary>
    public string MaterialType { get; set; } = string.Empty;

    /// <summary>Element area in square metres, from the <c>area_sqm</c> input column.</summary>
    public double AreaSquareMetre { get; set; }

    /// <summary>Years since the element was new or last replaced.</summary>
    public int Age { get; set; }

    /// <summary>
    /// Score the optimiser ranks candidate treatments by. Higher means "treating this element is
    /// more worthwhile". Recomputed by <see cref="SetObjectiveValue"/> whenever state changes.
    /// </summary>
    public double ObjectiveValue => _objectiveValue;

    /// <summary>
    /// Recomputes <see cref="ObjectiveValue"/> from current state. Call this after any change to
    /// condition or age. The formula is deliberately trivial — a real model would weigh traffic,
    /// criticality, consequence of failure and so on.
    /// </summary>
    public void SetObjectiveValue()
    {
        _objectiveValue = this.ConditionRating * Math.Sqrt(this.Age);
    }

    /// <summary>
    /// Writes this element's state back into the framework's parameter store. Every parameter
    /// named in the <c>parameters</c> sheet of <c>domain_model_setup.xlsx</c> must be written
    /// here, or the framework will report a missing-parameter setup error.
    /// </summary>
    /// <param name="numModParamValues">Sink for numeric parameters, supplied by the framework.</param>
    /// <param name="textModParamValues">Sink for text parameters, supplied by the framework. Unused in this model.</param>
    public void SetParameterValues(Action<string, double> numModParamValues, Action<string, string> textModParamValues)
    {
        numModParamValues("par_age", this.Age);
        numModParamValues("par_cond_rating", this.ConditionRating);
        numModParamValues("par_obj", this.ObjectiveValue);
    }

    /// <summary>
    /// Advances the element by one modelling period when no treatment was applied. This is the
    /// deterioration model.
    ///
    /// <para>The per-material rates below are still in code. Thresholds and cost adjustments are
    /// not — those come from <c>lookups.xlsx</c> via <see cref="Constants"/>. Moving these too is
    /// the exercise in README section 7, and it is worth doing in a real model: deterioration
    /// rates are exactly the kind of number a modeller recalibrates against observed data.</para>
    /// </summary>
    public void Increment()
    {
        this.Age += 1;
        this.ConditionRating += this.GetDeteriorationRate();
        this.SetObjectiveValue();
    }

    /// <summary>
    /// Snaps the element back to its post-treatment state. Called by the framework only for
    /// elements that actually received a treatment in this period, and only after the optimiser
    /// has decided. The treatment names handled here must match the <c>treatment_name</c> values
    /// in the <c>treatments</c> sheet of <c>domain_model_setup.xlsx</c>.
    /// </summary>
    /// <param name="treatmentName">Name of the treatment that was applied.</param>
    public void Reset(string treatmentName)
    {
        switch (treatmentName)
        {
            case TreatmentNames.Repair:
                // A repair halves the remaining defect but does not make the element new.
                this.ConditionRating *= 0.5;
                this.Age = 0;
                break;

            case TreatmentNames.Replace:
                this.ConditionRating = 10;
                this.Age = 0;
                break;

            case TreatmentNames.RoutineMaintenance:
                // Routine maintenance holds condition where it is; it does not reset age.
                break;

            default:
                throw new Exception($"Treatment '{treatmentName}' is not handled by SampleElement.Reset().");
        }

        this.SetObjectiveValue();
    }

    /// <summary>
    /// Condition points added per year for this element's material. Higher means faster decay.
    ///
    /// <para><b>DELIBERATE COUNTER-EXAMPLE — do not copy this shape.</b> These rates are hard-coded
    /// in C# on purpose, as the contrast that makes the rule visible. A deterioration rate is
    /// exactly what a modeller recalibrates against observed condition data, so in a real model it
    /// belongs in <c>inputs\lookups.xlsx</c> and is read through <see cref="Constants"/>. As
    /// written, changing one needs a developer, a rebuild and a republish. Moving them is the
    /// reader's second exercise — README section 7.</para>
    /// </summary>
    public double GetDeteriorationRate()
    {
        // COUNTER-EXAMPLE. See the summary above and README section 7. In your own model these
        // rates come from lookups.xlsx through Constants, not from a switch expression.
        return this.MaterialType.ToLower() switch
        {
            "metal" => 2.0,
            "plastic" => 5.0,
            "concrete" => 1.5,
            "cast-iron" => 2.2,
            "titanium" => 0.5,
            _ => throw new Exception($"Material type '{this.MaterialType}' is not recognised."),
        };
    }

    /// <summary>
    /// Cost per square metre of replacing this element, by material.
    ///
    /// <para><b>DELIBERATE COUNTER-EXAMPLE — do not copy this shape.</b> A unit rate is the single
    /// most-often-retuned number in any model, and the web app's Tuning page exists to edit rates
    /// without a code change. Hard-coded here on purpose, as the contrast. See README section 7,
    /// and <see cref="Constants.GetUnitRate"/> for how the same model does it properly for the
    /// per-treatment rate multiplier.</para>
    /// </summary>
    public double GetReplacementRate()
    {
        // COUNTER-EXAMPLE. See the summary above and README section 7.
        return this.MaterialType.ToLower() switch
        {
            "metal" => 60,
            "plastic" => 30,
            "concrete" => 90,
            "cast-iron" => 75,
            "titanium" => 120,
            _ => throw new Exception($"Material type '{this.MaterialType}' is not recognised."),
        };
    }

    /// <summary>Cost per square metre of repairing this element. A repair costs a third of a replacement.</summary>
    public double GetRepairRate()
    {
        return this.GetReplacementRate() / 3;
    }
}
