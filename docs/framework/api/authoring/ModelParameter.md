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

# ModelParameter

**Namespace:** `JCass_ModelCore.ModelObjects`  
**Assembly:** `JCass_ModelCore`  
**Kind:** class

> **Should a domain model use this?**  
> You read these. They are the model parameter definitions from the setup.

The definition of one model parameter: its name, type, allowed range and display precision. One of these per row of the model setup's parameters sheet.

**Remarks.** Model parameters are the values that evolve as the model runs - condition, age, treatment history - as distinct from raw input data, which never changes. A domain model reads these definitions through `model.Parameters`; it does not create them.

**Inherits:** `DataObject`  

## Constructors

### ModelParameter

```csharp
public ModelParameter(Dictionary<string, object> setupRow, int index)
```

Creates a parameter definition from a row of the setup's parameters sheet. Called by the framework.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `setupRow` | `Dictionary<string, object>` | The row. Must contain `parameter_name`, `data_type`, `minimum` and `maximum`; `decimals` is optional and defaults to 2. |
| 2 | `index` | `int` | Zero-based position of this parameter in the model's parameter list. |

**Throws.**

- `System.Exception` — Thrown, naming the column, if a required column is absent.

**Remarks.** A blank `minimum` or `maximum` cell is not an error and does not default to "unbounded" - it defaults to zero. See `JCass_ModelCore.ModelObjects.ModelParameter.Minimum`.

## Properties

### DataType

```csharp
public string DataType { get; set; }
```

The parameter's type as written in the setup: `"number"`, or anything else for text.

**Remarks.** Only the exact string `"number"` makes a parameter numeric. A misspelling such as "numeric" or "Number" silently produces a text parameter, and every later write of a numeric value to it goes through text conversion instead.

### Decimals

```csharp
public int Decimals { get; set; }
```

Decimal places used when this parameter is rounded for export and for KPI reporting. Defaults to 2 for numeric parameters, 0 for text.

**Remarks.** Affects exported and reported values only. The value held during the run is not rounded, so a forecast is not changed by this - only what you read afterwards.

### Index

```csharp
public int Index { get; set; }
```

Zero-based position of this parameter in the model's parameter list. This is what `DomainModelBase.PIndex` returns.

### IsNetworkStatistic

```csharp
public bool IsNetworkStatistic { get; set; }
```

True if this parameter holds a network-level statistic rather than a per-element value.

**Remarks.** Always false as constructed from a setup row; the framework sets it where it applies.

### IsNumeric

```csharp
public bool IsNumeric { get; set; }
```

True if `JCass_ModelCore.ModelObjects.ModelParameter.DataType` is exactly `"number"`.

### Maximum

```csharp
public double Maximum { get; set; }
```

Highest value this parameter may hold, enforced by clamping on every write. Defaults to zero when the setup leaves it blank - see `JCass_ModelCore.ModelObjects.ModelParameter.Minimum` for why that matters more than it appears to.

### Minimum

```csharp
public double Minimum { get; set; }
```

Lowest value this parameter may hold. Enforced by clamping on every write, not by validation - see the remarks.

**Remarks.** This is the framework's most dangerous default. Every numeric value written to a parameter is passed through `Math.Clamp` against this and `JCass_ModelCore.ModelObjects.ModelParameter.Maximum`. Both default to zero when the setup row leaves them blank, and nothing validates them at setup.

So a numeric parameter with blank minimum and maximum is clamped to the range zero-to-zero: every value written to it becomes 0, for every element, for every period, with no error and no warning. The model runs to completion and the parameter is flat.

A setup where minimum exceeds maximum is worse in a more useful way - `Math.Clamp` throws, so the run fails rather than lying.

Set a real range on every numeric parameter. If a parameter genuinely has no bounds, give it generous ones rather than leaving the cells empty.

### Name

```csharp
public string Name { get; set; }
```

Parameter name, as written in the setup. Domain models address parameters by this name, and it is matched exactly and case-sensitively.
