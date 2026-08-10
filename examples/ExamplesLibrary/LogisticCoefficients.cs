using ExamplesLibrary.Shared;
using JCass_Core.Statistics;
using JCass_Data.Objects;

namespace ExamplesLibrary;

/// <summary>
/// PATTERN: logistic-coefficients. A fitted logistic model - coefficients from a CSV, never from
/// C# - used to turn an element's attributes into a probability.
///
/// <para>Documentation: <c>docs\patterns\logistic-coefficients.md</c>. API:
/// <c>docs\framework\api\authoring\LogisticModel.md</c>. Where the CSV lives, and why:
/// <c>docs\patterns\setup-data-from-supporting-csv.md</c>.</para>
///
/// <para><b>Coefficients are the archetypal <c>supporting\</c> data.</b> They are produced by a
/// fit, they change as a whole set when the fit is redone, and they arrive from R or Python as a
/// file with a <c>term</c> column and an <c>estimate</c> column. Nobody hand-edits eleven of them
/// into <c>lookups.xlsx</c> after a refit; they do it wrongly, or they do not do it.</para>
///
/// <para><b>The term names are a contract with the prediction call, and nothing checks it.</b>
/// The dictionary handed to <c>PredictProbability</c> must use exactly the names in the CSV's
/// <c>term</c> column, including the intercept's own name and including any transform the fit
/// applied - if the regression was fitted on <c>log(pressure)</c> then the term is literally
/// <c>log(pressure)</c> and the value passed must be the log, not the pressure. Get either half
/// wrong and the model still returns a number.</para>
/// </summary>
public static class LogisticCoefficients
{
    private const string MetallicCoefficientsFile = "logistic_failure_metallic.csv";
    private const string NonMetallicCoefficientsFile = "logistic_failure_non_metallic.csv";

    /// <summary>
    /// Builds the failure-probability models once, at setup, from CSVs in the client's
    /// <c>supporting\</c> folder.
    ///
    /// <para>Two models rather than one because the fit was done separately per material family.
    /// That is a modelling decision, and it shows up here as two files and two objects rather
    /// than as a branch inside one - which keeps the choice visible to whoever refits them.</para>
    /// </summary>
    /// <param name="subModels">Holder to assign the models to.</param>
    /// <param name="workFolder">
    /// <c>model.Configuration.WorkFolder</c> - the client root, not the bundle folder.
    /// </param>
    public static void LoadFailureProbabilityModels(PipeSubModels subModels, string workFolder)
    {
        subModels.FailureProbabilityMetallic =
            BuildLogisticModel(workFolder, MetallicCoefficientsFile);

        subModels.FailureProbabilityNonMetallic =
            BuildLogisticModel(workFolder, NonMetallicCoefficientsFile);
    }

    /// <summary>
    /// Reads a two-column coefficients CSV and builds a logistic model from it.
    /// </summary>
    /// <param name="workFolder"><c>model.Configuration.WorkFolder</c>.</param>
    /// <param name="fileName">Coefficients file in the client's <c>supporting\</c> folder.</param>
    private static LogisticModel BuildLogisticModel(string workFolder, string fileName)
    {
        jcDataSet coefficientData = SetupDataFromSupportingCsv.ReadSupportingCsv(workFolder, fileName);

        Dictionary<string, double> coefficients = new Dictionary<string, double>();

        for (int iRow = 0; iRow < coefficientData.Count; iRow++)
        {
            Dictionary<string, object> row = coefficientData.Row(iRow);

            string term = SetupDataFromSupportingCsv.GetText(row, "term", fileName, iRow);
            double estimate = SetupDataFromSupportingCsv.GetNumber(row, "estimate", fileName, iRow);

            // Reject a duplicated term rather than letting the later row win. R and Python both
            // emit one row per term, so a duplicate means the file was concatenated or hand-
            // edited - and silently taking the last one produces a model nobody fitted.
            if (coefficients.ContainsKey(term))
            {
                throw new Exception($"Term '{term}' appears more than once in '{fileName}'.");
            }

            coefficients[term] = estimate;
        }

        if (coefficients.Count == 0)
        {
            throw new Exception($"'{fileName}' contains no coefficient rows.");
        }

        return new LogisticModel(coefficients);
    }

    /// <summary>
    /// Returns the probability that a segment fails in the coming period.
    ///
    /// <para><b>Every key here must match a <c>term</c> in the CSV.</b> The pairing is by name
    /// and it is checked at run time, not at compile time - so a renamed predictor in a refitted
    /// file fails on the first element of the first period rather than at build.</para>
    /// </summary>
    /// <param name="segment">The segment to predict for.</param>
    /// <param name="subModels">The models built at setup.</param>
    public static double GetFailureProbability(PipeSegment segment, PipeSubModels subModels)
    {
        Dictionary<string, double> predictors = new Dictionary<string, double>
        {
            { "age", segment.Age },
            { "cond_grade", segment.ConditionGrade },
            { "break_rate", segment.BreakRatePerKmYear },
            { "diameter_mm", segment.DiameterMm },
        };

        LogisticModel model = IsMetallic(segment.MaterialType)
            ? subModels.FailureProbabilityMetallic
            : subModels.FailureProbabilityNonMetallic;

        return model.PredictProbability(predictors);
    }

    /// <summary>
    /// Which family a material belongs to, for choosing between the two fitted models.
    ///
    /// <para>The material names are structural rather than tunable - they are the values the
    /// client's input column actually contains, and changing one here would not recalibrate the
    /// model, it would stop it recognising the data. So they stay in C#, named. A modeller who
    /// wants a different <i>threshold</i> edits <c>lookups.xlsx</c>; a modeller whose data uses
    /// a new material needs this list extended and a coefficient file to go with it.</para>
    /// </summary>
    /// <param name="materialType">Value of the segment's <c>material</c> input column.</param>
    private static bool IsMetallic(string materialType)
        => materialType is "cast_iron" or "ductile_iron" or "steel";
}
