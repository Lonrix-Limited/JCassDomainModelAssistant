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

# DistributionSimulator

**Namespace:** `JCass_ModelCore.MonteCarlo`  
**Assembly:** `JCass_ModelCore`  
**Kind:** class

> **Should a domain model use this?**  
> Yes, in Monte Carlo models — you build these at setup from a coefficients CSV.

Draws a random value for one parameter from a distribution chosen by which cohort an element falls into. The workhorse of a Monte Carlo model's deterioration.

**Remarks.** Cohorts are defined in a setup file: each row gives a label, a rule that decides whether an element belongs to that cohort, and a piecewise-linear description of the distribution's shape. At simulation time the element's data is matched against the rules, and a value is drawn from the matching cohort's distribution.

Cohort order is priority order. Rules are evaluated in the order the setup file lists them and the first match wins, so a broad catch-all rule placed above a specific one silently takes every element that would have matched the specific one. Order the setup file from most specific to most general.

Build these once in `SetupInstance` and keep them on your domain model. Constructing one parses every cohort rule and builds a curve per cohort, which is not work to repeat per element per period.

The shape is expressed as a distribution's inverse: a uniform random number between 0 and 1 goes in as the x value, and the curve maps it to a parameter value. So the curve is the quantile function of whatever distribution was fitted, usually produced in R or Python and delivered as a CSV.

## Constructors

### DistributionSimulator

```csharp
public DistributionSimulator(string parameterName, jcDataSet setupData)
```

Constructs a DistributionSimulator for a given parameter, using setup data that defines cohorts with rules and associated piecewise linear models for the distribution shapes.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `parameterName` | `string` | The name of the parameter for which the distribution simulator is being created. Used only for error messages and logging. |
| 2 | `setupData` | `jcDataSet` | The setup data defining cohorts, rules, and associated piecewise linear models for the distribution shapes. Should have columns: "cohort_label", "cohort_rule", "cohort_shape". |

## Methods

### GetSimulatedValue

```csharp
public double GetSimulatedValue(Dictionary<string, object> dataRow, Random randomizer)
```

Draws a value for one element: works out which cohort it belongs to, then samples that cohort's distribution.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `dataRow` | `Dictionary<string, object>` | The element's data, against which the cohort rules are evaluated. Must contain every column the rules reference. |
| 2 | `randomizer` | `Random` | Random number generator to draw from. |

**Returns.** The simulated value.

**Throws.**

- `System.Exception` — Thrown, naming the parameter, if no cohort rule matches the row, if a rule references a column the row does not have, or if the value cannot be sampled.

**Remarks.** Pass the framework's random generator - `model.Random`, or a domain model's own `Rando` - and not a newly constructed one. Both are seeded from the model configuration, which is what makes a Monte Carlo run reproducible; a fresh `Random` is seeded from the clock, and the run silently stops giving the same answer twice.
