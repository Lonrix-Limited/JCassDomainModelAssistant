using ExamplesLibrary.Shared;
using JCass_Core.Statistics;
using JCass_ModelCore.DomainModels;
using JCass_ModelCore.MonteCarlo;
using JCass_ModelCore.Treatments;

namespace ExamplesLibrary;

/// <summary>
/// The examples' host model.
///
/// <para><b>This is not the model to copy.</b> It exists so the setup patterns have somewhere
/// real to run from - <c>SetupInstance</c> is the only method here with anything in it. For a
/// complete, small, working model read
/// <c>reference-model\DomainModelSample\README.md</c>; to start your own, run
/// <c>jcass-dm scaffold MyModel --from-sample</c>.</para>
///
/// <para><b>What IS worth taking from it is the shape of <c>SetupInstance</c>:</b> one line per
/// thing being set up, each delegating to a named helper, wrapped in a try/catch that says which
/// model failed. That is what every working model does, and it is what makes a setup failure
/// readable - the framework wraps the exception again on the way out, and without a sentence
/// naming the stage the engineer gets a stack trace and nothing else.</para>
/// </summary>
public class PipeNetworkModel : DomainModelBase
{
    /// <summary>
    /// Every tunable number, read from <c>inputs\lookups.xlsx</c>. Assigned in
    /// <see cref="SetupInstance"/>, read-only thereafter.
    /// </summary>
    public PipeConstants Constants { get; private set; } = null!;

    /// <summary>
    /// The fitted sub-models loaded from CSVs in the client's <c>supporting\</c> folder.
    /// Assigned in <see cref="SetupInstance"/>.
    /// </summary>
    public PipeSubModels SubModels { get; } = new PipeSubModels();

    /// <summary>
    /// Called once, after the framework has loaded lookups, treatment types and the budget, and
    /// before any element is touched. Everything that is the same for every element belongs here.
    ///
    /// <para><b>What is ready:</b> <c>model.Lookups</c>, <c>model.TreatmentTypes</c>,
    /// <c>model.Budget</c>, <c>model.Configuration</c> - including <c>WorkFolder</c>, which is
    /// how the <c>supporting\</c> CSVs below are found.</para>
    ///
    /// <para><b>What is not, and fails silently:</b> <c>model.NElements</c>,
    /// <c>model.NPeriods</c> and <c>model.NParameters</c> are all still zero here. Sizing an
    /// array off one of them produces an empty array and a run that completes with nothing in
    /// it.</para>
    /// </summary>
    public override void SetupInstance()
    {
        try
        {
            // Tunable scalars first: everything below may need one.
            this.Constants = new PipeConstants(this.model.Lookups);

            // Then the fitted sets, each from a CSV in the client's supporting\ folder.
            string workFolder = this.model.Configuration.WorkFolder;

            SetupDataFromSupportingCsv.LoadDeteriorationCurves(this.SubModels, workFolder);
            DistributionSimulators.LoadBreakRateSimulators(this.SubModels, workFolder);
            LogisticCoefficients.LoadFailureProbabilityModels(this.SubModels, workFolder);
            PiecewiseLinearModels.BuildSuitabilityCurves(this.SubModels, this.Constants);
        }
        catch (Exception ex)
        {
            // Name the stage. Without this the engineer sees only the framework's own wrapper.
            throw new Exception($"Error setting up {nameof(PipeNetworkModel)}: {ex.Message}");
        }
    }

    /// <summary>Not implemented - see the reference model. Present only to satisfy the base class.</summary>
    public override void Initialise(
        int iElemIndex,
        Dictionary<string, double> numInputs,
        Dictionary<string, string> textInputs,
        Action<string, double> numModParamValues,
        Action<string, string> textModParamValues)
        => throw new NotSupportedException(NotAModelMessage);

    /// <summary>Not implemented - see the reference model. Present only to satisfy the base class.</summary>
    public override List<TreatmentInstance> GetTreatmentCandidates(
        int iElemIndex,
        int iPeriod,
        Dictionary<string, double> numInputs,
        Dictionary<string, string> textInputs,
        Dictionary<string, double> numModParamValues,
        Dictionary<string, string> textModParamValues)
        => throw new NotSupportedException(NotAModelMessage);

    /// <summary>Not implemented - see the reference model. Present only to satisfy the base class.</summary>
    public override TreatmentInstance GetTriggeredMaintenance(
        int ielem,
        int iPeriod,
        Dictionary<string, double> numInputs,
        Dictionary<string, string> textInputs,
        Dictionary<string, double> numModParamValues,
        Dictionary<string, string> textModParamValues)
        => throw new NotSupportedException(NotAModelMessage);

    /// <summary>Not implemented - see the reference model. Present only to satisfy the base class.</summary>
    public override void Increment(
        int iElemIndex,
        int iPeriod,
        Dictionary<string, double> numInputs,
        Dictionary<string, string> textInputs,
        Dictionary<string, double> currentNumModParamValues,
        Dictionary<string, string> currentTextModParamValues,
        Action<string, double> numModParamValues,
        Action<string, string> textModParamValues)
        => throw new NotSupportedException(NotAModelMessage);

    /// <summary>Not implemented - see the reference model. Present only to satisfy the base class.</summary>
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
        => throw new NotSupportedException(NotAModelMessage);

    /// <summary>Not implemented - see the reference model. Present only to satisfy the base class.</summary>
    public override void DoEndOfPeriodCalculations(int iPeriod)
        => throw new NotSupportedException(NotAModelMessage);

    private const string NotAModelMessage =
        "ExamplesLibrary is a pattern library, not a runnable model. See reference-model\\DomainModelSample.";
}

/// <summary>
/// Holder for the sub-models built at setup. Every working Monte Carlo model has one of these -
/// a plain bag of fitted objects, hung off the domain model, built once and read every period.
///
/// <para>It is a separate class rather than a dozen properties on the model because the model's
/// own file should read as a switchboard. Everything here is assigned during
/// <see cref="PipeNetworkModel.SetupInstance"/> and never reassigned, which is what
/// <c>null!</c> is asserting.</para>
/// </summary>
public class PipeSubModels
{
    /// <summary>Condition grade against age, per material. Built from a <c>supporting\</c> CSV.</summary>
    public Dictionary<string, PieceWiseLinearModel> DeteriorationCurves { get; } = new();

    /// <summary>Cohort-based simulator for the annual increase in break rate.</summary>
    public DistributionSimulator BreakRateIncrementSimulator { get; set; } = null!;

    /// <summary>Cohort-based simulator for the break rate a segment is left with after relining.</summary>
    public DistributionSimulator BreakRateAfterRelineSimulator { get; set; } = null!;

    /// <summary>Probability that a metallic segment fails in the coming period.</summary>
    public LogisticModel FailureProbabilityMetallic { get; set; } = null!;

    /// <summary>Probability that a non-metallic segment fails in the coming period.</summary>
    public LogisticModel FailureProbabilityNonMetallic { get; set; } = null!;

    /// <summary>Maps a segment's condition-and-criticality rank to a replacement suitability score.</summary>
    public PieceWiseLinearModel ReplaceSuitabilityCurve { get; set; } = null!;

    /// <summary>Maps the same rank to a reline suitability score, shaped differently.</summary>
    public PieceWiseLinearModel RelineSuitabilityCurve { get; set; } = null!;
}
