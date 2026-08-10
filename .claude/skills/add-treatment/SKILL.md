---
name: add-treatment
description: Add a treatment to a Juno Cassandra domain model — the TreatmentNames constant, the bundle treatments row, the trigger, the Resetter case arm and the lookups rows. Five places, silent in four of them. Use for "add a treatment", "add a reseal", "the model needs an overlay".
---

# Add a treatment

**This skill is a wrapper.** Every step is a page in `docs/` plus a `jcass-dm` verb. Without it, do
the same job by reading
[`docs/workflow/30-make-a-change.md` § Add a treatment](../../../docs/workflow/30-make-a-change.md#add-a-treatment)
and running the verb it names.

## 0. Before the first step

- **Honour the verb** — [`docs/00-start-here.md` § 2](../../../docs/00-start-here.md). "Walk me
  through adding a treatment" means *they* make the five edits, one at a time, and this skill is not
  the way round that. An engineer who has never made this change cannot maintain the model.
- **Stop conditions apply** — [`docs/conventions/when-to-stop.md`](../../../docs/conventions/when-to-stop.md).
  Use the `draft-support-request` skill if one fires.

## 1. Read

- [`docs/workflow/30-make-a-change.md` § Add a treatment](../../../docs/workflow/30-make-a-change.md#add-a-treatment) — the five places, and the procedure.
- [`docs/patterns/treatment-instances.md`](../../../docs/patterns/treatment-instances.md) — the constructor, and why every argument from `quantity` on is named. **Take the signature from [`docs/framework/api/authoring/TreatmentInstance.md`](../../../docs/framework/api/authoring/TreatmentInstance.md), never from memory and never from another model.**
- [`docs/patterns/treatment-suitability-scoring.md`](../../../docs/patterns/treatment-suitability-scoring.md) — see § 4 below; this is not optional.
- [`docs/patterns/candidate-strategies.md`](../../../docs/patterns/candidate-strategies.md) — whether this treatment should be offered *alongside* an existing one.
- [`docs/patterns/routine-maintenance.md`](../../../docs/patterns/routine-maintenance.md) — if what they are adding is maintenance rather than a capital treatment.

## 2. The number in their sentence goes to `lookups.xlsx`

**This is the specific way this skill gets broken.** The engineer usually supplies the threshold in
the same breath as the request — *"a reseal that triggers on surfaces older than 12 years"*. Writing

```csharp
private const int ResealAgeYears = 12;   // WRONG
```

uses their number and still does the wrong thing: it locks a value the modeller could have owned
into a rebuild-and-republish cycle. A lookup row and a `Constants` property instead — the procedure
is [`docs/patterns/constants-from-lookups.md`](../../../docs/patterns/constants-from-lookups.md)
§ *Adding a number, end to end*, and the rule and its boundaries are
[`docs/conventions/where-numbers-live.md`](../../../docs/conventions/where-numbers-live.md).

The `add-lookup-constant` skill does that half.

## 3. The five places

Work the table in `workflow/30-make-a-change.md` § Add a treatment. The only one with a verb:

```powershell
.\tools\jcass-dm.exe add-treatment ..\MyRoadModel\domain_model_setup.xlsx `
    --name reseal --budget-category surfacing --description "Reseal"
```

**`--budget-category` must name a column that exists in the client's `inputs\budgets.xlsx`.** Get it
wrong and the run stops at setup naming the treatment — loud, but only after an upload and a wait.
`jcass-dm check` and the web app's **Check Setup** both find it sooner. You cannot see their budget
columns, so **ask** rather than choosing a plausible one.

Two things that are easy to miss, both called out on that page: adding the trigger method and
forgetting to **call it** from `GetTriggeredTreatments`, and the `Resetter` case arm.

## 4. The trigger stub sets `TreatmentSuitabilityScore`

The constructor leaves it at zero, and at zero the candidate is never preferred over a scored one
and **nothing reports that it was passed over** — the treatment simply never happens.
[`docs/patterns/treatment-suitability-scoring.md`](../../../docs/patterns/treatment-suitability-scoring.md).

The same page covers `RankParamSimple` for anything returned from routine maintenance: that list is
sorted by it, not optimised, so zero makes the funding order arbitrary in a way that looks
deliberate across runs.

**Scaffold the place and ask.** How this treatment should be ranked is a modelling decision — ask
what makes an element a good candidate for it, and put the weights and break points in
`lookups.xlsx` like every other tunable number. What is never acceptable is omitting the property
because the value is not known yet.

## 5. Ask, do not invent

| Ask | Never |
|---|---|
| The trigger conditions and their thresholds | A plausible age or condition limit |
| The unit rate, and what quantity it is priced per | A rate, or the assumption that quantity is the element's whole size |
| The budget category | A category name that sounds right |
| What the treatment does to the element in `Reset` | A condition reset value |
| How the candidate should be ranked | A scoring formula presented as standard |

## 6. Then

```powershell
dotnet build ..\MyRoadModel\MyRoadModel.csproj -c Debug --no-incremental
.\tools\jcass-dm.exe check --project ..\MyRoadModel --lookups ..\lookups.xlsx
```

`check` covers places 1, 2 and 4 mechanically, in both directions. **Places 3 and 5 it cannot
cover** — only the engineer knows that a treatment which never fires is wrong. Say that, and say to
watch for it at F5.

## 7. Never

- **Never emit a `const` for a threshold, rate or factor** — § 2.
- **Never leave `TreatmentSuitabilityScore` unset** on a capital candidate — § 4.
- **Never construct a `TreatmentInstance` from memory or from another model's call site.** The API
  reference is the authority.
- **Never publish** because the change is finished. Next is
  [`docs/workflow/20-upload-and-debug.md`](../../../docs/workflow/20-upload-and-debug.md).
