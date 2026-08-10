# Multi-budget cost split

**Compiling example:** [`MultiBudgetCostSplit.cs`](../../examples/ExamplesLibrary/MultiBudgetCostSplit.cs)
**API:** [`../framework/api/authoring/TreatmentInstance.md`](../framework/api/authoring/TreatmentInstance.md) → `AssignBudgetCategoryFractions`
**Also:** [`../framework/api/authoring/Budget.md`](../framework/api/authoring/Budget.md) → `BudgetCategories`

---

## When to reach for it

**Almost never — and that is the point of this page.**

`AssignBudgetCategoryFractions` appears in **three places in the entire corpus** of working models.
It is rare, non-obvious and high-consequence, which is exactly the combination an AI assistant has
no chance of inferring and every chance of getting subtly wrong.

Leave the fractions alone and the whole cost goes to the treatment type's own budget category, which
is what the bundle's `budget_category` column already says. That is correct for the overwhelming
majority of treatments.

You need this when **one physical job draws on two funding pots**. In the example: a relining that
includes localised structural repairs before the liner goes in — the repairs come out of the repairs
budget and the lining out of the renewals budget, and it is one job that either happens or does not.

Splitting it into two separate candidates would be wrong, because the optimiser could then fund one
without the other.

---

## The idiom, and why it is shaped that way

Three steps. **The middle one looks like a hack and is not**, and the obvious simplification breaks
the costing silently.

### Step 1 — cost each component separately, at its own rate

```csharp
double liningQuantity = segment.LengthMetres;
double repairQuantity = segment.LengthMetres * constants.RepairExtentFraction;

double liningCost = liningQuantity * constants.GetUnitRate(TreatmentNames.Reline);
double repairCost = repairQuantity * constants.GetUnitRate(TreatmentNames.PatchRepair);

double totalCost = liningCost + repairCost;
```

The two components have **different quantities and different unit rates**. There is no single
(quantity, rate) pair that describes the job — which is precisely why the ordinary construction in
[`treatment-instances.md`](treatment-instances.md) does not work here.

Both rates come from `lookups.xlsx`, as always.

### Step 2 — the synthetic quantity and unit rate

```csharp
double syntheticQuantity = totalCost;
const double syntheticUnitRate = 1.0;   // structural: makes the product equal the total

TreatmentInstance treatment = new TreatmentInstance(
    segment.ElementIndex,
    TreatmentNames.RelineWithRepairs,
    period,
    quantity: syntheticQuantity,
    unitRate: syntheticUnitRate,
    force: false,
    reason: ...,
    comment: $"Lining {Math.Round(liningCost, 0)} + repairs {Math.Round(repairCost, 0)}");
```

**The framework calculates `Cost` as `Quantity × UnitRate`, adjusted by the present-worth factor for
the period. It offers no way to say "the cost is this number".**

So the instance is given a **quantity equal to the total cost** and a **unit rate of exactly 1**. The
multiplication then reproduces the real total, and everything downstream — discounting, budget
deduction, export, benefit-cost scoring — works on the right figure.

The `1.0` is a **structural** constant, not a tunable one. Changing it would break the arithmetic
this pattern depends on, not recalibrate anything. It belongs in C#, named, with the comment.

> **Why not keep the real quantity and derive a rate from it?**
>
> `quantity = length` with `unitRate = totalCost / length` also produces the right total, and it is
> tempting because the quantity stays meaningful.
>
> It is worse in practice: the **exported unit rate then varies element by element** and no longer
> matches anything in `lookups.xlsx`. A modeller reconciling rates finds a column of numbers nobody
> recognises and no way to tell a composite treatment from a mis-set rate. With the shape above the
> exported quantity is obviously a currency amount and the rate is obviously a placeholder, so
> neither invites a false reconciliation.

### If the composite treatment has a `unit_rates` row, read it and assert it

The literal `1.0` above is the simplest correct form, and it is right **when the composite treatment
has no row in `lkp_unit_rates`.**

Very often it does have one — every other treatment does, so a modeller adding this one puts a rate
beside it without thinking about it. And **every row in that sheet is editable on the Tuning page's
Treatment Rates tab.** A modeller who escalates that rate by 10%, exactly as they would for any
other treatment, gets nothing: the literal ignores the row entirely. Worse, if the code *does* read
it, the whole composite cost is silently rescaled while the fractions stay correct — a wrong total
with no symptom anywhere.

