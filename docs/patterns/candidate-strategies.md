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

## What a rollout tells your domain model

Rolling a candidate forward is not something the framework does to a data structure. **It calls your
model again** — `GetTreatmentCandidates`, `GetTriggeredMaintenance`, `Increment`, `Reset` — once per
look-ahead period, on every branch of every strategy, for every element that triggered anything. That
is why the length of the list you return is a run-time cost and not just a modelling choice.

Four things about those calls are not obvious, and all four have caught somebody out.

### The period you are handed is the real modelling period, and it can run past the end of the model

Not a position in the look-ahead. A strategy based in period 9 with a look-ahead of 20 calls your
model for periods 9, 10, 11 … exactly as the main run would, so any rule keyed on the calendar is
correct inside a rollout without you doing anything.

The consequence to plan for is at the other end: **`iPeriod` can exceed `model.NPeriods`.** A strategy
based in period 20 of a 20-period run still evaluates periods 21 upward, because a strategy has to be
valued over its whole life to be comparable with a cheaper one. That is deliberate and correct — no
treatment is ever placed out there — but it means:

- **`iPeriod > model.NPeriods` is not an error**, and code that treats it as one will fail runs near
  the horizon that were doing nothing wrong;
- anything sized or indexed by the horizon needs to tolerate it. Reading a per-period array you
  allocated as `new double[model.NPeriods + 1]` at `iPeriod` will throw only for the elements
  unlucky enough to trigger late in the run, which is a bug that hides until a client's data finds it.

If your rules genuinely need to know, `model.NPeriods` is the comparison to make.

### `previous_treatments` describes the strategy, not the network

`model.GetSpecialPlaceholderValues` is resolved for you during a rollout. `previous_treatments`
reports **what the strategy being evaluated has done to this element so far**, laid over the model's
real history from before the strategy's base period.

This is what makes a *periods since last treatment* rule work inside a rollout. Without it, a strategy
that resurfaced an element in its base period would be free to resurface it again the next period,
because as far as your trigger could see nothing had been done.

### `next_treatment_*` still describes committed treatments only

Deliberately unchanged, and not an oversight. The only treatments ahead of the current period are
**committed** ones — real, and applied by the rollout when it reaches them. The strategy's own future
does not exist yet at the moment your model is called: a rollout marches forward one period at a time,
so it has decided its own past and nothing of its own future.

### Reading model parameters has a rule, and breaking it is silent everywhere else

**The highest epoch you may read is `iPeriod - 1`.** Inside a rollout the framework resolves the read
against the strategy's own timeline and throws by name when it cannot answer. **Outside a rollout —
every other model type — the same mistake returns zeros with no word of complaint.**

The full rule, both halves, and the asymmetry with treatments that catches people:
[`../conventions/silent-failures.md` § 12](../conventions/silent-failures.md#12-reading-a-model-parameter-at-or-above-the-period-you-were-handed).

### And the escape hatch you almost certainly do not need

`model.CurrentRollout` is non-null only inside a rollout
([`../framework/api/authoring/BcaRolloutContext.md`](../framework/api/authoring/BcaRolloutContext.md)).
**A model that behaves identically in both contexts simply does not look at it**, and that is the
normal case — the period you were handed is already right. The two genuine uses are declining to do
something that could never be placed, because `AbsolutePeriod` has run past `model.NPeriods`, and
logic that needs its depth into the look-ahead rather than the calendar period. Never cache the
instance; it is replaced on every step of every rollout.

---

## How a treatment is actually selected

Worth reading once, because three of the four points below change how you would design a trigger, and
none of them can be worked out from the outputs.

**1. Only the first treatment of a strategy is ever placed.** Always, and always in the strategy's
base period. The rest of the strategy never becomes a commitment. Next period the model re-triggers
that element from its new condition and decides again, against a network that has moved on.

**2. So a strategy is a device for valuing its first treatment, not a plan.** What the framework is
really asking is *"if you pick treatment X as the first move, how good is the path that follows?"* The
benefit-cost ratio in the debug export is the answer to that question and nothing else.

**3. There are two different cost figures, deliberately.** What the budget gives up is the **first
treatment's** cost — that is what the optimiser weighs and what affordability is tested against. The
strategy's benefit-cost ratio uses the **whole strategy's** lifecycle cost against the baseline. Both
are correct; they answer different questions. So a treatment costing 10 inside a strategy costing 20
at a ratio of 2.0, and one costing 50 inside a strategy costing 200 at a ratio of 2.5, are both
legitimately in the race — one is cheap now, the other is better value over its life.

**4. Selection is a Pareto-front walk, not a ranking.** Candidates on an element are ordered along
that element's own front, and the optimiser repeatedly takes the highest *incremental* benefit-cost
step available across all elements. Two consequences an engineer will otherwise get wrong:

- **A strategy's position depends on which other strategies exist on the same element.** Two
  candidates on one element never compete on standalone merit; they are successive steps on one
  frontier, and the cheaper is simply reached first. This is the real argument for returning a cheap
  holding action alongside the permanent fix — it gives the front a step to take.
- **No column in any export can tell you why something was picked.** Not a gap to be filled in;
  selection is a frontier walk and is not reproducible from a sorted list.

> **What the strategy debug export is for.** Confirming that rollouts came out as expected, that the
> benefit — the area under the objective curve — is computed correctly, and that costs and deltas
> make sense. **It is not a record of what was selected**, and its sort order is indicative only. On
> its own it says nothing about which strategies reached the front or got funded.

### Two log warnings about the benefit-cost ratio

Both are about a strategy compared against its baseline, and both are worth recognising rather than
escalating:

- **"Strategy cheaper than the baseline…"** — **legitimate, not an error.** Treating early can avoid
  more maintenance than the treatment costs, so the strategy strictly dominates and is reported with
  the maximum ratio. It is the *expected* result for a good treatment when `BCA Benefit Relative to
  Maintenance-Only` is on. The only thing to check is that the baseline you intended is the one
  configured.
- **"Strategy costs the same over its life as the baseline…"** — **usually a real setup fault.** A
  treatment with a zero quantity, or a unit rate that resolved to zero, so the strategy costs nothing
  over the baseline. Its ratio is reported as zero. Check the treatment's quantity and its unit rate
  in `lookups.xlsx`.

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
- [`../conventions/silent-failures.md` § 12](../conventions/silent-failures.md#12-reading-a-model-parameter-at-or-above-the-period-you-were-handed) — the parameter epoch rule, and the zeros you get outside a rollout
- [`../framework/api/authoring/BcaRolloutContext.md`](../framework/api/authoring/BcaRolloutContext.md) — the rollout escape hatch, and why you almost never want it
- [`../workflow/30-make-a-change.md`](../workflow/30-make-a-change.md#add-a-treatment) — adding a treatment, all five places
