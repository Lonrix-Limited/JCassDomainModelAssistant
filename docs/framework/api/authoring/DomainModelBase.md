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

# DomainModelBase

**Namespace:** `JCass_ModelCore.DomainModels`  
**Assembly:** `JCass_ModelCore`  
**Kind:** abstract class

> **Should a domain model use this?**  
> **Yes — your entry class inherits from this.**
>  
> The abstract members are the six execution stages you must write. The protected members — `model`, `Rando`, `PIndex` — are what you call from inside them. `SetupInstance` is where your own setup goes, and its own page below is the one to read before writing any: lookups and configuration ARE ready there, and `model.NElements`, `model.NPeriods` and `model.NParameters` are all still zero, which is one of the framework's quietest failures.

Base class for every domain model. Inherit from this, implement the abstract methods, and the framework will call them at the right point in every modelling period.

**Remarks.** The framework owns the loop over periods and elements, the optimisation and the budgeting. A domain model owns the engineering: what deteriorates and how fast, what treatments are worth considering, and what a treatment resets. The abstract methods below are the seam between the two, and each one corresponds to a stage of model execution.

Setup order matters and it is not obvious. The framework creates your class with its parameterless constructor, then calls `SetupBase`, which assigns `JCass_ModelCore.DomainModels.DomainModelBase.model`, seeds `JCass_ModelCore.DomainModels.DomainModelBase.Rando` and calls `JCass_ModelCore.DomainModels.DomainModelBase.SetupInstance`. Nothing that touches the framework works before that - see `JCass_ModelCore.DomainModels.DomainModelBase.SetupInstance` for what is and is not ready by then.

**Implements:** `IDomainModel`  

## Constructors

### DomainModelBase

```csharp
public DomainModelBase()
```

Creates the domain model. The framework instantiates your class through its parameterless constructor when it loads your assembly, so your own constructor must not require arguments.

**Remarks.** Do no work here. `JCass_ModelCore.DomainModels.DomainModelBase.model` is not assigned until the framework calls `SetupBase` afterwards, so anything in a constructor that touches inputs, lookups or configuration fails with a null reference. `JCass_ModelCore.DomainModels.DomainModelBase.SetupInstance` is the place for it.

## Fields

### Rando

```csharp
protected Random Rando;
```

Random number generator for this domain model, seeded by the framework from the model's configured random seed.

**Remarks.** Use this rather than creating your own `Random`. It is seeded from the model configuration, which is what makes a run reproducible: the same seed and the same inputs give the same forecast. A privately constructed `Random` is seeded from the clock and quietly destroys that property - the model still runs, and it stops giving the same answer twice.

Assigned by the framework in `SetupBase`; it is null before then.

### model

```csharp
protected ModelBase model;
```

The framework model. This is how a domain model reaches input data, model parameters, lookups, the budget and the model configuration.

**Remarks.** Assigned by the framework in `SetupBase`, which runs after your constructor. It is null inside your constructor, so anything that reads inputs or lookups belongs in `JCass_ModelCore.DomainModels.DomainModelBase.SetupInstance` and not in the constructor.

## Methods

### DoEndOfPeriodCalculations

```csharp
public abstract void DoEndOfPeriodCalculations(int iPeriod)
```

