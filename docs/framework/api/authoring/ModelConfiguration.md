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

# ModelConfiguration

**Namespace:** `JCass_ModelCore.ModelObjects`  
**Assembly:** `JCass_ModelCore`  
**Kind:** class

> **Should a domain model use this?**  
> You read it. Never write to it.
>  
> `WorkFolder` is the one property most domain models need: it is the **client root**, so `Path.Combine(WorkFolder, "supporting/<name>.csv")` resolves to the same file under a normal run and under a debug F5 run. There is no bundle-folder property, which is why side-car CSVs belong in `supporting/` and not in the bundle.

*The framework carries no `<summary>` for this type. The signatures below come
from the assembly metadata and are authoritative; the description is not available.*

## Constructors

### ModelConfiguration

```csharp
public ModelConfiguration()
```

*No framework documentation for this member.*

## Properties

### BCABenefitIsRelativeToMaintenanceOnly

```csharp
public bool BCABenefitIsRelativeToMaintenanceOnly { get; set; }
```

Flag to indicate if the Benefit-Cost Analysis benefit and cost calculations are relative to maintenance only (true) or relative to doing nothing (false).

### BCALookAheadPeriods

```csharp
public int BCALookAheadPeriods { get; set; }
```

Number of look-ahead periods to use for BCA optimisation. Should not be more than the number of years in the budget.

### BCAOptimisationMethod

```csharp
public string BCAOptimisationMethod { get; set; }
```

Optimisation method to use for Benefit-Cost Analysis types. Set in setup using the treatment selection key in the Meta setup file. Valid values are:

### BCAStrategyPeriodsToSkip

```csharp
public int BCAStrategyPeriodsToSkip { get; set; }
```

Number of periods to skip between repeated branches of the same treatment. If zero, then all triggered branches are considered. If a value of say 1, then if a treatment is triggered in period 1, the same treatment will not be considered again until period 3, etc. Setting this to 1 or more can significantly speed up BCA-optimisation model runs, with a small potential that slightly sub-optimal strategies may be picked.

### BudgetTagName

```csharp
public string BudgetTagName { get; set; }
```

*No framework documentation for this member.*

### CommandID

```csharp
public int CommandID { get; set; }
```

*No framework documentation for this member.*

### DebugStrategyElementIndex

```csharp
public int DebugStrategyElementIndex { get; set; }
```

Element Index for which to export detailed strategy debugging information. Set to -1 to disable debug export.

### DebugStrategyPeriodIndex

```csharp
public int DebugStrategyPeriodIndex { get; set; }
```

Period Index for which to export detailed strategy debugging information. Set to -1 to disable debug export.

### DebugTreatRankPeriod

```csharp
public int DebugTreatRankPeriod { get; set; }
```

Period Index for which to export detailed treatment candidate set debugging information. Set to -1 to disable debug export.

### DiscountRatePercent

```csharp
public double DiscountRatePercent { get; set; }
```

Discount Rate (percent), e.g. 6.7 for 6.7%

### DomainModelClassName

```csharp
public string DomainModelClassName { get; set; }
```

*No framework documentation for this member.*

### DomainModelDLLFilePath

```csharp
public string DomainModelDLLFilePath { get; set; }
```

*No framework documentation for this member.*

### ElementIdentifiers

```csharp
public string[] ElementIdentifiers { get; set; }
```

Columns in the Input Set to include in output set for easy element identification and debugging/checking.

### ExportData

```csharp
public bool ExportData { get; set; }
```

Flag that determines if Treatment, Debug and Parameter data should be exported. Default is TRUE, but this can be set to false for Goal Seeking purposes to speed up iterative model runs.

### ExportLongFormatFile

```csharp
public bool ExportLongFormatFile { get; set; }
```

Should the parameter data be exported in a 'Long-Format' (narrow format) file. Note that for many parameters and elements (say more than 10,000), this file can cause 'out-of-memory' problems

### ExportParameterNames

```csharp
public List<string> ExportParameterNames { get; set; }
```

List of Parameters for which data should be exported. If there is only one value = 'all', then data for ALL parameters are exported

### FeedbackMode

```csharp
public int FeedbackMode { get; set; }
```

Determines how dense the feedback is.

3 = Chatty

2 = Somewhat Shy

1 = Introvert

0 = Silent

### GroupColumnName

```csharp
public string GroupColumnName { get; set; }
```

Name of the column that holds Group Codes/Ids for a Grouped MCDA model. Optional - only valid for Grouped MCDA models

### InflationRatePercent

```csharp
public double InflationRatePercent { get; set; }
```

Infalation Rate (percent), e.g. 6.7 for 6.7%

### InitialCalendarEpoch

```csharp
public int InitialCalendarEpoch { get; set; }
```

Calendar year of the first modelling period (i.e. period zero), e.g. 2024

