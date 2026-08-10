using JCass_Core.Statistics;
using JCass_Data.Objects;
using JCass_Data.Utils;

namespace ExamplesLibrary;

/// <summary>
/// PATTERN: setup-data-from-supporting-csv. Loading a fitted set of coefficients from a CSV in
/// the client's <c>supporting\</c> folder, once, at setup.
///
/// <para>Documentation: <c>docs\patterns\setup-data-from-supporting-csv.md</c>. Whether a number
/// belongs here rather than in <c>lookups.xlsx</c> is decided by
/// <c>docs\conventions\where-numbers-live.md</c>, not by this file.</para>
///
/// <para><b>Three things make this the pattern rather than just "reading a file".</b></para>
///
/// <para>1. <b>The path is built from <c>WorkFolder</c>, and the folder is <c>supporting</c>.</b>
/// <c>model.Configuration.WorkFolder</c> is the client root, so
/// <c>Path.Combine(workFolder, "supporting/x.csv")</c> resolves to the same file under a normal
/// run and under an in-browser debug run. There is no bundle-folder property, and a path built
/// relative to the model's own bundle reads a different folder under F5.</para>
///
/// <para>2. <b>Guard, then read, and name the file in the message.</b> A missing or misnamed CSV
/// is the single most common setup failure, and it is one the modeller can fix themselves the
/// moment they are told which file was expected. Note the order: check existence <i>before</i>
/// reading, not after.</para>
///
/// <para>3. <b>It runs once.</b> Parsing a CSV and building a curve per row is not work to repeat
/// per element per period. Everything here is called from <c>SetupInstance</c> and the results
/// live on the model for the rest of the run.</para>
/// </summary>
public static class SetupDataFromSupportingCsv
{
    /// <summary>
    /// Name of the CSV in the client's <c>supporting\</c> folder. Given a name rather than
    /// inlined because it appears twice - in the path, and in the error message when the read
    /// fails.
    /// </summary>
    private const string DeteriorationCurvesFile = "pipe_deterioration_curves.csv";

    /// <summary>
    /// Loads one piecewise-linear deterioration curve per material from a <c>supporting\</c> CSV.
    ///
    /// <para>The file has one row per material and two columns that matter: <c>material</c>, and
    /// <c>curve_setup_code</c> holding the curve as an <c>x,y|x,y|...</c> string. Both the shape
    /// and the number of rows come from the file, so a refit that adds a material needs no code
    /// change at all - which is the entire point of putting it here rather than in C#.</para>
    /// </summary>
    /// <param name="subModels">Holder to assign the loaded curves to.</param>
    /// <param name="workFolder">
    /// <c>model.Configuration.WorkFolder</c> - the client root, not the bundle folder.
    /// </param>
    public static void LoadDeteriorationCurves(PipeSubModels subModels, string workFolder)
    {
        jcDataSet setupData = ReadSupportingCsv(workFolder, DeteriorationCurvesFile);

        for (int iRow = 0; iRow < setupData.Count; iRow++)
        {
            Dictionary<string, object> row = setupData.Row(iRow);

            string material = GetText(row, "material", DeteriorationCurvesFile, iRow);
            string curveCode = GetText(row, "curve_setup_code", DeteriorationCurvesFile, iRow);

            // false = do not extrapolate. Outside the fitted range the curve returns its end
            // value rather than continuing the last gradient, which is the safer default for a
            // relationship fitted over a finite age range.
            subModels.DeteriorationCurves[material] = new PieceWiseLinearModel(curveCode, false);
        }

        if (subModels.DeteriorationCurves.Count == 0)
        {
            // An empty file parses perfectly well and produces a model that cannot deteriorate
            // anything. Nothing downstream would report it, so it is checked here.
            throw new Exception(
                $"'{DeteriorationCurvesFile}' in the supporting folder has a header row but no data rows.");
        }
    }

    /// <summary>
    /// Resolves a file in the client's <c>supporting\</c> folder and reads it, or throws naming
    /// the file.
    ///
    /// <para><b>Reuse this rather than repeating the two lines.</b> Every working model that
    /// loads side-car data ends up with a helper of this shape, and the models that wrote the
    /// guard out by hand at each call site are the ones where one of them drifted - in one real
    /// case, reading the file <i>before</i> testing whether it exists, which turns a clear
    /// "file not found in supporting\" into whatever the CSV reader happens to throw.</para>
    /// </summary>
    /// <param name="workFolder">
    /// <c>model.Configuration.WorkFolder</c>. The client root, under which <c>supporting\</c>
    /// sits.
    /// </param>
    /// <param name="fileName">Name of the CSV, without any folder part.</param>
    /// <returns>The file's contents as a data set.</returns>
    public static jcDataSet ReadSupportingCsv(string workFolder, string fileName)
    {
        string filePath = Path.Combine(workFolder, "supporting", fileName);

        if (!File.Exists(filePath))
        {
            // Name the file, not the absolute path. The engineer uploads through the web app's
            // Files page and never sees the server's folder layout; the file name is the part
            // they can act on. It is also the part that is safe to put in a log.
            throw new Exception(
                $"Setup file '{fileName}' not found in the client's supporting folder. " +
                "Upload it on the Files page, under Inputs.");
        }

        return CSVHelper.ReadDataFromCsvFile(filePath);
    }

    /// <summary>
    /// Reads one text cell, failing with the file, the row and the column if it is missing.
    ///
    /// <para>The row number is worth the extra parameter: "column 'material' is empty on row 14
    /// of pipe_deterioration_curves.csv" is a message a modeller acts on in seconds, and
    /// "Object reference not set to an instance of an object" is not.</para>
    /// </summary>
    /// <param name="row">The row, as returned by <c>jcDataSet.Row</c>.</param>
    /// <param name="columnName">Column to read.</param>
    /// <param name="fileName">Name of the file, for the failure message.</param>
    /// <param name="iRow">Zero-based row index, for the failure message.</param>
    public static string GetText(Dictionary<string, object> row, string columnName, string fileName, int iRow)
    {
        if (!row.ContainsKey(columnName))
        {
            throw new Exception($"'{fileName}' has no column named '{columnName}'.");
        }

        string? value = row[columnName]?.ToString();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Exception($"Column '{columnName}' is empty on row {iRow + 1} of '{fileName}'.");
        }

        return value;
    }

    /// <summary>
    /// Reads one numeric cell, failing with the file, the row and the column if it is missing.
    ///
    /// <para>As with lookups, this converts rather than casts. A CSV cell arrives as text, so
    /// <c>(double)row[columnName]</c> throws an <see cref="InvalidCastException"/> that says
    /// nothing about which file or which row.</para>
    /// </summary>
    /// <param name="row">The row, as returned by <c>jcDataSet.Row</c>.</param>
    /// <param name="columnName">Column to read.</param>
    /// <param name="fileName">Name of the file, for the failure message.</param>
    /// <param name="iRow">Zero-based row index, for the failure message.</param>
    public static double GetNumber(Dictionary<string, object> row, string columnName, string fileName, int iRow)
    {
        string text = GetText(row, columnName, fileName, iRow);

        if (!double.TryParse(text, out double value))
        {
            throw new Exception(
                $"Column '{columnName}' on row {iRow + 1} of '{fileName}' is '{text}', which is not a number.");
        }

        return value;
    }
}
