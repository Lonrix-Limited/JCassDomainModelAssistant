# Routine maintenance

**Compiling example:** [`RoutineMaintenance.cs`](../../examples/ExamplesLibrary/RoutineMaintenance.cs)
**API:** [`../framework/api/authoring/TreatmentInstance.md`](../framework/api/authoring/TreatmentInstance.md) → `RankParamSimple`, `IsRoutineMaintenance`

---

## When to reach for it

**When work happens whether or not there is capital budget for it.**

Pothole filling, crack sealing, flushing, jetting, make-safe work. The network does it regardless,
so it does not belong in the pool of candidates the optimiser chooses between.

---

## Why modelling it matters more than it looks

Routine maintenance is usually **the largest single consequence of deferring renewal**. Defer the
replacement and the emergency repairs go up every year afterwards, for as long as the asset stays in
service.

A model that leaves maintenance out makes doing nothing look free — which is the exact conclusion an
asset-management model exists to disprove. It is not a rounding-error line item; it is the mechanism
by which the do-nothing option gets its price.

---

## Where it sits in the run

`GetTriggeredMaintenance` is called **after** the optimiser has chosen and funded capital
treatments, once per element per period. Maintenance does not compete with capital work. It is
charged against its own budget category, sorted by `RankParamSimple`, and funded down the list until
that budget runs out.

Return **`null`** when none is due. For most elements in most periods that is the answer.

---

## The shape

```csharp
public static TreatmentInstance? GetTriggeredMaintenance(
    PipeSegment segment,
    PipeConstants constants,
    int period)
{
    if (segment.ConditionGrade <= constants.FlushConditionGreaterThan) return null;

    double severity = segment.ConditionGrade / constants.FlushConditionGreaterThan;
    double quantity = segment.LengthMetres * severity;

    TreatmentInstance maintenance = new TreatmentInstance(
        segment.ElementIndex,
        TreatmentNames.Flush,
        period,
        quantity: quantity,
        unitRate: constants.GetUnitRate(TreatmentNames.Flush),
        force: false,
        reason: $"Condition {Math.Round(segment.ConditionGrade, 1)} > {constants.FlushConditionGreaterThan}",
        comment: $"Severity factor {Math.Round(severity, 2)}");

    TreatmentSuitabilityScoring.SetMaintenancePriority(maintenance, segment, constants);

    return maintenance;
}
```

Four things in it.

### 1. The quantity is not automatically the element's size

Maintenance effort scales with **how bad** the element is as much as with how big it is. Here the
quantity is a condition-weighted length rather than the raw length, and the weighting is a ratio of
two values that both come from lookups — so no literal appears.

Whatever you choose, it must be in the unit the rate in `lookups.xlsx` is priced in. The two are
multiplied and **nothing anywhere checks that they agree**.

### 2. Set the priority

`RankParamSimple` is the only control over what gets done first when the maintenance budget is
short. Left at zero, every candidate compares equal and the order is whatever the element loop
produced. [`treatment-suitability-scoring.md`](treatment-suitability-scoring.md).

### 3. Its budget category still has to exist

Maintenance is charged like any other treatment, so its `budget_category` in the bundle needs a
matching column in the client's `inputs\budgets.xlsx`.

**That one the framework does check**, at setup, naming the treatment — so it is loud, unlike the
[multi-category case](multi-budget-cost-split.md). Loud is better than silent, but a run that dies at
setup still costs the engineer a round trip, so let `jcass-dm check` find it before you upload.

### 4. `null` means no maintenance

The framework's caller treats the result as nullable, but the abstract signature it overrides is
not annotated as such. In the entry class that means:

```csharp
return RoutineMaintenance.GetTriggeredMaintenance(element, this.Constants, iPeriod)!;
```

The `!` is how you say "no maintenance" without the compiler objecting. It is in the scaffolded
project for the same reason.

---

## The other half: what a capital treatment does to maintenance

**Easy to forget, and it does not error.**

A segment that has just been relined or replaced should not carry its maintenance history into the
next period — for maintenance purposes it is a new pipe. Reset that state in the **resetter**, not in
the maintenance trigger, so that the trigger stays a pure function of the segment's current
condition.

Miss it and treated segments go on generating maintenance at their pre-treatment rate for the rest
of the run. That makes renewal look **less** worthwhile than it is — a bias in exactly the direction
that is hardest to notice, because it makes the model conservative rather than absurd.

```csharp
switch (treatment.TreatmentName)
{
    case TreatmentNames.Replace:
        segment.BreakRatePerKmYear = 0;
        segment.ConditionGrade = constants.ConditionAfterReplace;
        break;

    case TreatmentNames.PatchRepair:
        segment.ConditionGrade *= constants.ConditionFactorAfterRepair;
        segment.BreakRatePerKmYear *= constants.ConditionFactorAfterRepair;
        break;

    // ... an arm per treatment ...

    default:
        throw new Exception(
            $"No reset defined for treatment '{treatment.TreatmentName}'. " +
            "Every treatment the trigger can return needs an arm here.");
}
```

> **The `default` arm that throws is the pattern, not decoration.** A treatment added to the trigger
> but not to the resetter is funded, applied, and has **no effect at all** — the element deteriorates
> on as if the money had never been spent, and nothing reports it. `jcass-dm check` has a rule for
> exactly this, and the throwing default is the belt to its braces.

---

## Reading the configured maintenance treatment name

```csharp
public static bool IsRoutineMaintenance(TreatmentInstance treatment, ModelBase frameworkModel)
    => treatment.IsRoutineMaintenance(frameworkModel.Configuration);
```

The framework knows which treatment is *the* routine maintenance one — it is named in the model's
meta setup and exposed as `model.Configuration.RoutineMaintenanceTreatmentName`. Comparing against
that rather than against your own constant is how post-processing and the framework's own reporting
stay in agreement with the model about which spending was maintenance.

---

## Stateless or stateful?

The example is a `static` class, because it needs nothing cached at setup. **The working models split
on this**: one keeps a stateful maintenance modeller, built in `SetupInstance`, because its
maintenance extent is simulated from a fitted distribution rather than derived from condition.

Start static. Move it to an instance class the moment it needs something built at setup — a
[distribution simulator](distribution-simulators.md) or a
[logistic probability model](logistic-coefficients.md) — and pass the framework model into the
constructor at that point.

---

## Related

- [`treatment-instances.md`](treatment-instances.md) — the construction, parameter by parameter
- [`treatment-suitability-scoring.md`](treatment-suitability-scoring.md) — `RankParamSimple`, and why zero is not neutral
- [`constants-from-lookups.md`](constants-from-lookups.md) — where the threshold, the rate and the reset factors come from
- [`candidate-strategies.md`](candidate-strategies.md) — the capital path, which this deliberately does not use
- [`distribution-simulators.md`](distribution-simulators.md) — for a stochastic maintenance extent
- [`../conventions/silent-failures.md`](../conventions/silent-failures.md) — including the missing reset arm