So where the row exists, read it and **pin it**:

```csharp
double unitRate = constants.GetUnitRate(TreatmentNames.RelineWithRepairs);

if (unitRate != SyntheticUnitRate)
{
    throw new Exception(
        $"The unit rate for '{TreatmentNames.RelineWithRepairs}' in lookups.xlsx is {unitRate}, " +
        $"and it must be {SyntheticUnitRate}. This treatment's cost is built from its components " +
        "and split across budget categories, so its quantity is already the total cost. Any other " +
        "rate would silently rescale it. To change what this treatment costs, change the reline " +
        "and patch_repair rates instead.");
}
```

**That message is the point of the guard**, not the comparison. It tells the modeller why the row is
inert, and — the part that actually helps — which rows to edit instead.

This is the same principle as the guard idiom everywhere else in this library, applied to a value
that is *structurally required to be a particular number*: a lookup row that silently does nothing is
worse than no row, so assert it rather than ignoring it.

> Two of the three working call sites use the bare literal. The third reads the rate and asserts it
> is `1.0`, which is the stronger shape and the one to prefer wherever the row exists.

**Do not simplify this away.** Dropping the synthetic pair and passing the real length with the
reline rate loses the repair cost entirely: the treatment is funded for less than it costs, the
repairs budget is never drawn on, and the run completes with no complaint.

### Step 3 — only now, the fractions

```csharp
Dictionary<string, decimal> fractions = new Dictionary<string, decimal>
{
    { RenewalsBudget, Convert.ToDecimal(liningCost / totalCost) },
    { RepairsBudget,  Convert.ToDecimal(repairCost / totalCost) },
};

treatment.AssignBudgetCategoryFractions(fractions);
```

Three mechanics:

- **`decimal`, not `double`.** `Dictionary<string, decimal>` is the parameter type, and this is the
  compile error most people meet first here. `Convert.ToDecimal` on each fraction.
- **They must sum to 1**, checked to six decimal places, and an exception if not. Deriving each as
  `component / total` guarantees it.
- **Order does not matter**; the dictionary is keyed by category name.

---

## The failure that has no diagnostic in it

**This is the one place a budget category name is not validated at setup.**

The framework's `ModelSetupChecker` validates every *treatment type's own* budget category against
the columns of the client's `budgets.xlsx`, before the run starts, and reports a mismatch **by
name**. That check is thorough and it is why a mistyped `budget_category` in the bundle is a
non-event.

It cannot check a category that **only exists once your code has run**. A name supplied here with no
matching budget column kills the run part way through with a bare `KeyNotFoundException` naming
nothing at all — no category, no treatment, no element.

So check it yourself:

```csharp
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
```

**Worth the cost.** It runs only on treatments that actually split a cost, which is a small
minority, and the alternative is a failure mode with no diagnostic in it whatsoever.

Note the message names the available categories. The engineer is looking at a spreadsheet with those
columns in it, and the mismatch is usually a case difference or a hyphen.

> **A per-client override can change the category names under you.** A client can supply
> `inputs\budget_categories.xlsx` to reassign which budget category a treatment type charges to.
> That does not change the names you pass here, but it does mean the set in `budgets.xlsx` is the
> authority rather than anything in the bundle — which is another reason to read
> `Budget.BudgetCategories` rather than hard-coding a list you believe in.

---

## One more guard worth having

```csharp
if (totalCost <= 0)
{
    throw new Exception(
        $"Reline-with-repairs on element {segment.ElementIndex} costs nothing. " +
        "Check the reline and patch_repair unit rates in lookups.xlsx.");
}
```

A zero total makes the fractions a division by zero. The resulting `NaN` fractions then fail the
sum-to-1 check, so the run does stop — but with a message about fractions rather than about a
missing rate, which sends the engineer to the wrong file.

---

## Related

- [`treatment-instances.md`](treatment-instances.md) — the ordinary construction this departs from, and why
- [`constants-from-lookups.md`](constants-from-lookups.md) — where both component rates come from
- [`../framework/api/authoring/Budget.md`](../framework/api/authoring/Budget.md) — `BudgetCategories`, `GetBudgetBalance`, `CanApplyTreatment`
- [`../conventions/silent-failures.md`](../conventions/silent-failures.md) — the other failures with no diagnostic
