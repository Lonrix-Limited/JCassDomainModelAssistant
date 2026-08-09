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

Optional interface for domain models whose elements are treated in groups rather than individually - a whole road section resurfaced together, say, rather than segment by segment.

**Remarks.** Implement this in addition to inheriting `DomainModelBase`, and only when the model setup defines a grouping column. Most domain models do not need it.

Grouping changes what the optimiser is choosing between: whole groups rather than single elements. The framework still asks your triggers for candidates element by element, then hands each group's candidates back here for a final decision.

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

Decides what actually happens to a group, given every treatment its elements triggered. Called once per group per period, after triggering and before optimisation.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `model` | `ModelBase` | The framework model. |
| 2 | `groupName` | `string` | The group's key, from the setup's grouping column. |
| 3 | `iPeriod` | `int` | Modelling period. |
| 4 | `elementIndexesForGroup` | `List<int>` | Zero-based indexes of every element in the group, including those that triggered nothing. |
| 5 | `triggeredTreatments` | `List<TreatmentInstance>` | The treatments the group's elements triggered. Modify this list in place to change what the group is put forward for. |

> Positional order matters and 5 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

**Returns.** The treatment name settled on for the group, for reporting.

**Remarks.** This is where a group's competing candidates are reconciled - typically by choosing one treatment for the whole group and extending it to elements that did not trigger it themselves, so the group is treated as a unit.

The list is the output. The return value is a label; what the model actually does is whatever is left in `triggeredTreatments` when this returns. Returning a treatment name without having changed the list changes nothing.
