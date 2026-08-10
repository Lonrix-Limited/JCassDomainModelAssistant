# Treatment instances

**Compiling example:** [`TreatmentInstances.cs`](../../examples/ExamplesLibrary/TreatmentInstances.cs)
**API:** [`../framework/api/authoring/TreatmentInstance.md`](../framework/api/authoring/TreatmentInstance.md)

---

## When to reach for it

**Every time a trigger returns anything.** `TreatmentInstance` is the currency between a domain
model and the framework: your code creates them, the framework ranks them, funds what fits the
budget, and hands the winners back to `Reset`.

It is also **the single most error-prone thing a domain model does**, and the reason the generated
API reference exists at all.

---

## There is exactly one constructor, and it takes eight parameters

```csharp
public TreatmentInstance(
    int element_index,   // 1  zero-based, as the framework passed it in
    string name,         // 2  must match a treatment in the bundle
    int period,          // 3  1-based; zero or negative throws
    double quantity,     // 4  in the same unit as unitRate
    double unitRate,     // 5  cost per unit — from lookups, never a literal
    bool force,          // 6  bypass the ranking (not the budget)
    string reason,       // 7  exported; a modeller reads it
    string comment)      // 8  free text; no meaning to the framework
```

**No overloads.** If you have seen a seven-argument call, it came from a model written before May
2026 — unit rates moved off `TreatmentType` onto the instance then, and the older overload was
removed with them. Those call sites no longer compile.

> **This matters more than it sounds.** A domain model that no longer compiles against the current
> framework looks *exactly* like a working model when you read it. Copy a construction out of one
> and you get a build error if you are lucky and a wrong model if you are not. If a model you are
> reading does not build, disregard it entirely rather than working around it.
>
> **The API reference is the authority on this signature**, not any model, and not this page:
> [`../framework/api/authoring/TreatmentInstance.md`](../framework/api/authoring/TreatmentInstance.md).

---

## Use named arguments. Every time.

Look at the parameter list again. **Positions 4 and 5 are consecutive `double`s. Positions 6, 7 and
8 are a `bool` and two consecutive `string`s.** A call with the right values in the wrong order
compiles cleanly.

| Swap | What happens |
|---|---|
| `quantity` ↔ `unitRate` | The cost is wrong by orders of magnitude. The optimiser funds all the wrong things, and the run completes. |
| `reason` ↔ `comment` | The export reads as nonsense to whoever asks why an element was treated. Nothing errors. |

Named arguments cost nothing and turn both of those into a compile error:

```csharp
return new TreatmentInstance(
    segment.ElementIndex,
    TreatmentNames.Replace,
    period,
    quantity: segment.LengthMetres,
    unitRate: constants.GetUnitRate(TreatmentNames.Replace),
    force: false,
    reason: $"Condition {Math.Round(segment.ConditionGrade, 1)} > {constants.ReplaceConditionGreaterThan}",
    comment: $"Break rate {Math.Round(segment.BreakRatePerKmYear, 2)}/km/yr");
```

Real call sites in the working models mix positional and named freely. **Do not copy that.** Name
everything from `quantity` onwards — the first three are a different type each and are hard to get
wrong.

---

## The eight parameters, one at a time

### 1. `element_index`

Exactly the index the framework passed into the method you are in. Zero-based. Nothing else.

### 2. `name`

**Must match a treatment in the `treatments` sheet of `domain_model_setup.xlsx`.** The name is used
as a dictionary key whenever a cost is allocated or a row is exported, so a name with no matching
treatment type does **not** fail where the instance was created — it fails later, during costing or
export, with a message about a dictionary rather than about a typo in a trigger.

Keep the names as constants in one file (`TreatmentNames.cs`), and let `jcass-dm check` compare them
against the bundle before you upload. For a name assembled at run time, check it yourself:

```csharp
if (!frameworkModel.TreatmentTypes.ContainsKey(treatmentName))
{
    throw new Exception(
        $"Treatment '{treatmentName}' is not defined in the treatments sheet of domain_model_setup.xlsx. " +
        "Defined treatments: " + string.Join(", ", frameworkModel.TreatmentTypes.Keys.Order()) + ".");
}
```

### 3. `period`

**1-based.** The constructor throws on zero or negative, which is one of the few things it does
check.