Stub for the Domain Model that can be used to perform any end of period calculations after the treatment selection and parameter updates have been performed for all elements in the current period. This can be used to calculate any additional parameters that are needed for the next period or for reporting purposes, using the updated parameter values after treatment application. This method is called from the Framework Model at the end of each period, after all elements have been processed for the current period. You can use this to do things such as calculating network level rankings, statistics, proportions over/under etc. that can be used to drive decisions in the next period. Implementers should store calculated values in the Domain Model object. Take care on how you index or store results. Inless you index by period, your values will be replaced/recycled at the end of each period.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iPeriod` | `int` | Modelling period (values like 1,2,...n) |

### GetEmptySetupSet

```csharp
public static jcDataSet GetEmptySetupSet(string dataType)
```

Returns a template setup table of the requested kind, with the right columns and one or two example rows, for use when generating a starting setup file.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `dataType` | `string` | One of: raw_headers, parameters, treatments, mcda_setup, network_functions, functionSet. |

**Returns.** An empty-but-shaped data set with example rows.

**Throws.**

- `System.Exception` — Thrown if `dataType` is not one of the handled values.

**Remarks.** A tooling helper for producing setup templates, not part of a model's runtime path. A domain model does not call this during a run.

### GetExecutionStageNames

```csharp
public static List<string> GetExecutionStageNames()
```

The execution stage names the framework recognises in a model's function-block setup, in execution order: precalcs, initialise, increment, triggers, resets, maintenance, postcalcs.

**Returns.** The allowed stage names.

**Remarks.** Used to validate setup data. A domain model does not normally call it.

### GetLookupValueNumber

```csharp
public double GetLookupValueNumber(string lookupSetName, string settingKey)
```

Reads a numeric value from the model's lookups. This is how a tunable number - a trigger threshold, an age limit, a unit rate - reaches your code.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `lookupSetName` | `string` | Name of the lookup set. |
| 2 | `settingKey` | `string` | Key within that lookup set. |

**Returns.** The value, converted to a double.

**Throws.**

- `System.Exception` — Thrown, naming the set or the key, if either is not found.

**Remarks.** Put every tunable number here rather than in C#. A number in the lookups is one the modeller changes themselves and re-runs; the same number written as a constant in code needs a developer, a rebuild and a republish to change. Structural values - unit conversions, array bounds, sentinels - are not tunable and stay in code as named constants.

It throws rather than returning a default, on purpose: a missing threshold that silently became zero would change every forecast without anything to show for it. Available from `JCass_ModelCore.DomainModels.DomainModelBase.SetupInstance` onwards.

### GetLookupValueText

```csharp
public string GetLookupValueText(string lookupSetName, string settingKey)
```

Reads a text value from the model's lookups - a material class, a treatment name, a policy switch.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `lookupSetName` | `string` | Name of the lookup set. |
| 2 | `settingKey` | `string` | Key within that lookup set. |

**Returns.** The value as a string, or an empty string if the stored value is null.

**Throws.**

- `System.Exception` — Thrown, naming the set or the key, if either is not found.

**Remarks.** The text counterpart of `JCass_ModelCore.DomainModels.DomainModelBase.GetLookupValueNumber(System.String,System.String)`, and the same rule applies: if a modeller would ever change it to recalibrate the model, it belongs in the lookups rather than in code. Available from `JCass_ModelCore.DomainModels.DomainModelBase.SetupInstance` onwards.

### GetTreatmentCandidates

```csharp
public abstract List<TreatmentInstance> GetTreatmentCandidates(
    int iElemIndex,
    int iPeriod,
    Dictionary<string, double> numInputs,
    Dictionary<string, string> textInputs,
    Dictionary<string, double> numModParamValues,
    Dictionary<string, string> textModParamValues)
```

Execute treatment selection/trigger logic to select all treatment instances for an element in the current period. The framework model will call this method for each element in and for each period. This method is only used in MCDA type models that evaluate individual treatments instead of Strategies. Use the raw input values for the element as well as the previous values of the parameters for the element with your domain logic to determine which treatment(s) can be considered for this element in the optimisation stage. If no treatments are applicable, return an empty list.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iElemIndex` | `int` | Zero-based index of the element |
| 2 | `iPeriod` | `int` | Modelling period (values like 1,2,...n) |
| 3 | `numInputs` | `Dictionary<string, double>` | Raw numeric input values for the element. Keys are input names, values are input values |
| 4 | `textInputs` | `Dictionary<string, string>` | Raw text input values for the element. Keys are input names, values are input values |
| 5 | `numModParamValues` | `Dictionary<string, double>` | Values for Numeric Model Parameters as they were in the previous epoch. Keys are parameter names |
| 6 | `textModParamValues` | `Dictionary<string, string>` | Values for Text Model Parameters as they were at the previous epoch. Keys are parameter names |

> Positional order matters and 6 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

**Returns.** A list of all treatment instances to consider for this element in the optimisation stage

### GetTriggeredMaintenance

```csharp
public abstract TreatmentInstance GetTriggeredMaintenance(
    int ielem,
    int iPeriod,
    Dictionary<string, double> numInputs,
    Dictionary<string, string> textInputs,
    Dictionary<string, double> numModParamValues,
    Dictionary<string, string> textModParamValues)
```

