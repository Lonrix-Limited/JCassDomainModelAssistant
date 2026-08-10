namespace ExamplesLibrary.Shared;

/// <summary>
/// The element class the examples work with: one length of buried water main.
///
/// <para><b>This is scaffolding for the examples, not a pattern in its own right.</b> The element
/// class and its factory are covered by the scaffolded project itself - see
/// <c>Objects\ModelElement.cs</c> and <c>Objects\ModelElementFactory.cs</c> in a project produced
/// by <c>jcass-dm scaffold</c>, and <c>docs\workflow\30-make-a-change.md</c> for changing one.
/// It is here so that each pattern file has something concrete to operate on.</para>
///
/// <para>Note what it does and does not hold. Attributes that never change - diameter, material,
/// length - sit alongside state the model advances every period: condition grade, age, break
/// rate. In a real factory the first group is read from the client's input columns in both
/// factory methods, and the second group is read from the input columns only in period 0 and
/// from the model's own parameters in every period after that.</para>
/// </summary>
public class PipeSegment
{
    /// <summary>Zero-based index of the element, as the framework passes it in.</summary>
    public int ElementIndex { get; init; }

    /// <summary>Identifier a modeller recognises. Used in trigger reasons.</summary>
    public string SegmentName { get; init; } = string.Empty;

    /// <summary>Pipe material. Keys the per-material rate sets in <c>lookups.xlsx</c>.</summary>
    public string MaterialType { get; init; } = string.Empty;

    /// <summary>Length of the segment in metres. The quantity most treatments are priced against.</summary>
    public double LengthMetres { get; init; }

    /// <summary>Nominal internal diameter in millimetres.</summary>
    public double DiameterMm { get; init; }

    /// <summary>Years since the segment was laid or last replaced. Evolving state.</summary>
    public int Age { get; set; }

    /// <summary>
    /// Condition grade, 1 (as new) to 5 (failed). Evolving state, and the thing the model
    /// forecasts.
    /// </summary>
    public double ConditionGrade { get; set; }

    /// <summary>Observed breaks per kilometre per year. Evolving state.</summary>
    public double BreakRatePerKmYear { get; set; }

    /// <summary>
    /// How much disruption a failure here would cause, 0 to 1. An input attribute, not state -
    /// it reflects what is above the pipe rather than the pipe's own condition.
    /// </summary>
    public double CriticalityScore { get; init; }
}