### Logger

```csharp
public ILogItemData Logger { get; set; }
```

*No framework documentation for this member.*

### MCDACostScalingColumn

```csharp
public string MCDACostScalingColumn { get; set; }
```

Name of the input column that contains the quantity by means of which to scale costs for MCDA/MOORA optimisation. Normally, this will map to a input column containing e.g. quantity like length or square metre. If this value is left as 'none', then cost is scaled by 1 (no scaling)

### MaximumTreatmentsPerPeriod

```csharp
public int MaximumTreatmentsPerPeriod { get; set; }
```

Maximum number of non-routine treatments that can be applied in a single period. Currently, only implemented in the MCDA ungrouped model. Default value is 99,9999 (i.e. effectively no limit)

### MinimiseBCAObjectiveParameter

```csharp
public bool MinimiseBCAObjectiveParameter { get; set; }
```

Flag to indicative if the Objective Parameter is to be minimised (true) or maximised (false)

### MinimumTreatmentSuitabilityScoreAllowed

```csharp
public double MinimumTreatmentSuitabilityScoreAllowed { get; set; }
```

Minimum Treatment Suitability Score allowed for a treatment to be considered a candidate in MCDA analysis. This property is only used for MCDA analyses. If the treatment suitability score is below this value, the treatment will not be added to the candidate treatments list (this decision is made in the Default Domain Model).

### ModelTypeName

```csharp
public string ModelTypeName { get; set; }
```

Name of the model type to run. Must be one of the allowed model types in the _ModelTypesAllowed list.

### ModelVersionCode

```csharp
public string ModelVersionCode { get; set; }
```

*No framework documentation for this member.*

### MonteCarloExportParameters

```csharp
public List<string> MonteCarloExportParameters { get; set; }
```

*No framework documentation for this member.*

### NumberOfModellingPeriods

```csharp
public int NumberOfModellingPeriods { get; set; }
```

*No framework documentation for this member.*

### ObjectiveParameterName

```csharp
public string ObjectiveParameterName { get; set; }
```

Name of the Model Parameter that is the Objective for BCA optimisation.

### RandomSeed

```csharp
public int RandomSeed { get; set; }
```

*No framework documentation for this member.*

### RawDataFilePath

```csharp
public string RawDataFilePath { get; set; }
```

For desktop application postprocessing. Not needed for web.

### RoutineMaintenanceIncludeInFWPExport

```csharp
public bool RoutineMaintenanceIncludeInFWPExport { get; set; }
```

Should we include Routine Maintenance treatments in the FWP table export?

### RoutineMaintenanceTreatmentName

```csharp
public string RoutineMaintenanceTreatmentName { get; set; }
```

Name of the Treatment that represents Routine Maintenance

### RoutineMaintenanceTriggerReset

```csharp
public bool RoutineMaintenanceTriggerReset { get; set; }
```

Does Routine Maintenance result in a condition reset? If true, then the Reset method on the Domain Model is called. If false, then the Increment method on the Domain Model is called.

### RunKey

```csharp
public string RunKey { get; set; }
```

*No framework documentation for this member.*

### RunParallel

```csharp
public bool RunParallel { get; set; }
```

*No framework documentation for this member.*

### TriggerMaintenance

```csharp
public bool TriggerMaintenance { get; set; }
```

Flag to indicate if Routine Maintenance treatments should be triggered during the model run.

### UserID

```csharp
public int UserID { get; set; }
```

*No framework documentation for this member.*

### WorkFolder

```csharp
public string WorkFolder { get; set; }
```

Path to work folder, for desktop implementations. This is needed in the model run to:

1. Load Machine Learning models from the 'ml' sb-folder

2. Pass to the Exporter for exporting data after the run

## Methods

### GetElementIdentifiersString

```csharp
public string GetElementIdentifiersString()
```

*No framework documentation for this member.*

### GetExportParameterNameString

```csharp
public string GetExportParameterNameString()
```

*No framework documentation for this member.*

### Setup — overload 1 of 2

```csharp
public void Setup(jcDataSet setup, ILogItemData logger)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `setup` | `jcDataSet` | — |
| 2 | `logger` | `ILogItemData` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### Setup — overload 2 of 2

```csharp
public void Setup(
    string workFolder,
    ILogItemData logger,
    int userID = 0,
    int commandID = 0)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `workFolder` | `string` | — |
| 2 | `logger` | `ILogItemData` | — |
| 3 | `userID` | `int` | — |
| 4 | `commandID` | `int` | — |

> Positional order matters and 4 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

### UpdateForGoalSeek

```csharp
public void UpdateForGoalSeek(int feedbackMode, List<string> paramsToOutput, bool exportData = false)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `feedbackMode` | `int` | — |
| 2 | `paramsToOutput` | `List<string>` | — |
| 3 | `exportData` | `bool` | — |
