namespace ExamplesLibrary.Shared;

/// <summary>
/// Treatment names used across the examples. In a real model these strings are a contract with
/// the <c>treatments</c> sheet of <c>domain_model_setup.xlsx</c> and, through that sheet's
/// <c>budget_category</c> column, with the client's <c>inputs\budgets.xlsx</c>.
///
/// <para>Keeping them as constants in one file is the pattern, not decoration: a treatment name
/// is used in the trigger, in the resetter, as a lookup key for its unit rate, and as a bundle
/// row, and a typo in any one of those fails somewhere other than where it was typed.</para>
/// </summary>
public static class TreatmentNames
{
    /// <summary>Localised repair of a defective length of pipe.</summary>
    public const string PatchRepair = "patch_repair";

    /// <summary>A cured-in-place liner through the existing pipe. Cheaper than replacement.</summary>
    public const string Reline = "reline";

    /// <summary>Full replacement of the pipe segment.</summary>
    public const string Replace = "replace";

    /// <summary>
    /// A relining that includes localised structural repairs before the liner goes in. Its cost
    /// splits across two budget categories - see <c>MultiBudgetCostSplit</c>.
    /// </summary>
    public const string RelineWithRepairs = "reline_with_repairs";

    /// <summary>Routine maintenance: flushing and root cutting. Applied outside the optimiser.</summary>
    public const string Flush = "flush";
}
