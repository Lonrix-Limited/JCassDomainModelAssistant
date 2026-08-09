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

*The framework carries no `<summary>` for this type. The signatures below come
from the assembly metadata and are authoritative; the description is not available.*

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

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `dataRow` | `Dictionary<string, object>` | — |
| 2 | `randomizer` | `Random` | — |
