<!-- ------------------------------------------------------------------
     GENERATED FILE - DO NOT EDIT BY HAND.

     Generated from the framework reference assemblies and their XML
     documentation in refs\, by:

       cassandra_main\scripts\assistant\generate-api-reference.ps1

     The sync is ONE-WAY. Any edit made here is lost the next time that
     script runs, without warning and without a merge conflict. To change
     what this page says, change the /// documentation comments in the
     framework source, or the scoped surface in
     cassandra_main\scripts\assistant\api-surface.json, and regenerate.
     ------------------------------------------------------------------ -->

# Budget

**Namespace:** `JCass_ModelCore.ModelObjects`  
**Assembly:** `JCass_ModelCore`  
**Kind:** class

> **Should a domain model use this?**  
> You read it — typically to ask whether a candidate can be afforded. The framework does the spending.

The money available to the run: an amount per budget category per period, reduced as treatments are funded.

**Remarks.** Loaded from the budget sheet named by `ModelConfiguration.BudgetTagName`. Each column of that sheet other than `period` and `colour` becomes a budget category, so the sheet's columns define what categories exist.

A domain model reads this - usually to ask whether a candidate could be afforded - and never writes to it. The framework subtracts costs as it funds treatments.

Balances are live, not planned. Every accessor returns what is left in that period right now, part-way through the framework's funding pass, not the period's original allocation. The same call earlier or later in a period gives a different answer.

## Constructors

### Budget

```csharp
public Budget(ModelBase model)
```

Creates the budget for a run. Called by the framework; a domain model reads `model.Budget` rather than constructing one.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | The framework model. |

## Properties

### BudgetCategories

```csharp
public List<string> BudgetCategories { get; }
```

The budget categories this run can fund - one per column of the budget sheet, excluding `period` and `colour`.

**Remarks.** This list is the authority on what categories exist. A treatment cost directed at a category not in here cannot be funded, and the framework will say so - see `JCass_ModelCore.ModelObjects.Budget.CanApplyTreatment(JCass_ModelCore.Treatments.TreatmentInstance)`.

### IsMonolithicBudget

```csharp
public bool IsMonolithicBudget { get; }
```

Is this a monolithic budget? We presume that is always the case if there is only one budget category

### MonolithicBudgetCategoryName

```csharp
public string MonolithicBudgetCategoryName { get; }
```

Name of the Monolithic Budget Category. If this is not a monolithic budget, an error will be thrown if this property is used.

## Methods

### CanApplyTreatment

```csharp
public bool CanApplyTreatment(TreatmentInstance treatment)
```

Checks if a treatment can be applied in the current budget period. If the treatment relates to multiple budget categories, it checks that the total cost for each category does not exceed the budget for that category in the treatment period. If the cost in ANY budget category exceeds the budget for that category, the treatment cannot be applied. Note that default situation will be that the treatment only relates to a single budget category (default as specified in model setup), so in that default case this method will return true if the treatment cost is less than or equal to the budget for that category in the treatment period.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `treatment` | `TreatmentInstance` | The treatment to test. Its cost must already have been calculated. |

**Returns.** True if every category the cost falls into has enough left in that period.

**Throws.**

- `System.Exception` — Thrown if the budget has no allocation for the treatment's period.
- `System.Collections.Generic.KeyNotFoundException` — Thrown if the cost is directed at a budget category that is not a column of the budget sheet.

**Remarks.** A category with no budget column fails here, and it fails badly rather than quietly. The framework checks each treatment type's own budget category at setup and reports it as a setup error by name. It cannot check a category supplied at runtime by `TreatmentInstance.AssignBudgetCategoryFractions`, so that one surfaces mid-run as a bare `KeyNotFoundException` naming nothing useful. If a run dies that way, compare the category names your fractions use against `JCass_ModelCore.ModelObjects.Budget.BudgetCategories`.

The cost must already have been calculated, or the test is made against a cost of zero and passes trivially.

