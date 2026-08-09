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

# TreatmentInstance

**Namespace:** `JCass_ModelCore.Treatments`  
**Assembly:** `JCass_ModelCore`  
**Kind:** class

> **Should a domain model use this?**  
> **Yes — you construct these.** It is what a trigger returns.
>  
> This is the most error-prone type in the framework and the reason this reference exists. The constructor takes eight parameters and several are the same type, so passing them in the wrong order compiles cleanly and produces a wrong model. Use named arguments. `AssignBudgetCategoryFractions` is rare, non-obvious and high-consequence: it is how one treatment's cost is split across budget groups, and a cost charged to a budget category with no column in `budgets.xlsx` is silently never funded.

One treatment proposed for one element in one modelling period: what it is, how much of it, what it costs and why it was put forward.

**Remarks.** This is the currency between a domain model and the framework. A domain model creates instances of this class in its trigger methods and returns them as candidates; the framework then ranks them, funds what fits the budget, and passes the winners back to `Reset`.

Domain models construct these. Everything else here - costing, ranking, funding, exporting - is done by the framework, and the properties it fills in are marked as such below.

## Constructors

### TreatmentInstance

```csharp
public TreatmentInstance(
    int element_index,
    string name,
    int period,
    double quantity,
    double unitRate,
    bool force,
    string reason,
    string comment)
```

Creates a treatment candidate for one element in one period. This is what a domain model's trigger methods return.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `element_index` | `int` | Zero-based index of the element, as passed into the domain model method. |
| 2 | `name` | `string` | Treatment name. Must match a treatment type in the model setup - see `JCass_ModelCore.Treatments.TreatmentInstance.TreatmentName`. |
| 3 | `period` | `int` | Modelling period the treatment falls in. Must be 1 or greater. |
| 4 | `quantity` | `double` | How much of the treatment is applied, in the same unit as `unitRate`. |
| 5 | `unitRate` | `double` | Cost per unit of quantity. Read this from the model's lookups; do not hard-code it. |
| 6 | `force` | `bool` | True to place this treatment regardless of how it ranks - see `JCass_ModelCore.Treatments.TreatmentInstance.Force`. |
| 7 | `reason` | `string` | Why the treatment was triggered. Exported, and read by modellers. |
| 8 | `comment` | `string` | Free text carried with the treatment. No meaning to the framework. |

> Positional order matters and 8 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

**Throws.**

- `System.Exception` — Thrown if `period` is zero or negative.

**Remarks.** Call this with named arguments. Eight positional parameters, of which two are consecutive doubles and three are consecutive strings, means a call with the right values in the wrong order compiles cleanly and produces a wrong model rather than an error.

The unit rate is supplied per instance and is no longer inherited from the treatment type - see `JCass_ModelCore.Treatments.TreatmentInstance.UnitRate`. It belongs in the project's lookups, so that a modeller can recalibrate rates without a rebuild.

## Properties

### Cost

```csharp
public double Cost { get; }
```

Cost of the treatment, discounted and inflated to the period it falls in. Read-only.

**Remarks.** This is zero until the framework calculates it. A domain model supplies `JCass_ModelCore.Treatments.TreatmentInstance.Quantity` and the unit rate; the framework multiplies them by the present-worth factor for the period in `CalculateDiscountedAndInflatedCost` and rounds to two decimal places. Reading `Cost` straight after constructing an instance returns 0, not the treatment's cost.

### UnitRate

```csharp
public double UnitRate { get; }
```

Unit Rate inherited from the TreatmentType definition or optionally specified in the constructor.

## Fields

### BudgetCategoryFractions

```csharp
public IReadOnlyDictionary<string, decimal> BudgetCategoryFractions;
```

(Optional) Budget category fractions for cost allocation for this treatment. If this is empty, the treatment cost will be allocated to the budget category of the treatment type (default budget category for treatment type).

### Comment

```csharp
public string Comment;
```

Free text carried with the treatment and exported as `treatment_comment`. Unlike `JCass_ModelCore.Treatments.TreatmentInstance.Reason` the framework attaches no meaning to it.

### CustomPropertiesNumber

