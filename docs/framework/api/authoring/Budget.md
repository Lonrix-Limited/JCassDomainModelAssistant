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

*The framework carries no `<summary>` for this type. The signatures below come
from the assembly metadata and are authoritative; the description is not available.*

## Constructors

### Budget

```csharp
public Budget(ModelBase model)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | — |

## Properties

### BudgetCategories

```csharp
public List<string> BudgetCategories { get; }
```

*No framework documentation for this member.*

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
| 1 | `treatment` | `TreatmentInstance` | — |

### CanApplyTreatmentStrategy

```csharp
public bool CanApplyTreatmentStrategy(TreatmentStrategy strategy)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `strategy` | `TreatmentStrategy` | — |

### CheckForNegativeBudgets

```csharp
public void CheckForNegativeBudgets(ModelBase model)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | — |

### GetBudgetBalance

```csharp
public double GetBudgetBalance(int period, string budgetCategory)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `period` | `int` | — |
| 2 | `budgetCategory` | `string` | — |

### GetBudgetBalances

```csharp
public Dictionary<string, double> GetBudgetBalances(int period)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `period` | `int` | — |

### GetBudgetForPeriod

```csharp
public Dictionary<string, double> GetBudgetForPeriod(int iPeriod)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iPeriod` | `int` | — |

### GetMaximumBudgetPeriod

```csharp
public int GetMaximumBudgetPeriod()
```

*No framework documentation for this member.*

### Setup

```csharp
public void Setup(jcDataSet setupData)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `setupData` | `jcDataSet` | — |

### SubtractTreatmentCost

```csharp
public void SubtractTreatmentCost(TreatmentInstance treatment)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `treatment` | `TreatmentInstance` | — |
