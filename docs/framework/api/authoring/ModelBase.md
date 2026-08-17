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

# ModelBase

**Namespace:** `JCass_ModelCore.Models`  
**Assembly:** `JCass_ModelCore`  
**Kind:** abstract class

> **Should a domain model use this?**  
> **You call it, you never create it.** It is the framework model, reachable as the protected `model` field on `DomainModelBase`.
>  
> Only the author-facing members are listed here. `ModelBase` has a large surface and the rest of it is framework plumbing — model control, exporting, optimiser orchestration — that a domain model has no business calling.

The framework model: it owns the loop over periods and elements, holds the input data, the model parameters, the lookups and the budget, and calls the domain model at each stage.

**Remarks.** A domain model reaches this as the protected `model` field on `DomainModelBase`.

You call it; you never create it and you never inherit it. The concrete model types - forecast, MCDA, benefit-cost - are the framework's own, chosen by the project's setup. The class you write inherits `DomainModelBase`.

Most of this class is framework plumbing. What a domain model actually uses is the small set of accessors: reading input data, reading and writing model parameters, reading lookups, and checking the budget.

## Properties

### Configuration

```csharp
public ModelConfiguration Configuration { get; }
```

The run's configuration - periods, rates, model type, and `WorkFolder`, which is the client folder a domain model builds side-car data paths from.

**Remarks.** Read it; do not write to it mid-run. See `JCass_ModelCore.ModelObjects.ModelConfiguration`.

### CurrentRollout

```csharp
public BcaRolloutContext CurrentRollout { get; }
```

Where the current domain model call sits inside a BCA strategy rollout, or null when the model is not inside one.

**Remarks.** Most domain models never need this. The period passed to `Increment`, `Reset`, `GetTreatmentCandidates` and `GetTriggeredMaintenance` is the real modelling period in a rollout exactly as in the main run, so period-based logic is correct without consulting it. See `JCass_ModelCore.ModelObjects.BcaRolloutContext` for the two cases that differ.

Set only by `TreatmentStrategyGenerator`, which rolls elements out one at a time on the calling thread. If strategy generation is ever parallelised across elements this must become thread-local, or two rollouts will overwrite each other's context.

### Lookups

```csharp
public Dictionary<string, Dictionary<string, object>> Lookups { get; set; }
```

Domain Model lookup set for looking up parameters/constants/etc.

### MultiColumnLookups

```csharp
public Dictionary<string, jcDataSet> MultiColumnLookups { get; set; }
```

Domain/Project lookup set for Multi-Column Lookup Tables. Key is the set name, and value is the lookup table as jcDataSet. See function 'JFuncLookupMultiColumn'

### Random

```csharp
public Random Random { get; }
```

The run's random number generator, seeded from `JCass_ModelCore.Models.ModelBase.RandomSeed`.

**Remarks.** Use this, or a domain model's own `Rando`, rather than constructing a `Random`. Both are seeded from the model configuration, which is what makes a run reproducible. A privately constructed `Random` is seeded from the clock, and the model then stops giving the same answer twice with nothing to show that anything changed.

### RandomSeed

```csharp
public int RandomSeed { get; set; }
```

Random seed for simulations. Setting this will re-initialise the Random object, so that you can reset the random seed at any point and get the same sequence of random numbers from that point on. This is useful for testing and debugging, as it allows you to get the same random numbers for each run.

## Fields

### Budget

```csharp
public Budget Budget;
```

The run's budget: what money is available in each category and each period, and how much of it is left.

**Remarks.** Read it - typically to ask whether a candidate could be afforded. The framework does the spending; a domain model proposes candidates and the optimiser decides what is funded.

### NElements

```csharp
public int NElements;
```

Number of elements in the run

### NParameters

```csharp
public int NParameters;
```

Number of model parameters

### NPeriods

```csharp
public int NPeriods;
```

Number of Modelling Periods

### StrategiesSetupData

```csharp
public List<StrategySetupInfo> StrategiesSetupData;
```

The multi-treatment strategies defined in the model setup, in the order the setup sheet lists them. Empty in models that do not use strategies.

**Remarks.** A domain model reads this when it needs to know which strategies the project defines - typically to decide, per element, which of them are worth offering. It does not create the entries; they come from the setup data and are populated before `SetupInstance` runs.