Uses domain logic to determine if there is routine maintenance triggered for the current element and period. This method is called from the Framework Model after treatment selection to determine if there is any triggered maintenance that should be applied to the element. If there is no triggered maintenance, return null.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `ielem` | `int` | Zero-based index of the element |
| 2 | `iPeriod` | `int` | Modelling period (values like 1,2,...n) |
| 3 | `numInputs` | `Dictionary<string, double>` | Raw numeric input values for the element. Keys are input names, values are input values |
| 4 | `textInputs` | `Dictionary<string, string>` | Raw text input values for the element. Keys are input names, values are input values |
| 5 | `numModParamValues` | `Dictionary<string, double>` | Values for Numeric Model Parameters as they were in the previous epoch. Keys are parameter names |
| 6 | `textModParamValues` | `Dictionary<string, string>` | Values for Text Model Parameters as they were at the previous epoch. Keys are parameter names |

> Positional order matters and 6 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

**Returns.** A Treatment Instance object representing Routine Maintenance

### Increment

```csharp
public abstract void Increment(
    int iElemIndex,
    int iPeriod,
    Dictionary<string, double> numInputs,
    Dictionary<string, string> textInputs,
    Dictionary<string, double> currentNumModParamValues,
    Dictionary<string, string> currentTextModParamValues,
    Action<string, double> numModParamValues,
    Action<string, string> textModParamValues)
```

Evaluates the Increment for all parameters for the element in the current period. This method is called from the Framework Model for elements that do not have a treatment selected after optimisation in the current period.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iElemIndex` | `int` | Zero-based index of the element |
| 2 | `iPeriod` | `int` | Modelling period (values like 1,2,...n) |
| 3 | `numInputs` | `Dictionary<string, double>` | Raw numeric input values for the element. Keys are input names, values are input values |
| 4 | `textInputs` | `Dictionary<string, string>` | Raw text input values for the element. Keys are input names, values are input values |
| 5 | `currentNumModParamValues` | `Dictionary<string, double>` | Values for Numeric Model Parameters as they were in the previous epoch. Keys are parameter names |
| 6 | `currentTextModParamValues` | `Dictionary<string, string>` | Values for Text Model Parameters as they were at the previous epoch. Keys are parameter names |
| 7 | `numModParamValues` | `Action<string, double>` | Return value: Sink holding values for numeric parameters (to be updated by Domain Model). Keys are parameter names, values are assigned values |
| 8 | `textModParamValues` | `Action<string, string>` | Return value: Sink holding values for text parameters (to be updated by Domain Model). Keys are parameter names, values are assigned values |

> Positional order matters and 8 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

### Initialise

```csharp
public abstract void Initialise(
    int iElemIndex,
    Dictionary<string, double> numInputs,
    Dictionary<string, string> textInputs,
    Action<string, double> numModParamValues,
    Action<string, string> textModParamValues)
```

Evaluates the Initial Values for all parameters for the element at the start of the analysis. This method is called from the Framework Model for all elements at the start of the model run. Use the raw/input data values with domain logic to assign an initial value to all modelling parameters.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iElemIndex` | `int` | Zero-based index of the element |
| 2 | `numInputs` | `Dictionary<string, double>` | Raw numeric input values for the element. Keys are input names, values are input values |
| 3 | `textInputs` | `Dictionary<string, string>` | Raw text input values for the element. Keys are input names, values are input values |
| 4 | `numModParamValues` | `Action<string, double>` | Return value: Sink holding values for numeric parameters (to be updated by Domain Model). Keys are parameter names, values are assigned values |
| 5 | `textModParamValues` | `Action<string, string>` | Return value: Sink holding values for text parameters (to be updated by Domain Model). Keys are parameter names, values are assigned values |

> Positional order matters and 5 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

### PIndex

```csharp
protected int PIndex(string paramName)
```

Returns the zero-based position of a model parameter in the forecast data array, given the parameter's name.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `paramName` | `string` | Name or code of the parameter, as defined in the model setup. |

**Returns.** Zero-based index of the parameter.

**Throws.**

- `System.Collections.Generic.KeyNotFoundException` — Thrown if no parameter of that name is defined in the model setup.

