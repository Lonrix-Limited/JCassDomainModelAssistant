using ExamplesLibrary.Shared;
using JCass_ModelCore.Models;
using JCass_ModelCore.Treatments;

namespace ExamplesLibrary;

/// <summary>
/// PATTERN: multi-budget-cost-split. One treatment whose cost is charged to more than one budget
/// category.
///
/// <para>Documentation: <c>docs\patterns\multi-budget-cost-split.md</c> - <b>read it before
/// copying this file.</b> API:
/// <c>docs\framework\api\authoring\TreatmentInstance.md#assignbudgetcategoryfractions</c>. Where
/// the rates come from: <c>docs\patterns\constants-from-lookups.md</c>.</para>
///
/// <para><b>Most treatments never need this.</b> Leave the fractions alone and the whole cost
/// goes to the treatment type's own budget category, which is what a bundle row already says. You
/// need this only when one physical job draws on two funding pots - here, a relining that
/// includes structural repairs before the liner goes in, where the repairs come out of the
/// repairs budget and the lining out of the renewals budget.</para>
///
/// <para><b>The idiom below looks like a hack and is not.</b> It is worth understanding rather
/// than tidying, because the obvious simplification breaks the costing. The reasoning is spelled
/// out at <see cref="BuildRelineWithRepairs"/> and in the pattern page.</para>
/// </summary>
public static class MultiBudgetCostSplit
{
    /// <summary>
    /// Budget category names. These must match column headings in the client's
    /// <c>inputs\budgets.xlsx</c>.
    ///
    /// <para><b>This is the one place a budget category name is NOT validated at setup.</b> The
    /// framework checks each treatment type's own category against the budget sheet and reports a
    /// mismatch by name before the run starts. It cannot check a category that only exists once
    /// your code has run - so a name supplied here with no matching budget column kills the run
    /// mid-way with a bare <see cref="KeyNotFoundException"/> naming nothing at all.</para>
    ///
    /// <para>Which is why <see cref="AssertCategoriesExist"/> below is not optional decoration.</para>
    /// </summary>
    private const string RenewalsBudget = "Renewals";

    /// <inheritdoc cref="RenewalsBudget"/>
    private const string RepairsBudget = "Repairs";

    /// <summary>
    /// The unit rate a composite treatment must carry, so that <c>Quantity x UnitRate</c>
    /// reproduces the component total this class computes.
    ///
    /// <para><b>Structural, not tunable.</b> Changing it would break the arithmetic this whole
    /// pattern rests on rather than recalibrate anything, so it is a named constant in C# and not
    /// a lookup row. It is also what <see cref="AssertUnitRateIsSynthetic"/> pins the spreadsheet
    /// against.</para>
    /// </summary>
    private const double SyntheticUnitRate = 1.0;

    /// <summary>
    /// Builds a relining treatment whose cost is split between the renewals and repairs budgets.
    ///
    /// <para><b>Step 1 - cost each component separately, at its own rate.</b> The two components
    /// have different quantities and different unit rates. There is no single (quantity, rate)
    /// pair that describes the job, which is exactly why the ordinary construction does not
    /// work here.</para>
    ///
    /// <para><b>Step 2 - the synthetic quantity and unit rate.</b> The framework calculates
    /// <c>Cost</c> as <c>Quantity x UnitRate</c>, adjusted for discounting and inflation. It
    /// offers no way to say "the cost is this number". So the instance is given a
    /// <i>quantity equal to the total cost</i> and a <i>unit rate of exactly 1</i>. The
    /// multiplication then reproduces the real total, and every downstream calculation -
    /// discounting, budget deduction, export - works on the right figure.</para>
    ///
    /// <para><b>Why not keep the real quantity and derive a rate from it?</b> Setting
    /// <c>quantity = length</c> and <c>unitRate = totalCost / length</c> also produces the right
    /// total, and it is tempting because the quantity stays meaningful. It is worse in practice:
    /// the exported unit rate then varies element by element and no longer matches anything in
    /// <c>lookups.xlsx</c>, so a modeller reconciling rates finds a column of numbers nobody
    /// recognises. With the shape below the exported quantity is obviously a currency amount and
    /// the rate is obviously a placeholder, and neither invites a false reconciliation. Both
    /// working models that use this pattern chose the shape below.</para>
    ///
    /// <para><b>Step 3 - only now, the fractions.</b> They are the components' share of the
    /// total, they must sum to 1 (checked to six decimal places, and an exception if not), and
    /// they are <see cref="decimal"/> rather than <see cref="double"/> - which is not
    /// interchangeable and is the compile error most people meet first here.</para>
    ///
    /// <para><b>Do not simplify this away.</b> Dropping the synthetic pair and passing the real
    /// length with the reline rate loses the repair cost entirely: the treatment is funded for
    /// less than it costs, the repairs budget is never drawn on, and the run completes.</para>
    /// </summary>
    /// <param name="segment">The segment being relined.</param>
    /// <param name="constants">Rates read from <c>lookups.xlsx</c>.</param>
    /// <param name="frameworkModel">The framework model, for the budget's category names.</param>
    /// <param name="period">Modelling period (1-based).</param>
    /// <param name="suitabilityScore">Score from the suitability curve. See <c>TreatmentSuitabilityScoring</c>.</param>
    public static TreatmentInstance BuildRelineWithRepairs(
        PipeSegment segment,
        PipeConstants constants,
        ModelBase frameworkModel,
        int period,
        double suitabilityScore)
    {
        // ---- Step 1: cost each component at its own quantity and its own rate ----------------

        double liningQuantity = segment.LengthMetres;
        double repairQuantity = segment.LengthMetres * constants.RepairExtentFraction;

        double liningCost = liningQuantity * constants.GetUnitRate(TreatmentNames.Reline);
        double repairCost = repairQuantity * constants.GetUnitRate(TreatmentNames.PatchRepair);

        double totalCost = liningCost + repairCost;

        if (totalCost <= 0)
        {
            // A zero total would make the fractions below a division by zero, and the resulting
            // NaN fractions fail the sum-to-1 check with a message about fractions rather than
            // about a missing rate. Catch it here, where the cause is visible.
            throw new Exception(
                $"Reline-with-repairs on element {segment.ElementIndex} costs nothing. " +
                "Check the reline and patch_repair unit rates in lookups.xlsx.");
        }

        // ---- Step 2: the synthetic pair, so Quantity x UnitRate reproduces the real total -----

        double syntheticQuantity = totalCost;

        // This treatment has a row in lkp_unit_rates like every other treatment, and every row in
        // that sheet is editable on the Tuning page. Read it and pin it, rather than writing the
        // literal and leaving an editable row that silently does nothing.
        AssertUnitRateIsSynthetic(constants);

        TreatmentInstance treatment = new TreatmentInstance(
            segment.ElementIndex,
            TreatmentNames.RelineWithRepairs,
            period,
            quantity: syntheticQuantity,
            unitRate: SyntheticUnitRate,
            force: false,
            reason: $"Condition {Math.Round(segment.ConditionGrade, 1)} in the relinable band",
            comment: $"Lining {Math.Round(liningCost, 0)} + repairs {Math.Round(repairCost, 0)}");

        // ---- Step 3: the fractions, as decimals, summing to 1 --------------------------------

        Dictionary<string, decimal> fractions = new Dictionary<string, decimal>
        {
            { RenewalsBudget, Convert.ToDecimal(liningCost / totalCost) },
            { RepairsBudget, Convert.ToDecimal(repairCost / totalCost) },
        };

        AssertCategoriesExist(fractions, frameworkModel);
        treatment.AssignBudgetCategoryFractions(fractions);

        treatment.TreatmentSuitabilityScore = suitabilityScore;
        return treatment;
    }

