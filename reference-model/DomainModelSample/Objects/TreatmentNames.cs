namespace DomainModelSample.Objects;

/// <summary>
/// The treatment names this model knows about. These strings are a contract with two files
/// outside the C# project and must match both exactly:
///
/// <list type="bullet">
///   <item><description>the <c>treatment_name</c> column of the <c>treatments</c> sheet in
///   <c>domain_model_setup.xlsx</c>, and</description></item>
///   <item><description>the budget category columns in the client's
///   <c>inputs\budgets.xlsx</c> — indirectly, via the <c>budget_category</c> column of that
///   same <c>treatments</c> sheet.</description></item>
/// </list>
///
/// <para>A mismatch here is the single most common setup failure, and it surfaces late — as a
/// "treatment not recognised" exception partway through a run, or as treatments that never get
/// funded because their budget category does not exist. Keeping the strings in one place is
/// cheap insurance.</para>
/// </summary>
public static class TreatmentNames
{
    /// <summary>Partial intervention: halves the remaining defect, resets age.</summary>
    public const string Repair = "repair";

    /// <summary>Full intervention: element is effectively new again.</summary>
    public const string Replace = "replace";

    /// <summary>
    /// Routine maintenance. Applied outside the optimiser (see
    /// <see cref="DomainModelSample.GetTriggeredMaintenance"/>) — it is not a candidate that
    /// competes for budget, it is work that simply happens.
    /// </summary>
    public const string RoutineMaintenance = "RMaint";
}
