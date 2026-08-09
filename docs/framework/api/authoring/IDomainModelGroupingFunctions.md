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

# IDomainModelGroupingFunctions

**Namespace:** `JCass_ModelCore.DomainModels`  
**Assembly:** `JCass_ModelCore`  
**Kind:** interface

> **Should a domain model use this?**  
> Only if your model groups elements (for example, treating a whole road section as one candidate). Optional.

*The framework carries no `<summary>` for this type. The signatures below come
from the assembly metadata and are authoritative; the description is not available.*

## Methods

### FinaliseGroupTreatments

```csharp
public virtual string FinaliseGroupTreatments(
    ModelBase model,
    string groupName,
    int iPeriod,
    List<int> elementIndexesForGroup,
    List<TreatmentInstance> triggeredTreatments)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | — |
| 2 | `groupName` | `string` | — |
| 3 | `iPeriod` | `int` | — |
| 4 | `elementIndexesForGroup` | `List<int>` | — |
| 5 | `triggeredTreatments` | `List<TreatmentInstance>` | — |

> Positional order matters and 5 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.