### CanApplyTreatmentStrategy

```csharp
public bool CanApplyTreatmentStrategy(TreatmentStrategy strategy)
```

Whether a treatment strategy can be afforded, judged on its first treatment alone.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `strategy` | `TreatmentStrategy` | The strategy to test. |

**Returns.** True if the first treatment fits the budget for its period.

**Remarks.** Only the first treatment is checked. Later treatments in the strategy are not tested against future budgets - since November 2025 follow-ups are expected to come back through the domain model's own triggers, where they are budgeted like anything else. A strategy can therefore start and then not be completed as planned.

### CheckForNegativeBudgets

```csharp
public void CheckForNegativeBudgets(ModelBase model)
```

Logs a warning for any period and category whose balance has gone negative.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | The framework model, used for logging. |

**Remarks.** A warning, not a failure - the run continues with the overspend in it. It only inspects categories that some treatment type points at, so a category overspent solely through `AssignBudgetCategoryFractions` is not reported here.

### GetBudgetBalance

```csharp
public double GetBudgetBalance(int period, string budgetCategory)
```

How much is left in one budget category in one period, right now.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `period` | `int` | Modelling period. |
| 2 | `budgetCategory` | `string` | Budget category name, as it appears in `JCass_ModelCore.ModelObjects.Budget.BudgetCategories`. |

**Returns.** The remaining amount. Can be negative.

**Throws.**

- `System.Exception` — Thrown, naming the period or the category, if either is not in the budget.

### GetBudgetBalances

```csharp
public Dictionary<string, double> GetBudgetBalances(int period)
```

How much is left in every budget category in one period, right now.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `period` | `int` | Modelling period. |

**Returns.** A copy: category name to remaining amount. Changing it does not change the budget.

**Throws.**

- `System.Exception` — Thrown, naming the period, if it is not in the budget.

### GetBudgetForPeriod

```csharp
public Dictionary<string, double> GetBudgetForPeriod(int iPeriod)
```

The live balances for one period, by reference.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iPeriod` | `int` | Modelling period. |

**Returns.** The budget's own dictionary for that period - changing it changes the budget.

**Throws.**

- `System.Collections.Generic.KeyNotFoundException` — Thrown if the period is not in the budget.

**Remarks.** Use `JCass_ModelCore.ModelObjects.Budget.GetBudgetBalances(System.Int32)` instead unless you specifically need the live object. That one returns a copy and cannot corrupt the run by accident.

### GetMaximumBudgetPeriod

```csharp
public int GetMaximumBudgetPeriod()
```

The highest period the budget sheet defines an allocation for.

**Returns.** The last budgeted period.

**Throws.**

- `System.InvalidOperationException` — Thrown if no budget data has been loaded.

### Setup

```csharp
public void Setup(jcDataSet setupData)
```

Loads the per-period allocations from the budget sheet. Called by the framework during setup.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `setupData` | `jcDataSet` | The budget sheet. Must have a `period` column; every other column except `colour` becomes a budget category. |

**Throws.**

- `System.ArgumentException` — Thrown if the sheet lists the same period twice.

### SubtractTreatmentCost

```csharp
public void SubtractTreatmentCost(TreatmentInstance treatment)
```

Deducts a funded treatment's cost from the budget, split across categories as the treatment specifies. Called by the framework when a treatment is committed.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `treatment` | `TreatmentInstance` | The treatment being funded. |

**Throws.**

- `System.Collections.Generic.KeyNotFoundException` — Thrown if the treatment's period or a budget category is not in the budget.

**Remarks.** This does not re-check affordability and will drive a balance negative if called without `JCass_ModelCore.ModelObjects.Budget.CanApplyTreatment(JCass_ModelCore.Treatments.TreatmentInstance)` first. `JCass_ModelCore.ModelObjects.Budget.CheckForNegativeBudgets(JCass_ModelCore.Models.ModelBase)` reports that afterwards, as a log warning rather than a failure.
