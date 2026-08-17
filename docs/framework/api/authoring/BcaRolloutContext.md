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

# BcaRolloutContext

**Namespace:** `JCass_ModelCore.ModelObjects`  
**Assembly:** `JCass_ModelCore`  
**Kind:** sealed class

> **Should a domain model use this?**  
> **Almost never.** The period you are handed is already the real modelling period, in a strategy rollout exactly as in the main run, so period-based logic is correct without ever looking at this.
>  
> Reached as `model.CurrentRollout`, which is null whenever the model is not inside a BCA strategy rollout. Its own remarks name the only two cases that genuinely differ: declining to do something that can never be placed, because `AbsolutePeriod` has run past `model.NPeriods` and the rollout is looking beyond the end of the model; and logic that needs to know how deep into the look-ahead it is rather than which calendar period it is in, which is `RelativePeriod`. Never cache the instance — it is replaced on every step of every rollout and set back to null when the rollout ends. `TryResolveStrategyEpoch` is the framework's own rule for where a parameter read is answered from; you do not call it, but it is what throws when a domain model reads an epoch no strategy can answer for.

Where a domain model call sits inside a BCA strategy rollout, for the rare case where knowing that changes what the domain model should do.

**Remarks.** Most domain models should ignore this and never read it. Every period a domain model is given - in a rollout exactly as in the main run - is the real modelling period, so period-based logic works without consulting this. It exists for the two cases that genuinely differ:

- Deciding not to do something that can never be placed, because the rollout looks past the end of the model. `JCass_ModelCore.ModelObjects.BcaRolloutContext.AbsolutePeriod` beyond `model.NPeriods` says so.
- Logic that needs to know how far into the look-ahead it is rather than which calendar period it is in - `JCass_ModelCore.ModelObjects.BcaRolloutContext.RelativePeriod`.

Read it through `model.CurrentRollout`, which is null whenever the model is not inside a rollout. A domain model that wants to behave identically in both contexts simply does not look.

Do not cache the instance. It is replaced on every step of every rollout, and set back to null when the rollout ends.

## Constructors

### BcaRolloutContext

```csharp
public BcaRolloutContext(
    int elementIndex,
    int basePeriod,
    int relativePeriod,
    int absolutePeriod,
    ITreatmentLookup treatmentLookup,
    StrategyParameterData parameterData)
```

Creates a context. Called by the framework's strategy generator.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `elementIndex` | `int` | Zero-based index of the element being rolled out. |
| 2 | `basePeriod` | `int` | The modelling period the strategy is based in. |
| 3 | `relativePeriod` | `int` | Position within the look-ahead, counting from 1 at the base period. |
| 4 | `absolutePeriod` | `int` | The real modelling period. |
| 5 | `treatmentLookup` | `ITreatmentLookup` | The strategy's treatment view - see `JCass_ModelCore.ModelObjects.BcaRolloutContext.TreatmentLookup`. |
| 6 | `parameterData` | `StrategyParameterData` | The strategy branch's parameter timeline - see `JCass_ModelCore.ModelObjects.BcaRolloutContext.ParameterData`. |

> Positional order matters and 6 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

## Properties

### AbsolutePeriod

```csharp
public int AbsolutePeriod { get; }
```

The real modelling period, equal to `JCass_ModelCore.ModelObjects.BcaRolloutContext.BasePeriod` + `JCass_ModelCore.ModelObjects.BcaRolloutContext.RelativePeriod` - 1. This is the value passed to the domain model as `iPeriod`, and it can exceed the model's period count near the end of a run.

### BasePeriod

```csharp
public int BasePeriod { get; }
```

The modelling period the strategy is based in - the period whose treatment would actually be placed if this strategy were selected.

### ElementIndex

```csharp
public int ElementIndex { get; }
```

Zero-based index of the element being rolled out.

### ParameterData

```csharp
public StrategyParameterData ParameterData { get; }
```

This strategy branch's own parameter timeline, indexed by period relative to `JCass_ModelCore.ModelObjects.BcaRolloutContext.BasePeriod`. This is what the framework's parameter helpers read during the rollout.

**Remarks.** A domain model does not normally touch this - it gets the same answers through `model.GetParameterValues` and friends, which resolve it automatically. See `JCass_ModelCore.ModelObjects.BcaRolloutContext.TryResolveStrategyEpoch(System.Int32,System.Int32,System.Int32@)` for the rule.

Held by reference. The generator fills it in as the rollout proceeds, so each period's values become visible here as soon as the domain model has written them.

### RelativePeriod

```csharp
public int RelativePeriod { get; }
```

Position within the look-ahead, counting from 1 at `JCass_ModelCore.ModelObjects.BcaRolloutContext.BasePeriod`.

### TreatmentLookup

```csharp
public ITreatmentLookup TreatmentLookup { get; }
```

What this strategy has done to the element so far, laid over what the model has actually placed. This is what the framework's placeholder helpers read during the rollout.

**Remarks.** A domain model does not normally touch this - it gets the same answers through `model.GetSpecialPlaceholderValues`, which resolves it automatically. See `JCass_ModelCore.Treatments.RolloutTreatmentLookup` for what is and is not visible through it.

## Methods

### TryResolveStrategyEpoch

```csharp
public bool TryResolveStrategyEpoch(int iElem, int iEpoch, out int strategyEpoch)
```

Decides where a model parameter read should be answered from while this rollout is running, and refuses the read outright when nothing can answer it correctly.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iElem` | `int` | Zero-based index of the element whose parameter is being read. |
| 2 | `iEpoch` | `int` | Epoch being read, absolute - the same epoch numbering the model uses outside a rollout. |
| 3 | `strategyEpoch` | `int` | Set to the matching relative epoch in `JCass_ModelCore.ModelObjects.BcaRolloutContext.ParameterData` when the return value is true; zero otherwise. |

**Returns.** `true` to read `JCass_ModelCore.ModelObjects.BcaRolloutContext.ParameterData` at `strategyEpoch`, `false` to read the model's real parameter data at `iEpoch`.

**Throws.**

- `System.InvalidOperationException` — Thrown when no correct answer exists - see the remarks. This is deliberate: the alternative is the uninitialised zeros the caller would otherwise be handed without a word.

**Remarks.** The rule, in one line:an epoch the strategy has already passed comes from the strategy; anything earlier comes from the real data; anything later exists nowhere.

- Any element, epoch before `JCass_ModelCore.ModelObjects.BcaRolloutContext.BasePeriod` - real data. This is history the main run has already computed, and it is the same for every strategy.
- The element being rolled out, from `JCass_ModelCore.ModelObjects.BcaRolloutContext.BasePeriod` up to the last period this rollout has completed - the strategy's own timeline. Two sibling strategies legitimately give different answers here, which is the point.
- Anything else - refused. For the rolled-out element that means its own current or future period, which the strategy has not decided yet. For any other element it means a period the main run has not stepped, so the real array holds zeros rather than data. A strategy is a what-if about one element; no other element has a hypothetical timeline.

Note the asymmetry with `JCass_ModelCore.Treatments.RolloutTreatmentLookup`. There, a query about another element is passed straight through to the real treatment set and that is correct, because the real set is merely sparse. Here it is refused, because the real parameter array is empty ahead of the main run. Same-looking rule, opposite conclusion.