Reading this is not the same as building strategies. In a benefit-cost model the framework's own strategy generator rolls the candidates returned by `GetTreatmentCandidates` forward into multi-period strategies. A domain model returns candidates; it does not assemble a `JCass_ModelCore.Treatments.TreatmentStrategy`.

### TreatmentTypes

```csharp
public Dictionary<string, TreatmentType> TreatmentTypes;
```

The treatment types defined in the model setup, keyed by treatment name.

**Remarks.** A domain model reads this; it never adds to it. The usual reason to reach for it is to find a treatment's own budget category, or to check that a name a trigger is about to use is actually defined - a `JCass_ModelCore.Treatments.TreatmentInstance` carrying a name with no matching entry here does not fail where it was created, it fails later during costing or export.

Populated during setup, before the domain model's `SetupInstance` runs.

## Methods

### GetInputDataNumber

```csharp
public double GetInputDataNumber(int elementIndex, string header)
```

Reads a numeric value from the raw input data for one element - the network data as loaded, before any modelling.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `elementIndex` | `int` | Zero-based element index. |
| 2 | `header` | `string` | Input data column name. |

**Returns.** The value.

**Throws.**

- `System.Exception` — Thrown, naming the column, if it is not in the input data or is not numeric.

**Remarks.** Raw input data never changes during a run. It is what the element started as, and it is the same in period 1 and period 20. Anything that evolves - condition, age, treatment history - is a model parameter, read with `JCass_ModelCore.Models.ModelBase.GetParameterValueNumber(System.Int32,System.String,System.Int32)`. Confusing the two is one of the easiest mistakes to make and one of the hardest to see, because both return a plausible number.

### GetInputDataText

```csharp
public string GetInputDataText(int elementIndex, string header)
```

Reads a text value from the raw input data for one element - a material, a class, a treatment history code.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `elementIndex` | `int` | Zero-based element index. |
| 2 | `header` | `string` | Input data column name. |

**Returns.** The value.

**Throws.**

- `System.Exception` — Thrown, naming the column, if it is not in the input data or is not a text column.

**Remarks.** See `JCass_ModelCore.Models.ModelBase.GetInputDataNumber(System.Int32,System.String)` for why raw input data and model parameters are not the same thing.

### GetLookupValueNumber

```csharp
public double GetLookupValueNumber(string lookupSetName, string lookupKey)
```

Reads a numeric value from the model's lookups - the route by which every tunable number reaches a domain model.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `lookupSetName` | `string` | Name of the lookup set. |
| 2 | `lookupKey` | `string` | Key within that set. |

**Returns.** The value as a double.

**Throws.**

- `System.Exception` — Thrown, naming the set or the key, if either is not found or the value will not convert.

**Remarks.** Thresholds, limits and rates belong here, not in C#. A value in the lookups is one a modeller changes and re-runs themselves; the same value written as a constant in code needs a developer, a rebuild and a republish. It throws rather than defaulting, on purpose - a missing threshold silently becoming zero would change every forecast with nothing to show for it.

### GetLookupValueText

```csharp
public string GetLookupValueText(string lookupSetName, string lookupKey)
```

Reads a text value from the model's lookups.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `lookupSetName` | `string` | Name of the lookup set. |
| 2 | `lookupKey` | `string` | Key within that set. |

**Returns.** The value as text.

**Throws.**

- `System.Exception` — Thrown, naming the set or the key, if either is not found.

**Remarks.** A domain model inheriting `DomainModelBase` can call the equivalent helper on itself instead. Both do the same thing.

### GetParameterValueNumber

```csharp
public double GetParameterValueNumber(int ielem, string name, int iEpoch)
```

Reads a numeric model parameter for one element at one epoch. Model parameters are the values that evolve as the model runs.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `ielem` | `int` | Zero-based element index. |
| 2 | `name` | `string` | Parameter name, as defined in the model setup. |
| 3 | `iEpoch` | `int` | Epoch to read. Epoch 0 is the initial state; epoch N is the end of period N. |

**Returns.** The value.

**Throws.**

- `System.Collections.Generic.KeyNotFoundException` — Thrown if no parameter of that name is defined.

**Remarks.** An epoch marks the END of a period, and there is one more epoch than there are periods. Reading "the value at the start of period N" means reading epoch N-1. Getting that off by one shifts every forecast by a period and produces results that look entirely plausible.

