# Candidate strategies

**Compiling example:** [`CandidateStrategies.cs`](../../examples/ExamplesLibrary/CandidateStrategies.cs)
**API:** [`../framework/api/authoring/StrategySetupInfo.md`](../framework/api/authoring/StrategySetupInfo.md), [`../framework/api/authoring/TreatmentInstance.md`](../framework/api/authoring/TreatmentInstance.md)

---

## When to reach for it

**Whenever you write a trigger** — which is to say, whenever you write the part of the model that
decides what work is due.

This page is about the *shape of the list you return*, and that shape is where nearly all of a
domain model's influence over the optimiser lives.

---

## Read this before anything else: you return candidates, the framework builds the strategies

The natural assumption is the opposite one, and acting on it produces code that does not fit the
framework at all.

In a benefit-cost run the framework:

1. calls your `GetTreatmentCandidates` for each element and each period;
2. hands the list to **its own** strategy generator, which rolls each candidate forward into
   multi-period strategies — *do it now*, *do it in three periods*, *do nothing*;
3. scores the strategies and funds what fits the budget.

**A domain model does not assemble a `TreatmentStrategy`.** That type is on the framework's
*recognise it, do not construct it* list — [`../framework/api/referenced.md`](../framework/api/referenced.md) —
for exactly this reason.

> **You may find a `StrategyGenerator.cs` in an older model.** Both of the ones in the models
> available as evidence are dead code: one is excluded from its own `.csproj` and calls a
> `TreatmentInstance` constructor that no longer exists, and the other is never called from
> anywhere. Do not take a shape from either.

---

## So the leverage is entirely in what the list contains

**Return one candidate and you have reduced the optimiser to a yes/no funding decision.** Return
two — a cheap holding action alongside the permanent fix — and it can trade elements off against each
other under a budget, which is the whole reason to run an optimiser rather than a sorted list. It is
also what a benefit-cost model needs in order to have anything to compare.

That is the pattern, and in the example it is one method:

```csharp
private static void AddReplaceAsAlternativeIfValid(...)
{
    bool holdingActionTriggered = candidates.Exists(
        c => c.TreatmentName == TreatmentNames.PatchRepair || c.TreatmentName == TreatmentNames.Reline);

    bool replaceAlreadyOffered = candidates.Exists(c => c.TreatmentName == TreatmentNames.Replace);

    if (!holdingActionTriggered || replaceAlreadyOffered) return;

    TreatmentInstance treatment = TreatmentInstances.BuildReplace(segment, constants, period);
    treatment.Reason = "Alternative to the triggered holding action";
    treatment.TreatmentSuitabilityScore = TreatmentSuitabilityScoring.GetReplaceScore(segment, constants, subModels);

    candidates.Add(treatment);
}
```

Note the second condition. **A segment already bad enough to have triggered a replacement on its own
account gets no second option** — there is no genuine alternative to offer, and adding one only pads
the strategy count and slows the run down. Offering choices is not the same as offering more rows.

---

## The composition shape

```csharp
public static List<TreatmentInstance> GetCandidates(...)
{
    List<TreatmentInstance> candidates = new List<TreatmentInstance>();

    AddPatchRepairIfValid(segment, constants, subModels, period, candidates);
    AddRelineIfValid(segment, constants, subModels, period, candidates);
    AddReplaceIfValid(segment, constants, subModels, period, candidates);
    AddReplaceAsAlternativeIfValid(segment, constants, subModels, period, candidates);

    return candidates;
}
```

**One small `Add...IfValid` per treatment, composed in a readable sequence.** Every working model
converges on this, and it is worth keeping when the rules get complicated, because the alternative
is a nest of conditions no reviewer can check — and this is the file a reviewer reads first, since it
is where the engineering judgement lives.

Each `Add...` method **guards early and returns**, rather than nesting:

```csharp
if (segment.ConditionGrade <= constants.RepairConditionGreaterThan) return;
if (segment.ConditionGrade > constants.RelineConditionGreaterThan) return;
```

Every threshold from [`Constants`](constants-from-lookups.md). There should not be one numeric
literal in a trigger file, and that is the property to preserve as the model grows.

**Order matters** where a later rule reads what an earlier one added — as the alternative does above.
Say so in a comment when it does.

---

## Two things that are easy to get wrong

### Return an empty list, never `null`

For most elements in most periods, nothing is due. That is the normal case, not a failure, and an
empty list is how you say it.

### Score every candidate

`TreatmentSuitabilityScore` is left at zero by the constructor. In an MCDA model an unscored
candidate is never preferred over a scored one, and **nothing reports that it was passed over** — the
treatment simply never happens. [`treatment-suitability-scoring.md`](treatment-suitability-scoring.md).

---

## When the project defines strategies in its setup

Some projects define named multi-treatment strategies in the setup's strategies sheet. Where they
exist, the framework exposes them as `model.StrategiesSetupData` — a list of
[`StrategySetupInfo`](../framework/api/authoring/StrategySetupInfo.md), each naming a first treatment
and up to three follow-ups with wait periods. The list is **empty** in projects that do not use them.

A domain model reads them to decide **which of the project's defined strategies are worth offering
on a given element**:

```csharp
foreach (StrategySetupInfo strategy in frameworkModel.StrategiesSetupData)
{
    if (!IsStrategyApplicable(strategy, segment, constants)) continue;

    firstTreatments.Add(strategy.FirstTreatment);

    if (strategy.ForceFirstTreatment) break;
}
```

Two conventions in that loop:

- **The setup lists strategies in priority order**, so iterate in order rather than filtering.
- **The first forced strategy wins and you stop.** A forced strategy is a decision already taken;
  continuing to offer alternatives below it is a contradiction.

**Reading them is still not building them.** What you return is a candidate for the first treatment.
The framework's generator handles the rollout, including the wait periods, and sets `IsFollowUp` and
`FollowUpWaitPeriods` itself.

The only part a domain model genuinely owns here is the applicability rule — *does this strategy
suit this element* — and that is domain judgement, which is why it gets its own method.

---

## A related trap: scheduling your own follow-up

If you do return a candidate for a future period yourself, **check it against the horizon**:

```csharp
if (followUpPeriod > frameworkModel.NPeriods) return null;
```

A treatment beyond the last modelled period is discarded without a word — not recorded, not costed,
not warned about. Details in [`treatment-instances.md`](treatment-instances.md).

---

## Related

- [`treatment-instances.md`](treatment-instances.md) — constructing each candidate correctly
- [`treatment-suitability-scoring.md`](treatment-suitability-scoring.md) — ranking them, and the silent failure if you do not
- [`constants-from-lookups.md`](constants-from-lookups.md) — where every threshold in a trigger comes from
- [`routine-maintenance.md`](routine-maintenance.md) — work that does not go through this path at all
- [`../framework/concepts/07-bca-model.md`](../framework/concepts/07-bca-model.md) — what the optimiser does with what you return
- [`../workflow/30-make-a-change.md`](../workflow/30-make-a-change.md#add-a-treatment) — adding a treatment, all five places
