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

Everything about how a model run is configured: how many periods, which model type, the discount and inflation rates, where the run's files live, and which domain model to load.

**Remarks.** Populated from the project's setup data before the run starts, and reachable from a domain model as `model.Configuration`.

Read it; do not write to it. Changing a value here part-way through a run changes the rules the run is being scored under, and nothing recalculates what has already happened.

## Constructors

### ModelConfiguration

```csharp
public ModelConfiguration()
```

Creates a configuration with the framework's defaults, which one of the `Setup` overloads then overwrites from the project's setup data.

**Remarks.** The defaults are a working MCDA model: 10 periods from calendar epoch 2024, 3% discount and inflation, incremental benefit-cost optimisation over a 15-period look-ahead, routine maintenance off, and the three debug indices set to -1 meaning "no debug output".

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

Number of look-ahead periods over which each candidate treatment strategy is rolled out and evaluated in a BCA model. Must be between 1 and 35. Values of 5 to 20 are recommended: evaluating strategies decades ahead is not realistic given the uncertainty in long-range forecasts, and a longer look-ahead multiplies the number of strategies generated, so run times rise steeply. Values below 5 are normally only useful for debugging, because they leave no room for a follow-up treatment. It is normal and correct for a rollout near the end of a run to evaluate periods beyond the last modelled one - no treatment is placed there.

### BCAMaximumStrategiesPerElement

```csharp
public int BCAMaximumStrategiesPerElement { get; set; }
```

Largest number of treatment strategies that will be generated for a single element in a single period. Strategy generation stops adding branches once an element reaches this number, and the run continues with the strategies already generated rather than failing. Each candidate treatment is guaranteed a share of this budget, so what is dropped is variations of an option rather than the option itself - provided the value is large enough to hold the two baselines plus one strategy for every candidate treatment, which a separate warning checks. Fewer variations of each are compared. A warning is logged whenever it happens. Raise this to compare more strategies at the cost of a slower run; lower it to speed a run up. Allowed range is 10 to 500, and the default is 200. Raising it has sharply diminishing returns - strategies beyond the first few for each candidate treatment are near-duplicates of one another - while the cost in run time is strictly proportional, so prefer generating fewer strategies via 'BCA Strategy Periods to Skip' or a shorter look-ahead.

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

Which budget sheet in the project's budget workbook this run uses. Set from the model setup.

**Remarks.** The named sheet's columns are the budget categories the run can fund, and nothing else is a valid category. A treatment type pointing at a category with no column here is caught during setup and named; a category introduced at runtime by `TreatmentInstance.AssignBudgetCategoryFractions` cannot be, and fails mid-run instead.

### CommandID

```csharp
public int CommandID { get; set; }
```

Identifier of the command or job that started the run, for logging. Zero when the host does not supply one.

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

Name of the class inside that assembly which implements the domain model interface. Matched case-insensitively against the type name.

**Remarks.** This must match your entry class's name exactly. A mismatch fails the run at load time with "class not found" - which is loud, but points at the assembly rather than at the name that was actually wrong.

### DomainModelDLLFilePath

```csharp
public string DomainModelDLLFilePath { get; set; }
```

Full path to the compiled domain model assembly the framework loads for this run.

**Remarks.** Set by the framework from the project setup. A domain model does not use it.

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

Sink for run progress and log messages. Supplied by whatever is hosting the run.

**Remarks.** Excluded from serialisation. A domain model logs through the framework model, not through this.

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

Version code for the model setup. Carried on the configuration and serialised with it, but nothing in the framework reads it.

### MonteCarloExportParameters

```csharp
public List<string> MonteCarloExportParameters { get; set; }
```

Parameters to export from a Monte Carlo run, across all simulations.

**Remarks.** Only meaningful for Monte Carlo runs, where exporting every parameter for every simulation would be unmanageably large. Ignored by other model types.

### NumberOfModellingPeriods

```csharp
public int NumberOfModellingPeriods { get; set; }
```

How many periods the model runs for. Periods are 1-based, and there is always one more epoch than there are periods, because epoch 0 holds the initial state.

### ObjectiveParameterName

```csharp
public string ObjectiveParameterName { get; set; }
```

Name of the Model Parameter that is the Objective for BCA optimisation.

### RandomSeed

```csharp
public int RandomSeed { get; set; }
```

Seed for the run's random number generator, which is what makes a run reproducible: the same seed and the same inputs give the same forecast.