**Remarks.** The exception is a spelling check you get for free: a parameter name that does not match the setup fails immediately and by name, rather than silently reading the wrong column.

### Reset

```csharp
public abstract void Reset(
    TreatmentInstance treatment,
    int iElemIndex,
    int iPeriod,
    Dictionary<string, double> numInputs,
    Dictionary<string, string> textInputs,
    Dictionary<string, double> currentNumModParamValues,
    Dictionary<string, string> currentTextModParamValues,
    Action<string, double> numModParamValues,
    Action<string, string> textModParamValues)
```

Evaluates the Reset/Updated values for all parameters for the element at the start of the analysis. This method is called from the Framework Model for all elements at the start of the model run. Use the raw/input data values with domain logic to assign an initial value to all modelling parameters.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `treatment` | `TreatmentInstance` | The treatment being applied that triggers this reset; null/None when no treatment. |
| 2 | `iElemIndex` | `int` | Zero-based index of the element |
| 3 | `iPeriod` | `int` | Modelling period (values like 1,2,...n) |
| 4 | `numInputs` | `Dictionary<string, double>` | Raw numeric input values for the element. Keys are input names, values are input values |
| 5 | `textInputs` | `Dictionary<string, string>` | Raw text input values for the element. Keys are input names, values are input values |
| 6 | `currentNumModParamValues` | `Dictionary<string, double>` | Values for Numeric Model Parameters as they were in the previous epoch. Keys are parameter names |
| 7 | `currentTextModParamValues` | `Dictionary<string, string>` | Values for Text Model Parameters as they were at the previous epoch. Keys are parameter names |
| 8 | `numModParamValues` | `Action<string, double>` | Return value: Sink holding values for numeric parameters (to be updated by Domain Model). Keys are parameter names, values are assigned values |
| 9 | `textModParamValues` | `Action<string, string>` | Return value: Sink holding values for text parameters (to be updated by Domain Model). Keys are parameter names, values are assigned values |

> Positional order matters and 9 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

### SetRandomSeed

```csharp
public void SetRandomSeed(int seed)
```

Re-seeds `JCass_ModelCore.DomainModels.DomainModelBase.Rando`. Called by the framework during setup, using the seed from the model configuration.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `seed` | `int` | Seed value. |

**Remarks.** A domain model does not normally call this. Re-seeding mid-run makes the forecast depend on when you did it, which is the opposite of what a seed is for.

### SetupBase

```csharp
public void SetupBase(ModelBase model)
```

Wires this domain model to the framework model, seeds the random generator and then calls `JCass_ModelCore.DomainModels.DomainModelBase.SetupInstance`. Called by the framework immediately after it constructs your class.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | The framework model. |

**Remarks.** Do not call this yourself and do not override it. Put your own setup work in `JCass_ModelCore.DomainModels.DomainModelBase.SetupInstance`, which this method calls last and which exists for exactly that.

### SetupInstance

```csharp
public abstract void SetupInstance()
```

Your model's own setup: read lookups, load side-car coefficient files, build sub-models, and store them on your domain model object for the rest of the run to use. Called once, before any element is processed.

**Remarks.** This is the first place your code can touch the framework. The constructor cannot - `JCass_ModelCore.DomainModels.DomainModelBase.model` is still null there.

What IS ready when this runs. The framework deliberately loads the domain model last so that setup can use project data: `model.Lookups` and `model.MultiColumnLookups` are populated, treatment types and unit rates are loaded, the budget is set up, `model.Configuration` is available - including `WorkFolder`, which is what you combine with a `supporting/` path to find a side-car CSV - and `model.ParamNames` is populated, so `JCass_ModelCore.DomainModels.DomainModelBase.PIndex(System.String)` works.

What is NOT ready, and fails silently. The per-element data arrays are built after this method returns. `model.NElements`, `model.NPeriods` and `model.NParameters` are all still zero here - not null, not an error, just zero. Sizing an array off one of them during setup produces an empty array and a model that runs to completion with nothing in it. Read those in `Initialise` or later, where they are correct.

Guard every file you read and name the path in the exception message. A setup file that is missing and not guarded surfaces much later as a wrong number rather than as a missing file.
