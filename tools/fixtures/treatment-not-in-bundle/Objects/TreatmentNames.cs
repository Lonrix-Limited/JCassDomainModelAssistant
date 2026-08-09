namespace FixtureModel.Objects;

/// <summary>
/// The treatment names this model knows about. These strings are a contract with two files
/// outside the C# project and must match both exactly:
///
/// <list type="bullet">
///   <item><description>the <c>treatment_name</c> column of the <c>treatments</c> sheet in
///   <c>domain_model_setup.xlsx</c>, and</description></item>
///   <item><description>the budget category columns in the client's <c>inputs\budgets.xlsx</c> -
///   indirectly, via the <c>budget_category</c> column of that same <c>treatments</c>
///   sheet.</description></item>
/// </list>
///
/// <para>A mismatch surfaces late and quietly. A name that does not match the bundle throws
/// partway through a run; a <c>budget_category</c> with no column in <c>budgets.xlsx</c> throws
/// nothing at all - the treatment is simply never funded, in any period, and the forecast looks
/// like a modelling result. Keeping the strings in one place is cheap insurance against the
/// first, and <c>jcass-dm check</c> catches the second.</para>
///
/// <para><b>These three came with the walking skeleton.</b> Replace them with your own network's
/// treatments. Each one is a constant here, a bundle row, a trigger in
/// <see cref="TreatmentsTrigger"/>, an arm in <see cref="Resetter"/>, and a rate in
/// <c>lookups.xlsx</c> - five places, and <c>jcass-dm check</c> knows about four of them.</para>
/// </summary>
public static class TreatmentNames
{
    /// <summary>Partial intervention: improves condition by a factor, resets age.</summary>
    public const string Repair = "repair";

    /// <summary>Full intervention: the element is effectively new again.</summary>
    public const string Replace = "replace";

    /// <summary>
    /// Routine maintenance. Applied outside the optimiser (see
    /// <see cref="RoutineMaintenance"/>) - it is not a candidate that competes for capital
    /// budget, it is work that simply happens.
    /// </summary>
    public const string RoutineMaintenance = "RMaint";

    /// <summary>Deliberately has no row on the treatments sheet - see tools/fixtures/README.md.</summary>
    public const string Reseal = "reseal";
}