**Remarks.** The framework seeds both its own generator and a domain model's `Rando` from this. Use those rather than constructing a `Random` of your own, or the run stops being reproducible without anything appearing to be wrong.

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

Identifier for this run, appended to every output file name so that results from different runs sit side by side without overwriting each other.

### RunParallel

```csharp
public bool RunParallel { get; set; }
```

True if the framework processes elements in parallel.

**Remarks.** This is why a domain model must not keep mutable state that spans elements. With parallel processing on, several elements are inside your trigger and increment methods at the same time. A field on your domain model written during one element and read during another produces results that vary between runs and cannot be reproduced. Per-element state belongs in model parameters; anything genuinely network-wide belongs in `DoEndOfPeriodCalculations`, which runs once per period after every element is done.

### TriggerMaintenance

```csharp
public bool TriggerMaintenance { get; set; }
```

Flag to indicate if Routine Maintenance treatments should be triggered during the model run.

### UserID

```csharp
public int UserID { get; set; }
```

Identifier of the user who started the run, for logging. Zero when the host does not supply one.

### WorkFolder

```csharp
public string WorkFolder { get; set; }
```

Root folder for this run's project files - the client folder, holding `inputs/`, `supporting/` and `outputs/`.

**Remarks.** This is the property to build side-car data paths from, and it is the only folder property the framework exposes.

`Path.Combine(WorkFolder, "supporting/my_coefficients.csv")` resolves to the same file under an ordinary run and under an in-browser debug run. A path built relative to the domain model's own bundle folder does not: the bundle is staged under a different folder name when debugging, so a bundle-relative path reads the wrong folder in one of the two cases. Verified on both run types.

It also gives the framework the `ml` sub-folder for machine-learning models, and is passed to the exporter for writing results.

## Methods

### GetElementIdentifiersString

```csharp
public string GetElementIdentifiersString()
```

The element identifier column names joined with a pipe, for logging and export headers.

**Returns.** The joined names, or an empty string if none are configured.

### GetExportParameterNameString

```csharp
public string GetExportParameterNameString()
```

The export parameter names joined with a pipe, for logging and export headers.

**Returns.** The joined names, or an empty string if none are configured.

### Setup — overload 1 of 2

```csharp
public void Setup(jcDataSet setup, ILogItemData logger)
```

Fills the configuration from the project's meta setup data, validating each setting as it goes. Called by the framework before the run starts.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `setup` | `jcDataSet` | The meta setup table, which must have a "Setting" column of setting names. |
| 2 | `logger` | `ILogItemData` | Sink for setup messages and warnings. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Throws.**

- `System.Exception` — Thrown, naming the setting, if a required setting is missing or holds a value the framework does not accept.

### Setup — overload 2 of 2

```csharp
public void Setup(
    string workFolder,
    ILogItemData logger,
    int userID = 0,
    int commandID = 0)
```

Sets only the run's location and identity - work folder, logger and the user and command identifiers - leaving every modelling setting at its default.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `workFolder` | `string` | The client folder for this run. See `JCass_ModelCore.ModelObjects.ModelConfiguration.WorkFolder`. |
| 2 | `logger` | `ILogItemData` | Sink for setup messages. |
| 3 | `userID` | `int` | Identifier of the user starting the run, for logging. |
| 4 | `commandID` | `int` | Identifier of the command or job, for logging. |

> Positional order matters and 4 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

**Throws.**

- `System.Exception` — Thrown if the command name check fails.

**Remarks.** The counterpart to the overload that takes setup data, for hosts that configure the model in code rather than from a setup file. It does not read any modelling settings, so anything not assigned afterwards keeps the constructor's defaults.

### UpdateForGoalSeek

```csharp
public void UpdateForGoalSeek(int feedbackMode, List<string> paramsToOutput, bool exportData = false)
```

Turns down logging and exporting for a goal-seeking run, which executes the model many times over and would otherwise produce one full set of output per iteration.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `feedbackMode` | `int` | Logging verbosity to use for the iterations. See `JCass_ModelCore.ModelObjects.ModelConfiguration.FeedbackMode`. |
| 2 | `paramsToOutput` | `List<string>` | Parameters to keep exporting. |
| 3 | `exportData` | `bool` | True to keep writing output files during the iterations. |

**Remarks.** Called by the goal-seeking model. A domain model does not call this - it changes the configuration mid-run, which is otherwise exactly what not to do.