```csharp
public Dictionary<string, double> CustomPropertiesNumber;
```

Optional set of customisable Numeric properties that can be used by custom implementers to store additional information related to this treatment. These values are only held in memory during model execution and are not persisted.

### CustomPropertiesText

```csharp
public Dictionary<string, string> CustomPropertiesText;
```

Optional set of customisable Text/String properties that can be used by custom implementers to store additional information related to this treatment. These values are only held in memory during model execution and are not persisted.

### ElementIndex

```csharp
public int ElementIndex;
```

Zero-based index of the network element this treatment applies to. This is the same index the framework passes into every domain model method.

### FollowUpWaitPeriods

```csharp
public int FollowUpWaitPeriods;
```

For a follow-up treatment, the number of periods to wait after the strategy's first treatment before this one is placed. Set by the framework's strategy generator alongside `JCass_ModelCore.Treatments.TreatmentInstance.IsFollowUp`.

### Force

```csharp
public bool Force;
```

True if this treatment must be placed regardless of how it ranks economically.

**Remarks.** Forced treatments bypass the ranking rather than the budget. In an MCDA model a forced treatment is assigned the maximum rank parameter; in a BCA model forced strategies are separated out and funded ahead of the ranked ones. Use it for interventions that policy or safety requires, not to push a treatment the model would otherwise reject.

### IsCommitted

```csharp
public bool IsCommitted;
```

True if this treatment came from the model's committed-treatments setup data rather than from a domain model trigger. Set by the framework when it loads that data; a domain model does not assign it.

**Remarks.** Committed treatments are always loaded with `JCass_ModelCore.Treatments.TreatmentInstance.Force` set to true, because a treatment somebody has already committed to is not a candidate for the optimiser to reject.

### IsFollowUp

```csharp
public bool IsFollowUp;
```

True if this treatment is a follow-up within a multi-treatment strategy rather than the first treatment of one. Set by the framework's strategy generator - a domain model does not assign it.

### Quantity

```csharp
public double Quantity;
```

How much of the treatment is applied, in whatever unit the treatment type's unit rate is expressed in. Cost is quantity multiplied by unit rate, so the two must agree on units.

### RankParamSimple

```csharp
public double RankParamSimple;
```

Priority of this treatment among the routine-maintenance candidates competing for the maintenance budget. Higher wins. Set this from your domain model on treatments returned by `GetTriggeredMaintenance`.

**Remarks.** Maintenance is not optimised the way capital treatments are. The framework simply sorts every triggered maintenance treatment by this value, descending, and funds down the list until the budget runs out. So this value is the whole of your control over what gets done first when maintenance money is short.

Left at its default of 0 the ordering is arbitrary - every candidate compares equal, and which ones get funded is decided by the order elements happen to be processed in. That is stable enough to look deliberate and it is not. Set it whenever maintenance can be budget-constrained, using whatever expresses urgency in your domain: severity, a condition index, exposure, or cost-effectiveness.

It has no effect on capital treatment candidates, which are ranked by the optimiser instead.

### Reason

```csharp
public string Reason;
```

Short text saying why the treatment was triggered. Exported against every treatment as `treatment_reason`, so it is what a modeller reads when asking why the model did something. Worth writing for that reader rather than for a developer.

### TreatmentName

```csharp
public string TreatmentName;
```

Name of the treatment, which must match a treatment type defined in the model setup.

**Remarks.** The name is used as a dictionary key into the model's treatment types whenever a cost is allocated or a row is exported. A name with no matching treatment type therefore does not fail where it was created - it fails later, during costing or export.

### TreatmentPeriod

```csharp
public int TreatmentPeriod;
```

Modelling period in which the treatment is placed. Periods are 1-based; the constructor rejects zero or negative values.

### TreatmentRankParameter

```csharp
public double TreatmentRankParameter;
```

Treatment rank score from MOORA analysis. Used only in MCDA ranking type models.

### TreatmentSuitabilityScore

```csharp
public double TreatmentSuitabilityScore;
```

Treatment suitability score. Used only in MCDA ranking type models.

## Methods

### AssignBudgetCategoryFractions

