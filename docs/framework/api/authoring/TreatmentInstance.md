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

*The framework carries no `<summary>` for this type. The signatures below come
from the assembly metadata and are authoritative; the description is not available.*

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

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `element_index` | `int` | — |
| 2 | `name` | `string` | — |
| 3 | `period` | `int` | — |
| 4 | `quantity` | `double` | — |
| 5 | `unitRate` | `double` | — |
| 6 | `force` | `bool` | — |
| 7 | `reason` | `string` | — |
| 8 | `comment` | `string` | — |

> Positional order matters and 8 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

## Properties

### Cost

```csharp
public double Cost { get; }
```

*No framework documentation for this member.*

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

*No framework documentation for this member.*

### CustomAttributes

```csharp
public Dictionary<string, string> CustomAttributes;
```

*No framework documentation for this member.*

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

*No framework documentation for this member.*

### FollowUpWaitPeriods

```csharp
public int FollowUpWaitPeriods;
```

*No framework documentation for this member.*

### Force

```csharp
public bool Force;
```

*No framework documentation for this member.*

### IsCommitted

```csharp
public bool IsCommitted;
```

*No framework documentation for this member.*

### IsFollowUp

```csharp
public bool IsFollowUp;
```

*No framework documentation for this member.*

### Quantity

```csharp
public double Quantity;
```

*No framework documentation for this member.*

### RankParamSimple

```csharp
public double RankParamSimple;
```

Temporary simple ranking for maintenance. Higher parameter value means higher priority.

### Reason

```csharp
public string Reason;
```

*No framework documentation for this member.*

### TreatmentName

```csharp
public string TreatmentName;
```

*No framework documentation for this member.*

### TreatmentPeriod

```csharp
public int TreatmentPeriod;
```

*No framework documentation for this member.*

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
| 1 | `budgetCategoryFractions` | `Dictionary<string, decimal>` | — |

**Remarks.** Do NOT remove this method if references shows Zero!!! This needs to be called explicitly by custom implementers, and since these are read only at runtime, the reference is not in the code base.

### CalculateDiscountedAndInflatedCost

```csharp
public void CalculateDiscountedAndInflatedCost(ModelBase model, int absolutePeriod)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | — |
| 2 | `absolutePeriod` | `int` | — |

### GetBudgetCategoryCosts

```csharp
public Dictionary<string, decimal> GetBudgetCategoryCosts(Dictionary<string, TreatmentType> treatmentTypes)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `treatmentTypes` | `Dictionary<string, TreatmentType>` | — |

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

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | — |

### GetRankingDebugInformationRowForMCDA

```csharp
public Dictionary<string, object> GetRankingDebugInformationRowForMCDA(ModelBase model, int iPeriod)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | — |
| 2 | `iPeriod` | `int` | — |

### GetSetupFunctionValueNumber

```csharp
public static double GetSetupFunctionValueNumber(Dictionary<string, object> row, Dictionary<string, object> functionValues, string setupColumn)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `row` | `Dictionary<string, object>` | — |
| 2 | `functionValues` | `Dictionary<string, object>` | — |
| 3 | `setupColumn` | `string` | — |

### GetSetupFunctionValueString

```csharp
public static string GetSetupFunctionValueString(Dictionary<string, object> row, Dictionary<string, object> functionValues, string setupColumn)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `row` | `Dictionary<string, object>` | — |
| 2 | `functionValues` | `Dictionary<string, object>` | — |
| 3 | `setupColumn` | `string` | — |

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
