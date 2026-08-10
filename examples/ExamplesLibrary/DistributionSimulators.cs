using ExamplesLibrary.Shared;
using JCass_Data.Objects;
using JCass_ModelCore.Models;
using JCass_ModelCore.MonteCarlo;

namespace ExamplesLibrary;

/// <summary>
/// PATTERN: distribution-simulators. Drawing a random value from a distribution chosen by which
/// cohort an element falls into - the workhorse of a Monte Carlo model's deterioration.
///
/// <para>Documentation: <c>docs\patterns\distribution-simulators.md</c>. API:
/// <c>docs\framework\api\authoring\DistributionSimulator.md</c>. Where the cohort file lives, and
/// why: <c>docs\patterns\setup-data-from-supporting-csv.md</c>.</para>
///
/// <para><b>What a simulator is.</b> The cohort file gives, per cohort, a label, a rule deciding
/// membership, and a piecewise description of the distribution's shape. The shape is the
/// distribution's <i>inverse</i>: a uniform draw between 0 and 1 goes in as x, and the curve maps
/// it to a value. So the curve is the quantile function of whatever was fitted, and it normally
/// arrives from R or Python as a CSV.</para>
///
/// <para><b>Cohort order is priority order.</b> Rules are evaluated top to bottom and the first
/// match wins, so a broad catch-all above a specific rule silently swallows every element the
/// specific rule was written for - and the run completes, with the wrong distribution. Order the
/// file most specific to most general, and say so in the file itself.</para>
/// </summary>
public static class DistributionSimulators
{
    private const string BreakRateIncrementFile = "cohorts_break_rate_increment.csv";
    private const string BreakRateAfterRelineFile = "cohorts_break_rate_after_reline.csv";

    /// <summary>
    /// Builds the simulators once, at setup, from CSVs in the client's <c>supporting\</c> folder.
    ///
    /// <para><b>Build these at setup and keep them.</b> Constructing one parses every cohort rule
    /// and builds a curve per cohort. Doing that per element per period is the difference between
    /// a run that takes a minute and one that takes an hour, and it produces identical
    /// numbers - so nothing tells you it is happening.</para>
    /// </summary>
    /// <param name="subModels">Holder to assign the simulators to.</param>
    /// <param name="workFolder">
    /// <c>model.Configuration.WorkFolder</c> - the client root, not the bundle folder.
    /// </param>
    public static void LoadBreakRateSimulators(PipeSubModels subModels, string workFolder)
    {
        subModels.BreakRateIncrementSimulator =
            BuildSimulator("break_rate_increment", workFolder, BreakRateIncrementFile);

        subModels.BreakRateAfterRelineSimulator =
            BuildSimulator("break_rate_after_reline", workFolder, BreakRateAfterRelineFile);
    }

    /// <summary>
    /// Reads one cohort file and builds a simulator from it.
    /// </summary>
    /// <param name="parameterName">
    /// Name of the parameter being simulated. The framework uses it only in error messages - so
    /// make it the name the modeller would recognise, not a variable name.
    /// </param>
    /// <param name="workFolder"><c>model.Configuration.WorkFolder</c>.</param>
    /// <param name="fileName">Cohort file in the client's <c>supporting\</c> folder.</param>
    private static DistributionSimulator BuildSimulator(string parameterName, string workFolder, string fileName)
    {
        // The guard lives in the shared helper, which checks the file exists BEFORE reading it.
        jcDataSet setupData = SetupDataFromSupportingCsv.ReadSupportingCsv(workFolder, fileName);

        // Required columns, checked here rather than at the first draw. A cohort file missing
        // cohort_rule constructs without complaint and then throws on the first element of the
        // first period, several minutes into a run.
        setupData.CheckRequiredColumns(
            new List<string> { "cohort_label", "cohort_rule", "cohort_shape" },
            throwErrorIfNotFound: true);

        return new DistributionSimulator(parameterName, setupData);
    }

    /// <summary>
    /// Draws this period's increase in break rate for one segment.
    ///
    /// <para><b>Pass the framework's random generator, never a new one.</b>
    /// <c>model.Random</c> - or <c>Rando</c> on <c>DomainModelBase</c>, which is the same
    /// object - is seeded from the model configuration, and that seeding is the whole of what
    /// makes a Monte Carlo run reproducible. A freshly constructed <see cref="Random"/> is seeded
    /// from the clock, so the model silently stops giving the same answer twice: no error, no
    /// warning, and results that cannot be defended.</para>
    ///
    /// <para><b>The dictionary must carry every column the cohort rules reference.</b> The rules
    /// are text in the CSV, so nothing checks this at compile time. A rule mentioning
    /// <c>diameter_mm</c> against a dictionary that does not have it throws, naming the
    /// parameter, at the first draw.</para>
    /// </summary>
    /// <param name="segment">The segment to draw for.</param>
    /// <param name="subModels">The simulators built at setup.</param>
    /// <param name="frameworkModel">The framework model, for its seeded random generator.</param>
    public static double GetBreakRateIncrement(
        PipeSegment segment,
        PipeSubModels subModels,
        ModelBase frameworkModel)
    {
        Dictionary<string, object> cohortInputs = new Dictionary<string, object>
        {
            { "material", segment.MaterialType },
            { "diameter_mm", segment.DiameterMm },
            { "age", segment.Age },
            { "cond_grade", segment.ConditionGrade },
            { "break_rate", segment.BreakRatePerKmYear },
        };

        return subModels.BreakRateIncrementSimulator.GetSimulatedValue(cohortInputs, frameworkModel.Random);
    }

    /// <summary>
    /// Draws the break rate a segment is left with after relining.
    ///
    /// <para>Reset values are simulated the same way increments are, from their own cohort file.
    /// That is deliberate: a treatment's effect varies as much as deterioration does, and a model
    /// that simulates the decay but resets to a fixed value understates the spread of outcomes
    /// while looking stochastic.</para>
    /// </summary>
    /// <param name="segment">The segment being relined.</param>
    /// <param name="subModels">The simulators built at setup.</param>
    /// <param name="frameworkModel">The framework model, for its seeded random generator.</param>
    public static double GetBreakRateAfterReline(
        PipeSegment segment,
        PipeSubModels subModels,
        ModelBase frameworkModel)
    {
        Dictionary<string, object> cohortInputs = new Dictionary<string, object>
        {
            { "material", segment.MaterialType },
            { "diameter_mm", segment.DiameterMm },
            { "break_rate_pre", segment.BreakRatePerKmYear },
        };

        return subModels.BreakRateAfterRelineSimulator.GetSimulatedValue(cohortInputs, frameworkModel.Random);
    }
}