```csharp
public void AssignBudgetCategoryFractions(Dictionary<string, decimal> budgetCategoryFractions)
```

Assigns the budget category fractions for this treatment instance if the treatment is related to multiple budget categories. The values must sum to 1 else an exception will be thrown. This method needs to be called explicitly by custom implementers to set the budget category fractions for a treatment instance based on domain logic - this will vary by treatment type and networkr/client rules.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `budgetCategoryFractions` | `Dictionary<string, decimal>` | Budget category name to the fraction of this treatment's cost charged to it. Must sum to 1. |

**Throws.**

- `System.Exception` — Thrown if the fractions do not sum to 1 (checked to six decimal places).

**Remarks.** Leave this alone and the whole cost goes to the treatment type's own budget category, which is what most treatments want.

A category named here must have a matching column in the project's budget setup, and this is the one place that is not checked for you. The framework validates each treatment type's own budget category during setup and reports a mismatch by name. It cannot validate a category that only exists once your code has run, so a name that does not match a budget column kills the run mid-way with a bare `KeyNotFoundException` that names nothing. Check your names against `model.Budget.BudgetCategories`.

Do NOT remove this method if the reference count shows zero. It is called explicitly by custom domain models, which are compiled separately, so no call site appears in this code base.

### CalculateDiscountedAndInflatedCost

```csharp
public void CalculateDiscountedAndInflatedCost(ModelBase model, int absolutePeriod)
```

Calculates `JCass_ModelCore.Treatments.TreatmentInstance.Cost` as quantity multiplied by unit rate, adjusted by the present-worth factor for the period, rounded to two decimal places.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | The framework model, which supplies the discount and inflation rates. |
| 2 | `absolutePeriod` | `int` | Period to discount to. |

**Remarks.** Called by the framework. A domain model does not normally call this - the framework costs candidates after they are triggered. Until it runs, `JCass_ModelCore.Treatments.TreatmentInstance.Cost` is zero.

### GetBudgetCategoryCosts

```csharp
public Dictionary<string, decimal> GetBudgetCategoryCosts(Dictionary<string, TreatmentType> treatmentTypes)
```

Splits this treatment's cost across budget categories, using the fractions assigned by `JCass_ModelCore.Treatments.TreatmentInstance.AssignBudgetCategoryFractions(System.Collections.Generic.Dictionary{System.String,System.Decimal})` or, where none were assigned, charging the whole cost to the treatment type's own budget category.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `treatmentTypes` | `Dictionary<string, TreatmentType>` | The model's treatment types, keyed by treatment name. |

**Returns.** Budget category name to the cost charged to it.

**Throws.**

- `System.Exception` — Thrown if an assigned fraction is outside the range 0 to 1.

**Remarks.** Called by the framework when funding and exporting spending.

### GetBudgetCategoryInfoForExport

```csharp
public string GetBudgetCategoryInfoForExport(Dictionary<string, TreatmentType> treatmentTypes)
```

Gets a string representation of the budget category for export purposes. If multiple budget categories are defined, the string will contain the budget category fractions as a delimited string with pipe-separated values showing the budget category and the fraction in that category, e.g. 'rehab: 0.7|seal:0.3'. If no budget category fractions are defined, the default budget category of the treatment type is returned.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `treatmentTypes` | `Dictionary<string, TreatmentType>` | — |

### GetRankDebugInformationStructure

```csharp
public static jcDataSet GetRankDebugInformationStructure(ModelBase model)
```

Builds the empty, correctly-columned data set that MCDA ranking debug rows are written into.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | The framework model, which supplies the element identifiers and the ranking setup's columns. |

**Returns.** An empty data set with the right columns.

**Remarks.** Framework plumbing. Pairs with `JCass_ModelCore.Treatments.TreatmentInstance.GetRankingDebugInformationRowForMCDA(JCass_ModelCore.Models.ModelBase,System.Int32)`.

### GetRankingDebugInformationRowForMCDA

```csharp
public Dictionary<string, object> GetRankingDebugInformationRowForMCDA(ModelBase model, int iPeriod)
```

