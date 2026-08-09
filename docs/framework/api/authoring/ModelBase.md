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

*The framework carries no `<summary>` for this type. The signatures below come
from the assembly metadata and are authoritative; the description is not available.*

## Properties

### Configuration

```csharp
public ModelConfiguration Configuration { get; }
```

*No framework documentation for this member.*

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

*No framework documentation for this member.*

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

*No framework documentation for this member.*

### NParameters

```csharp
public int NParameters;
```

Number of model parameters

## Methods

### GetInputDataNumber

```csharp
public double GetInputDataNumber(int elementIndex, string header)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `elementIndex` | `int` | — |
| 2 | `header` | `string` | — |

### GetInputDataText

```csharp
public string GetInputDataText(int elementIndex, string header)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `elementIndex` | `int` | — |
| 2 | `header` | `string` | — |

### GetLookupValueNumber

```csharp
public double GetLookupValueNumber(string lookupSetName, string lookupKey)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `lookupSetName` | `string` | — |
| 2 | `lookupKey` | `string` | — |

### GetLookupValueText

```csharp
public string GetLookupValueText(string lookupSetName, string lookupKey)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `lookupSetName` | `string` | — |
| 2 | `lookupKey` | `string` | — |

### GetParameterValues

```csharp
public ValueTuple<Dictionary<string, double>, Dictionary<string, string>> GetParameterValues(int iElem, int iEpoch)
```

Gets dictionaries of all parameter values for an element at an epoch - both numeric and text parameters held in separate dictionaries

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iElem` | `int` | Zero-based element index |
| 2 | `iEpoch` | `int` | Epoch for which to get parameter values |

**Returns.** Keys are StringComparer.Ordinal for faster lookup

### GetSpecialPlaceholderValues

```csharp
public Dictionary<string, object> GetSpecialPlaceholderValues(int iElem, int period, TreatmentInstance treatment = null)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iElem` | `int` | — |
| 2 | `period` | `int` | — |
| 3 | `treatment` | `TreatmentInstance` | — |