Never read an epoch at or above the period you were handed. When the framework calls your domain model for period `iPeriod`, that period has not been stepped yet, so the highest epoch holding real data is `iPeriod - 1` - for every element, not just the one you were called about. Epoch `iPeriod` and above is uninitialised array, and outside a BCA rollout it is returned as zeros, silently. There is no warning and no exception: the run completes and the numbers are wrong.

Inside a BCA strategy rollout this is resolved for you, and enforced. For the element being rolled out, an epoch the strategy has already passed is answered from that strategy's own timeline, so "what was my condition three periods ago" is correct at any rollout depth, including periods past the end of the model. Everything the rollout cannot answer correctly throws rather than returning zeros. See `JCass_ModelCore.ModelObjects.BcaRolloutContext.TryResolveStrategyEpoch(System.Int32,System.Int32,System.Int32@)`.

### GetParameterValueText

```csharp
public string GetParameterValueText(int ielem, string name, int iEpoch)
```

Reads a text model parameter for one element at one epoch.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `ielem` | `int` | Zero-based element index. |
| 2 | `name` | `string` | Parameter name, as defined in the model setup. |
| 3 | `iEpoch` | `int` | Epoch to read. See `JCass_ModelCore.Models.ModelBase.GetParameterValueNumber(System.Int32,System.String,System.Int32)` for what an epoch is. |

**Returns.** The value.

**Throws.**

- `System.Collections.Generic.KeyNotFoundException` — Thrown if no parameter of that name is defined.

**Remarks.** The epoch rules are the same as for `JCass_ModelCore.Models.ModelBase.GetParameterValueNumber(System.Int32,System.String,System.Int32)`, including the silent zeros above `iPeriod - 1` and the rollout resolution. Read that first.

### GetParameterValues

```csharp
public ValueTuple<Dictionary<string, double>, Dictionary<string, string>> GetParameterValues(int iElem, int iEpoch)
```

Gets dictionaries of all parameter values for an element at an epoch - both numeric and text parameters held in separate dictionaries

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iElem` | `int` | Zero-based element index |
| 2 | `iEpoch` | `int` | Epoch for which to get parameter values. Epoch 0 is the initial state; epoch N is the end of period N. |

**Returns.** Two dictionaries, numeric first then text, each keyed by parameter name.

**Remarks.** Both dictionaries use ordinal string comparison, so parameter names are matched case-sensitively and exactly.

The epoch rules matter and are easy to get wrong. See `JCass_ModelCore.Models.ModelBase.GetParameterValueNumber(System.Int32,System.String,System.Int32)`: never read an epoch at or above the period you were handed, because outside a BCA rollout you get zeros with no warning. Inside a rollout this resolves against the strategy's own timeline and refuses what it cannot answer.

### GetSpecialPlaceholderValues

```csharp
public Dictionary<string, object> GetSpecialPlaceholderValues(int iElem, int period, TreatmentInstance treatment = null)
```

Builds the framework's reserved placeholder values for one element in one period: where it is in time, what treatment is coming next, what has been done to it before, and - during a reset
- what is being applied now.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iElem` | `int` | Zero-based element index. |
| 2 | `period` | `int` | Modelling period. |
| 3 | `treatment` | `TreatmentInstance` | The treatment being applied, when called during a reset. Null otherwise. |

**Returns.** Placeholder name to value.

**Remarks.** These keys are reserved words. They cannot be used as raw data column names or as model parameter names, and the framework checks that at setup - see `ReservedKeyNames`. Naming an input column `period` or `elem_index` fails the setup rather than quietly shadowing a placeholder.

Absence is expressed as a sentinel, not as null. Where there is no next treatment, `next_treatment_period` and `periods_to_next_treatment` are 999 and the text placeholders are the literal string `"none"`; where there is no current treatment, `this_treatment_cost` is `0`. Comparisons still behave sensibly, but anything that averages or sums `periods_to_next_treatment` across elements silently takes 999 as a real number. Test for the sentinel before doing arithmetic on it.

`previous_treatments` is the only entry that can genuinely be null.

Inside a BCA strategy rollout the treatment keys describe the strategy, not the network.`previous_treatments` reports what the strategy being evaluated has done to the element so far, over the model's real history before the strategy's base period, so a "periods since last treatment" calculation is right inside a rollout as well as outside one. The `next_treatment_*` keys are unaffected: the only treatments ahead of the current period are committed ones, and the strategy's own future has not been decided yet. See `JCass_ModelCore.Treatments.RolloutTreatmentLookup`.
