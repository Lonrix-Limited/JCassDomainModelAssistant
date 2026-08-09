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

# IDomainModel

**Namespace:** `JCass_ModelCore.DomainModels`  
**Assembly:** `JCass_ModelCore`  
**Kind:** interface

> **Should a domain model use this?**  
> **Yes — this is the contract.** The framework calls these methods on your model.
>  
> You will not normally implement this interface directly; you inherit `DomainModelBase`, which implements it for you. Read it to understand what the framework will call, and when.

The contract between the framework and a domain model: the methods the framework will call, and when.

**Remarks.** Read this to understand the shape of a model run. You do not normally implement it directly - inherit `JCass_ModelCore.DomainModels.DomainModelBase`, which implements it and adds the helpers a domain model needs.

The framework finds your class by looking through the assembly you supply for a public, non-abstract type implementing this interface with a parameterless constructor, and matching its name against the class name in the model setup. That is why the entry class must be public, must not be abstract, and must not require constructor arguments.

The methods correspond to the stages of a modelling period: setup once, then per element - initialise, trigger candidates, apply increments or resets - and once per period at the end.

## Methods

### DoEndOfPeriodCalculations

```csharp
public virtual void DoEndOfPeriodCalculations(int iPeriod)
```

Stub for the Domain Model that can be used to perform any end of period calculations after the treatment selection and parameter updates have been performed for all elements in the current period. This can be used to calculate any additional parameters that are needed for the next period or for reporting purposes, using the updated parameter values after treatment application. This method is called from the Framework Model at the end of each period, after all elements have been processed for the current period. You can use this to do things such as calculating network level rankings, statistics, proportions over/under etc. that can be used to drive decisions in the next period. Implementers should store calculated values in the Domain Model object. Take care on how you index or store results. Inless you index by period, your values will be replaced/recycled at the end of each period.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iPeriod` | `int` | Modelling period (values like 1,2,...n) |

### GetTreatmentCandidates

```csharp
public virtual List<TreatmentInstance> GetTreatmentCandidates(
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
public virtual TreatmentInstance GetTriggeredMaintenance(
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
public virtual void Increment(
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
public virtual void Initialise(
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

### Reset

```csharp
public virtual void Reset(
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
public virtual void SetRandomSeed(int seed)
```

Sets the random seed for the model. This is used to control the randomness in the model, ensuring reproducibility of results.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `seed` | `int` | — |

### SetupBase

```csharp
public virtual void SetupBase(ModelBase model)
```

Method in which you can set up your model, for example, loading machine learning models or other resources that are needed for the model to run.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | — |
