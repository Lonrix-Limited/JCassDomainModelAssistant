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

# StrategySetupInfo

**Namespace:** `JCass_ModelCore.Treatments`  
**Assembly:** `JCass_ModelCore`  
**Kind:** class

> **Should a domain model use this?**  
> Only in models that generate multi-treatment strategies for benefit-cost optimisation.

The definition of one treatment strategy: a first treatment plus up to three follow-ups, each placed a stated number of periods later. One of these per row of the strategies setup sheet.

**Remarks.** Only benefit-cost models use strategies. Where an MCDA model triggers individual treatments and lets the optimiser rank them, a BCA model evaluates whole sequences over a look-ahead period and compares their costs and benefits.

A domain model reads these through `model.StrategiesSetupData` when generating candidates. It does not create them - they come from the setup.

The shape is fixed at four treatments, and unused slots are left blank rather than omitted. A strategy of one treatment still has `treat2` through `treat4` columns; they are simply empty. That is why every property below always exists.

## Constructors

### StrategySetupInfo

```csharp
public StrategySetupInfo(Dictionary<string, object> setupRow)
```

Creates a strategy definition from a row of the strategies setup sheet. Called by the framework.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `setupRow` | `Dictionary<string, object>` | The row. All twelve columns must be present, including those for unused treatment slots. |

**Throws.**

- `System.Collections.Generic.KeyNotFoundException` — Thrown if any of the twelve columns is absent.
- `System.FormatException` — Thrown if a wait period or force flag cannot be read as a number or a boolean. A blank wait period or force cell fails here, so unused slots need `0` and `FALSE` rather than empty cells.

**Remarks.** Treatment names are trimmed of surrounding whitespace; nothing else is. Names are not checked against the treatment types here.

## Properties

### FirstTreatment

```csharp
public string FirstTreatment { get; set; }
```

Treatment applied at the start of the strategy. Must match a treatment type name.

### ForceFirstTreatment

```csharp
public bool ForceFirstTreatment { get; set; }
```

True if the first treatment must be placed regardless of how the strategy ranks economically. Forced strategies are funded ahead of ranked ones.

### StrategyName

```csharp
public string StrategyName { get; set; }
```

Name of the strategy, used as its key in the setup and in strategy debug output.

### Treat2Force

```csharp
public bool Treat2Force { get; set; }
```

True if the second treatment must be placed regardless of ranking.

### Treat2Name

```csharp
public string Treat2Name { get; set; }
```

Second treatment in the sequence, or an empty string if the strategy has only one.

### Treat2WaitPeriod

```csharp
public int Treat2WaitPeriod { get; set; }
```

How many periods after the first treatment the second one falls.

**Remarks.** A wait that pushes a treatment past the last modelled period means it is never placed, and nothing reports that - see `TreatmentSet.AppendTreatmentAndReduceBudget`. Check long waits against the run's period count.

### Treat3Force

```csharp
public bool Treat3Force { get; set; }
```

True if the third treatment must be placed regardless of ranking.

### Treat3Name

```csharp
public string Treat3Name { get; set; }
```

Third treatment in the sequence, or an empty string if unused.

### Treat3WaitPeriod

```csharp
public int Treat3WaitPeriod { get; set; }
```

How many periods after the first treatment the third one falls.

### Treat4Force

```csharp
public bool Treat4Force { get; set; }
```

True if the fourth treatment must be placed regardless of ranking.

### Treat4Name

```csharp
public string Treat4Name { get; set; }
```

Fourth treatment in the sequence, or an empty string if unused.

### Treat4WaitPeriod

```csharp
public int Treat4WaitPeriod { get; set; }
```

How many periods after the first treatment the fourth one falls.
