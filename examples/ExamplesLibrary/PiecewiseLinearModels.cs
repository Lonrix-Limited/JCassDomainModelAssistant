using JCass_Core.Statistics;
using JCass_Data.Objects;

namespace ExamplesLibrary;

/// <summary>
/// PATTERN: piecewise-linear-models. Expressing a relationship as a curve a modeller can shape,
/// rather than as a formula only a developer can change.
///
/// <para>Documentation: <c>docs\patterns\piecewise-linear-models.md</c>. API:
/// <c>docs\framework\api\authoring\PieceWiseLinearModel.md</c>.</para>
///
/// <para><b>Why a curve rather than an equation.</b> Most relationships a modeller wants to
/// calibrate are not naturally a formula - they are "flat until here, then it climbs, then it
/// saturates". A piecewise-linear model says exactly that in a form the modeller can draw, and
/// its whole definition is a short string, which means it can live in <c>lookups.xlsx</c> or in a
/// <c>supporting\</c> CSV instead of in C#.</para>
///
/// <para><b>The setup-code string.</b> <c>"x,y|x,y|x,y"</c> - pairs separated by pipes, x and y
/// separated by a comma, x values ascending and unique. Whitespace around the parts is
/// tolerated.</para>
///
/// <para><b>The extrapolation flag is a modelling decision, not a default.</b> With
/// <c>canExtrapolate: false</c> an x outside the fitted range returns the nearest end value; with
/// <c>true</c> the end gradient continues. False is right for something fitted over a finite
/// range - beyond it you have no evidence. True is right when the curve is a scoring rule you
/// defined rather than fitted, and you want it to keep separating values at the extremes instead
/// of flattening every one of them onto the same score.</para>
/// </summary>
public static class PiecewiseLinearModels
{
    /// <summary>
    /// Ends of the 0-100 percentile rank scale the suitability curves are defined over.
    ///
    /// <para>Structural, not tunable. A modeller changing these would not recalibrate the model,
    /// they would break the correspondence with the rank the curve is fed. Named rather than
    /// inlined so the next reader can see that the choice was made deliberately - a magic literal
    /// is still bad practice, it just is not a lookup row.</para>
    /// </summary>
    private const double RankScaleMinimum = 0.0;
    private const double RankScaleMaximum = 100.0;

    /// <summary>Score awarded at the bottom of a curve, on the same 0-100 scale.</summary>
    private const double ScoreMinimum = 0.0;

    /// <summary>Score awarded at the top of a curve.</summary>
    private const double ScoreMaximum = 100.0;

    /// <summary>
    /// Builds the treatment-suitability curves at setup, from break points held in
    /// <c>lookups.xlsx</c>.
    ///
    /// <para><b>This is the shape to copy when the curve is a policy rather than a fit.</b> The
    /// break points come from <see cref="PipeConstants"/>, so a modeller reshapes the curve on the
    /// Tuning page and re-runs. Only the scale endpoints are in C#, because they are structural.
    /// Assembling the setup string here rather than storing the whole string in a lookup row is
    /// deliberate: it means each break point is its own named, individually editable value rather
    /// than a punctuation-sensitive blob a modeller has to retype correctly.</para>
    /// </summary>
    /// <param name="subModels">Holder to assign the curves to.</param>
    /// <param name="constants">Thresholds read from <c>lookups.xlsx</c>.</param>
    public static void BuildSuitabilityCurves(PipeSubModels subModels, PipeConstants constants)
    {
        // Replacement: nothing below the threshold rank scores at all, everything above it climbs
        // linearly to the top of the scale.
        string replaceSetup =
            $"{constants.ReplaceConditionGreaterThan},{ScoreMinimum}|{RankScaleMaximum},{ScoreMaximum}";

        // canExtrapolate: true, on purpose. Ranks below the first break point would otherwise all
        // return the same score, and a scoring curve that returns identical values for a whole
        // band of elements hands the optimiser ties it has to break arbitrarily.
        subModels.ReplaceSuitabilityCurve = new PieceWiseLinearModel(replaceSetup, canExtrapolate: true);

        // Relining: peaks in the middle of the range. A segment too good does not need it, and a
        // segment too far gone cannot be helped by it.
        string relineSetup =
            $"{constants.RelineConditionGreaterThan},{ScoreMinimum}" +
            $"|{constants.RelineConditionAtMost},{ScoreMaximum}" +
            $"|{RankScaleMaximum},{ScoreMinimum}";

        subModels.RelineSuitabilityCurve = new PieceWiseLinearModel(relineSetup, canExtrapolate: false);
    }

    /// <summary>
    /// Builds a curve from a <c>supporting\</c> CSV holding one x/y pair per row.
    ///
    /// <para>This is the third constructor overload - explicit x and y lists - and it is the one
    /// to use when the curve came out of a fit with more points than anybody wants to read as a
    /// setup string. The two forms produce the same object; the difference is only where the
    /// numbers are readable.</para>
    ///
    /// <para><b>The rows must be in ascending x order and x must be unique.</b> The constructor
    /// rejects both violations, which is worth knowing because a CSV sorted by something else -
    /// or exported with a duplicate boundary point - looks perfectly reasonable in Excel.</para>
    /// </summary>
    /// <param name="setupData">The CSV, already read.</param>
    /// <param name="fileName">Name of the file, for failure messages.</param>
    /// <param name="xColumn">Column holding the x values.</param>
    /// <param name="yColumn">Column holding the y values.</param>
    /// <param name="canExtrapolate">See the note on extrapolation in this class's summary.</param>
    public static PieceWiseLinearModel BuildFromDataSet(
        jcDataSet setupData,
        string fileName,
        string xColumn,
        string yColumn,
        bool canExtrapolate)
    {
        List<double> xValues = new List<double>();
        List<double> yValues = new List<double>();

        for (int iRow = 0; iRow < setupData.Count; iRow++)
        {
            Dictionary<string, object> row = setupData.Row(iRow);
            xValues.Add(SetupDataFromSupportingCsv.GetNumber(row, xColumn, fileName, iRow));
            yValues.Add(SetupDataFromSupportingCsv.GetNumber(row, yColumn, fileName, iRow));
        }

        if (xValues.Count < 2)
        {
            // One point is not a curve. The constructor would object, but with a message about
            // lists rather than about the file the modeller uploaded.
            throw new Exception($"'{fileName}' needs at least two rows to define a curve.");
        }

        return new PieceWiseLinearModel(xValues, yValues, canExtrapolate);
    }

    /// <summary>
    /// Reads a value off a curve.
    ///
    /// <para>There is nothing more to it than this - which is the point. All the calibration is
    /// in the curve, so the call site stays readable and the modeller owns the shape.</para>
    /// </summary>
    /// <param name="curve">A curve built at setup.</param>
    /// <param name="rank">
    /// Where this element sits on the <see cref="RankScaleMinimum"/> to
    /// <see cref="RankScaleMaximum"/> scale.
    /// </param>
    public static double GetScore(PieceWiseLinearModel curve, double rank) => curve.GetValue(rank);
}
