# Treatment suitability scoring

**Compiling example:** [`TreatmentSuitabilityScoring.cs`](../../examples/ExamplesLibrary/TreatmentSuitabilityScoring.cs)
**API:** [`../framework/api/authoring/TreatmentInstance.md`](../framework/api/authoring/TreatmentInstance.md) → `TreatmentSuitabilityScore`, `RankParamSimple`

---

## When to reach for it

**Whenever you return a candidate.** Both properties below default to zero, and **both fail silently
when left there.**

---

## Two properties, two different mechanisms

| Property | Applies to | Mechanism | Left at 0 |
|---|---|---|---|
| `TreatmentSuitabilityScore` | Capital candidates | One of the criteria an MCDA run ranks on, alongside cost and any others the project has weighted | The candidate is ranked on a score of zero, and nothing reports it |
| `RankParamSimple` | Routine maintenance | Maintenance is **sorted** by it, descending, and funded down the list | Every candidate compares equal, so funding order is whatever the element loop produced |

Read the second one again. **Maintenance is not optimised.** There is no cleverness behind it: the
framework sorts on `RankParamSimple` and spends until the maintenance budget runs out. So that value
is the whole of your control over what gets done first when maintenance money is short.

Left at zero the ordering is arbitrary — and it is *reproducibly* arbitrary, which is worse. It looks
deliberate across runs, so nobody questions it.

---

## Why a class of its own

Three of the four working models keep scoring in a separate file, and the reasons hold:

- it is a **modelling decision in its own right**, and a reviewer wants to read it without the
  trigger's conditions around it;
- it is used from **more than one trigger**;
- it is the thing most likely to be revisited during calibration.

---

## The shape

### One rank, several curves

```csharp
public static double GetReplaceScore(PipeSegment segment, PipeConstants constants, PipeSubModels subModels)
{
    double need = GetNeedRank(segment, constants);
    return subModels.ReplaceSuitabilityCurve.GetValue(need);
}

public static double GetRelineScore(PipeSegment segment, PipeConstants constants, PipeSubModels subModels)
{
    double need = GetNeedRank(segment, constants);
    return subModels.RelineSuitabilityCurve.GetValue(need);
}
```

**One definition of need, and a curve per treatment.** Replacement climbs with need; relining peaks
in the middle band, because a segment too good does not need it and one too far gone cannot be
helped by it. Both curves are [piecewise-linear models](piecewise-linear-models.md) built at setup
from break points in `lookups.xlsx`, so a modeller reshapes either of them without a code change.

### Score this treatment on this element

**A score belongs to a pairing: how right is this treatment for this element, right now?** Two
treatments proposed on one element each get their own score.

Two shortcuts lose that. A fixed score per treatment makes every element look the same to the
ranking. A score that is only the element's condition gives every treatment on it the same number,
and turns the ranking into worst-first.

Here the input to every curve is the same underlying rank, and each treatment has its own curve, so
the score depends on both the element and the treatment. What makes a treatment suitable is the
engineer's judgement. The framework prescribes no formula and no scale.

### The weights are tunable; the scales are not

```csharp
const double worstGrade = 5.0;
const double bestGrade = 1.0;
const double rankScaleMaximum = 100.0;

double conditionFraction = (segment.ConditionGrade - bestGrade) / (worstGrade - bestGrade);
conditionFraction = Math.Clamp(conditionFraction, 0.0, 1.0);

double weightedTotal = constants.ConditionWeight * conditionFraction
                     + constants.CriticalityWeight * segment.CriticalityScore;
```

*"How much should consequence-of-failure count relative to condition?"* is the question a calibration
workshop argues about for an hour. It must not need a developer to answer, so both weights come from
`lookups.xlsx` through [`Constants`](constants-from-lookups.md).

The **scales** — condition grade running 1 to 5, the rank running 0 to 100 — are structural. They are
part of the data's definition, not a calibration choice, and changing one would break the
correspondence with the curve rather than change the forecast. Named constants in C#, with a comment
saying so. [`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md).

### Guard the degenerate case

```csharp
if (weightSum <= 0)
{
    throw new Exception(
        "condition_weight and criticality_weight in lookup set 'scoring_weights' are both zero, " +
        "so no candidate can be ranked. Set at least one of them above zero.");
}
```

Both weights zeroed divides by zero and produces `NaN` on every candidate. When the score has a weight
in an MCDA run, the framework stops the run with an error naming the element and the treatment, but it
cannot name the lookup rows that caused it. Name the set and both keys here.

---

## The maintenance side

```csharp
public static void SetMaintenancePriority(
    TreatmentInstance maintenance,
    PipeSegment segment,
    PipeConstants constants)
{
    maintenance.RankParamSimple = GetNeedRank(segment, constants);
}
```

Anything that expresses urgency in the domain will do — severity, a condition index, exposure,
cost-effectiveness. Using the same need rank the capital candidates use keeps **one definition of
urgent** across the model, which is worth something when somebody asks why maintenance went to one
segment and renewal to another.

`RankParamSimple` has no effect on capital candidates, which are ranked by the optimiser instead.

---

## The configured minimum

```csharp
public static bool IsWorthOffering(double score, ModelBase frameworkModel)
    => score > frameworkModel.Configuration.MinimumTreatmentSuitabilityScoreAllowed;
```

The project sets `Minimum Treatment Suitability Score` in its configuration. **The framework does not
apply it. Your domain model does, or nothing does.** A model that never reads the setting ignores it,
and no error says so. The reference model and scaffolded models do not check it, so add the check if
the project relies on the setting.

**Follow what the framework's `DefaultDomainModel` does:**

- **The score must be strictly greater than the minimum.** A score equal to it is left out, so with a
  minimum of 0 an unscored candidate is left out too.
- **Forced treatments skip the check.** Test `force` first, then the score.
- **Check it in the trigger and return early** rather than adding the candidate. The trigger also runs
  in benefit-cost models, where this keeps candidates out of the strategy rollout, which is worth real
  time on a large network.

---

## Related

- In the web app, **Help** → *The Treatment Suitability Score* and *The MCDA model* — what the score
  means to a modeller, how candidates are ranked, and how to see the scores from a run

- [`treatment-instances.md`](treatment-instances.md) — the properties the constructor leaves at zero
- [`candidate-strategies.md`](candidate-strategies.md) — where the scores get set
- [`piecewise-linear-models.md`](piecewise-linear-models.md) — the curves, and why the scoring ones extrapolate
- [`constants-from-lookups.md`](constants-from-lookups.md) — where the weights and break points come from
- [`routine-maintenance.md`](routine-maintenance.md) — the other half of `RankParamSimple`
- [`../framework/api/authoring/ModelConfiguration.md`](../framework/api/authoring/ModelConfiguration.md) — `MinimumTreatmentSuitabilityScoreAllowed`