Builds one row of MCDA ranking debug output for this treatment: its rank, its cost, and every raw-data column and model parameter the ranking setup used to score it.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | The framework model. |
| 2 | `iPeriod` | `int` | Modelling period. Parameter values are read from the previous epoch. |

**Returns.** Column name to value, matching the structure from `JCass_ModelCore.Treatments.TreatmentInstance.GetRankDebugInformationStructure(JCass_ModelCore.Models.ModelBase)`.

**Remarks.** Framework plumbing, produced only when a debug ranking period is configured. It is what you read to answer "why did the model choose that treatment over this one" - which makes it worth knowing exists, even though a domain model never calls it.

### GetSetupFunctionValueNumber

```csharp
public static double GetSetupFunctionValueNumber(Dictionary<string, object> row, Dictionary<string, object> functionValues, string setupColumn)
```

Resolves a setup cell that names a function into that function's numeric result.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `row` | `Dictionary<string, object>` | The setup row. |
| 2 | `functionValues` | `Dictionary<string, object>` | Function key to evaluated value, for the element and period in hand. |
| 3 | `setupColumn` | `string` | Column of `row` holding the function key. |

**Returns.** The function's value as a double.

**Throws.**

- `System.Exception` — Thrown, naming the key, if the function is not in `functionValues`.

**Remarks.** Framework plumbing. A domain model does not call this.

### GetSetupFunctionValueString

```csharp
public static string GetSetupFunctionValueString(Dictionary<string, object> row, Dictionary<string, object> functionValues, string setupColumn)
```

Resolves a setup cell that names a function into that function's text result.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `row` | `Dictionary<string, object>` | The setup row. |
| 2 | `functionValues` | `Dictionary<string, object>` | Function key to evaluated value, for the element and period in hand. |
| 3 | `setupColumn` | `string` | Column of `row` holding the function key. |

**Returns.** The function's value as text.

**Throws.**

- `System.Exception` — Thrown, naming the key, if the function is not in `functionValues`.

**Remarks.** Framework plumbing for setup sheets that reference function blocks. A domain model does not call this.

### GetSpendingExportColums

```csharp
public static List<ColumnInfo> GetSpendingExportColums(string[] identifiers, bool excludeIdentifiers = false)
```

Gets all of the columns that are required for exporting Spending data.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `identifiers` | `string[]` | — |
| 2 | `excludeIdentifiers` | `bool` | If true, identifier columns are omitted from the result. |

### GetSpendingExportRow

```csharp
public List<Dictionary<string, object>> GetSpendingExportRow(ModelBase model, int initialCalendarEpoch, Dictionary<string, object> baseRow = null)
```

Gets a dictionary row(s) for exporting spending data for each budget category to which the treatment applies.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | — |
| 2 | `initialCalendarEpoch` | `int` | — |
| 3 | `baseRow` | `Dictionary<string, object>` | — |

### GetTreatmentExportColums

```csharp
public static List<ColumnInfo> GetTreatmentExportColums(string[] identifiers, bool excludeIdentifiers = false)
```

Gets all of the columns that are required for exporting Treatment data.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `identifiers` | `string[]` | — |
| 2 | `excludeIdentifiers` | `bool` | If true, identifier columns are omitted from the result. |

### GetTreatmentExportRow

```csharp
public Dictionary<string, object> GetTreatmentExportRow(ModelBase model, int initialCalendarEpoch, Dictionary<string, object> row = null)
```

Gets a dictionary row for exporting treatment data. The row contains the treatment instance data and is used for exporting to CSV or other formats.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | — |
| 2 | `initialCalendarEpoch` | `int` | — |
| 3 | `row` | `Dictionary<string, object>` | — |

### IsRoutineMaintenance

```csharp
public bool IsRoutineMaintenance(ModelConfiguration modelConfig)
```

Wrapper to identify if this treatment is the specified Routine Maintenance treatment

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `modelConfig` | `ModelConfiguration` | Model Configuration |

**Returns.** True if this treatment's name matches the name of the routine maintenance treatment specified in the Meta Setup file and stored as property modelConfig.RoutineMaintenanceTreatmentName
