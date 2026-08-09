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

# MarkovTransitionSimulator

**Namespace:** `JCass_ModelCore.MonteCarlo`  
**Assembly:** `JCass_ModelCore`  
**Kind:** class

> **Should a domain model use this?**  
> Yes, in Monte Carlo models that step condition through discrete states.

Simulates the next state from a discrete-state Markov transition matrix.

The setup data is a square matrix supplied as a `JCass_Data.Objects.jcDataSet`. The first column (header literally `state`, lowercase) holds the current-state keys (one per row). Every other column header is a next-state key. Cell values are transition probabilities expressed as fractions in [0, 1] (e.g. 0.65, not 65). Blank cells are treated as 0. Each row's probabilities must sum to 1.0 (checked to four decimal places). The set of row keys and the set of next-state column headers must be identical (square + consistent).

Two sampling modes are available:

`JCass_ModelCore.MonteCarlo.MarkovTransitionSimulator.GetNextState(System.String,System.Random)` — homogeneous. Every element in the same current state draws from the identical row distribution.

`JCass_ModelCore.MonteCarlo.MarkovTransitionSimulator.GetNextState(System.String,System.Double,System.Random)` — calibrated per-element. The caller supplies an already-computed per-element leave-probability `p_leave_i ∈ [0, 1]` and the simulator uses it directly for the stay-vs-leave draw. On "leave", the destination is sampled from the row's conditional-on-leaving distribution (excludes the stay state, renormalised). The row's baseline leave-probability `p_s = 1 - p(s → s)` is exposed via `JCass_ModelCore.MonteCarlo.MarkovTransitionSimulator.GetBaselineLeaveProbability(System.String)` so the caller can construct `p_leave_i` (typically `clamp(p_s × raw_p_i / mean_p_s, 0, 1)`). Provided the caller ensures the mean of `p_leave_i` across the cohort in state s equals `p_s`, the aggregate row leave-rate is preserved in expectation.

Both overloads are allocation-free in the hot path: a couple of dictionary lookups, a small linear scan, and a returned reference to a cached column-header string.

## Constructors

### MarkovTransitionSimulator

```csharp
public MarkovTransitionSimulator(string parameterName, jcDataSet setupData)
```

Constructs a Markov transition simulator from a setup dataset.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `parameterName` | `string` | Logical name of the parameter being simulated. Used only in error messages. |
| 2 | `setupData` | `jcDataSet` | Square transition matrix; see class summary for the expected shape. |

## Methods

### GetBaselineLeaveProbability

```csharp
public double GetBaselineLeaveProbability(string currentStateKey)
```

Returns the row baseline leave-probability `p_s = 1 - p(s → s)` for the given state, precomputed at setup. Callers of `JCass_ModelCore.MonteCarlo.MarkovTransitionSimulator.GetNextState(System.String,System.Double,System.Random)` use this as the scaling factor when constructing a calibrated per-element leave-probability. Returns 0.0 (or a value below `JCass_ModelCore.MonteCarlo.MarkovTransitionSimulator.BaselineLeaveEpsilon`) for absorbing states.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `currentStateKey` | `string` | Key of the current state. Must match one of the row keys defined in setup. |

**Returns.** The row baseline leave-probability in [0, 1].

### GetNextState — overload 1 of 2

```csharp
public string GetNextState(string currentStateKey, Random randomizer)
```

Samples the next state given the current state and a random source. Allocation-free.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `currentStateKey` | `string` | Key of the current state. Must match one of the row keys defined in setup. |
| 2 | `randomizer` | `Random` | Random source. Caller is responsible for seeding/lifetime. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** The sampled next-state key (reference to the cached column header).

### GetNextState — overload 2 of 2

```csharp
public string GetNextState(string currentStateKey, double elementLeaveProbability, Random randomizer)
```

Samples the next state given the current state, a caller-supplied per-element leave-probability, and a random source. Allocation-free.

A first uniform draw decides stay-vs-leave against `elementLeaveProbability`; on a "leave" outcome a second uniform draw samples the destination from the row's conditional-on-leaving distribution (excludes the stay state, renormalised). Absorbing states (`baseline_s ≈ 0`) always stay regardless of the passed probability — this is a defensive short-circuit for callers that mistakenly pass a non-zero value for an absorbing state.

The caller is responsible for computing `elementLeaveProbability` so that it preserves the row's aggregate leave-rate in expectation. Typical construction: `p_leave_i = clamp(p_s × raw_p_i / mean_p_s, 0, 1)` where `p_s` is fetched via `JCass_ModelCore.MonteCarlo.MarkovTransitionSimulator.GetBaselineLeaveProbability(System.String)`, `raw_p_i` is an external per-element risk score (e.g. a logistic prediction), and `mean_p_s` is the sample mean of those scores across the current cohort in state `currentStateKey`. Under that construction the mean of `p_leave_i` is `p_s`, and the aggregate row transition rate is preserved in expectation.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `currentStateKey` | `string` | Key of the current state. Must match one of the row keys defined in setup. |
| 2 | `elementLeaveProbability` | `double` | Per-element probability of transitioning out of `currentStateKey` in this step. Must be in [0, 1] and finite; already-clamped by the caller. |
| 3 | `randomizer` | `Random` | Random source. Caller is responsible for seeding/lifetime. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** The sampled next-state key (reference to a cached column header).