> **A treatment placed beyond the last modelled period is discarded in silence.** The framework's
> append is wrapped in `if (treatment.TreatmentPeriod <= model.NPeriods)` **with no `else`**: not
> recorded, not costed, not warned about. A model that schedules a ten-period follow-up in a
> ten-period run loses it and reports nothing, so the forecast quietly assumes work that was never
> funded.
>
> Compare against `model.NPeriods` yourself and decide what should happen. Returning `null` — *there
> is no follow-up within the horizon* — is usually right. What is never right is scheduling it and
> assuming it happened.

### 4 and 5. `quantity` and `unitRate`

**Cost is `quantity × unitRate`, adjusted for discounting and inflation. Nothing checks that the two
agree on units.**

So the quantity is whatever the rate is priced in. A patch repair priced per metre of *pipe
repaired* takes the repaired length, not the segment length:

```csharp
double repairedLengthMetres = segment.LengthMetres * constants.RepairExtentFraction;
```

That fraction comes from `lookups.xlsx` like every other tunable number. Pass the whole segment
length instead and the cost is right by construction and wrong by engineering, which is the harder
kind of wrong to notice.

**The unit rate comes from lookups, never from a literal.** It is supplied per instance and is no
longer inherited from the treatment type. The Tuning page's *Treatment Rates* tab edits exactly this
set, so expect its values to change between runs with no code change at all:
[`constants-from-lookups.md`](constants-from-lookups.md).

Where one job draws on two budgets, the quantity and rate are handled differently — see
[`multi-budget-cost-split.md`](multi-budget-cost-split.md), and read it before improvising.

### 6. `force`

**Bypasses the ranking, not the budget.** In an MCDA model a forced treatment is assigned the
maximum rank parameter; in a benefit-cost model forced strategies are separated out and funded ahead
of the ranked ones. What it never does is create money.

Use it for interventions policy or safety requires. Do not use it to push through a treatment the
model would otherwise reject — that is how a model stops being evidence and starts being an
argument.

### 7. `reason`

Exported against every treatment as `treatment_reason`. **It is the only explanation anybody gets
when they ask why an element was treated**, so write it for a modeller and not for a developer, and
put the *values* that fired the rule in it rather than the rule's name:

```csharp
reason: $"Condition {Math.Round(segment.ConditionGrade, 1)} > {constants.ReplaceConditionGreaterThan}"
```

`"Condition trigger"` tells them nothing they could not already see.

### 8. `comment`

Free text, exported as `treatment_comment`. The framework attaches no meaning to it. Useful for the
secondary numbers that explain a decision without belonging in the reason.

---

## What the constructor does not do

Three things, each of which has caught somebody:

| Not set | Consequence | Set it via |
|---|---|---|
| `TreatmentSuitabilityScore` | In an MCDA model the candidate is never preferred over a scored one, and nothing reports that it was passed over | [`treatment-suitability-scoring.md`](treatment-suitability-scoring.md) |
| `RankParamSimple` | Maintenance funding order becomes whatever the element loop produced — stable enough to look deliberate | [`treatment-suitability-scoring.md`](treatment-suitability-scoring.md) |
| `Cost` | Zero until the framework multiplies quantity by unit rate and applies the present-worth factor | Nothing — the framework does it. Just do not read `Cost` straight after construction and expect a number |

---

## Properties the framework sets, and you must not

`IsCommitted`, `IsFollowUp` and `FollowUpWaitPeriods` are assigned by the framework — by the
committed-treatments loader and by its strategy generator respectively. Assigning them yourself
produces a model that disagrees with the framework about what it is doing. See
[`candidate-strategies.md`](candidate-strategies.md) for why a domain model does not build
multi-period strategies itself.

---

## Related

- [`constants-from-lookups.md`](constants-from-lookups.md) — where quantity factors and unit rates come from
- [`candidate-strategies.md`](candidate-strategies.md) — how many instances to return, and why more than one
- [`treatment-suitability-scoring.md`](treatment-suitability-scoring.md) — the two properties the constructor leaves at zero
- [`multi-budget-cost-split.md`](multi-budget-cost-split.md) — when one treatment's cost hits two budgets
- [`routine-maintenance.md`](routine-maintenance.md) — instances returned outside the optimiser
- [`../conventions/silent-failures.md`](../conventions/silent-failures.md) — the full list, including the modelling-horizon one above
- [`../workflow/30-make-a-change.md`](../workflow/30-make-a-change.md#add-a-treatment) — adding a treatment, all five places