    /// <summary>
    /// Pins the composite treatment's lookup rate at <see cref="SyntheticUnitRate"/>.
    ///
    /// <para><b>Why a lookup row exists at all for a rate that is not tunable.</b> Every other
    /// treatment has one, so a modeller adding this treatment puts a rate beside it without
    /// thinking - and every row in <c>lkp_unit_rates</c> is editable on the Tuning page's Treatment
    /// Rates tab. Ignoring the row leaves a control that appears to work and does nothing; reading
    /// it without checking lets a routine 10% escalation silently rescale the whole composite cost
    /// while the fractions stay correct, which is a wrong total with no symptom.</para>
    ///
    /// <para><b>The message is the point, not the comparison.</b> It has to say why the row is
    /// inert and which rows to edit instead, or the modeller simply sets it back and tries again.</para>
    ///
    /// <para><b>Exact equality is deliberate here - do not replace it with a tolerance.</b> The
    /// value came from a spreadsheet cell a person typed, so it is either 1 or it is a number
    /// somebody chose; there is no accumulated floating-point error to absorb. A tolerance would
    /// let 1.0001 through and rescale every composite cost by 0.01%, which is exactly the silent
    /// wrongness this guard exists to stop.</para>
    /// </summary>
    /// <param name="constants">Rates read from <c>lookups.xlsx</c>.</param>
    public static void AssertUnitRateIsSynthetic(PipeConstants constants)
    {
        double unitRate = constants.GetUnitRate(TreatmentNames.RelineWithRepairs);

        if (unitRate != SyntheticUnitRate)
        {
            throw new Exception(
                $"The unit rate for '{TreatmentNames.RelineWithRepairs}' in lookups.xlsx is {unitRate}, " +
                $"and it must be {SyntheticUnitRate}. This treatment's cost is built from its " +
                "components and split across budget categories, so its quantity is already the total " +
                "cost and any other rate would silently rescale it. To change what this treatment " +
                $"costs, change the '{TreatmentNames.Reline}' and '{TreatmentNames.PatchRepair}' rates " +
                "instead.");
        }
    }

    /// <summary>
    /// Checks every category name against the budget before assigning fractions.
    ///
    /// <para>Without this, a mistyped or renamed category surfaces as a
    /// <see cref="KeyNotFoundException"/> from inside the framework's funding code, part way
    /// through a run, naming nothing. With it, the run fails at the first triggered treatment
    /// with the wrong name, the available names, and the file to fix.</para>
    ///
    /// <para><b>Worth doing even though it costs a check per treatment.</b> It runs only on
    /// treatments that actually split a cost, which is a small minority, and the alternative is
    /// a failure mode with no diagnostic in it whatsoever.</para>
    /// </summary>
    /// <param name="fractions">The fractions about to be assigned.</param>
    /// <param name="frameworkModel">The framework model, for <c>Budget.BudgetCategories</c>.</param>
    public static void AssertCategoriesExist(
        Dictionary<string, decimal> fractions,
        ModelBase frameworkModel)
    {
        List<string> known = frameworkModel.Budget.BudgetCategories;

        foreach (string category in fractions.Keys)
        {
            if (!known.Contains(category))
            {
                throw new Exception(
                    $"Budget category '{category}' has no column in the client's budgets.xlsx. " +
                    $"Categories in this run: {string.Join(", ", known)}.");
            }
        }
    }
}
