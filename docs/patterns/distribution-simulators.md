# Distribution simulators

**Compiling example:** [`DistributionSimulators.cs`](../../examples/ExamplesLibrary/DistributionSimulators.cs)
**API:** [`../framework/api/authoring/DistributionSimulator.md`](../framework/api/authoring/DistributionSimulator.md)
**Where the cohort file lives:** [`setup-data-from-supporting-csv.md`](setup-data-from-supporting-csv.md)

---

## When to reach for it

**When deterioration is a distribution rather than a rate**, and the model is meant to produce a
spread of outcomes rather than a single line.

A deterministic model says *"the break rate increases by this much per year"*. A Monte Carlo model
says *"the break rate increases by an amount drawn from **this** distribution, and **which**
distribution depends on which cohort this element falls into"*. `DistributionSimulator` is the
framework's answer to the second, and it is the workhorse of every Monte Carlo model in the corpus.

Use it for increments, and use it for **resets** too — a treatment's effect varies as much as
deterioration does, and a model that simulates the decay but resets to a fixed value understates
the spread of outcomes while looking stochastic.

---

## What it actually is

The setup file gives, per cohort, three things:

| Column | Is |
|---|---|
| `cohort_label` | A name, for error messages |
| `cohort_rule` | An expression deciding whether an element belongs to this cohort |
| `cohort_shape` | A piecewise description of the distribution's shape |

**The shape is the distribution's inverse.** A uniform random number between 0 and 1 goes in as the
x value, and the curve maps it to a parameter value. So `cohort_shape` is the *quantile function* of
whatever was fitted — which is exactly what R's `quantile()` or numpy's `percentile()` give you, and
why these files normally arrive from a statistician as a CSV rather than being written by hand.

---

## The shape

### Build them at setup

```csharp
public static void LoadBreakRateSimulators(PipeSubModels subModels, string workFolder)
{
    subModels.BreakRateIncrementSimulator =
        BuildSimulator("break_rate_increment", workFolder, BreakRateIncrementFile);
}

private static DistributionSimulator BuildSimulator(string parameterName, string workFolder, string fileName)
{
    jcDataSet setupData = SetupDataFromSupportingCsv.ReadSupportingCsv(workFolder, fileName);

    setupData.CheckRequiredColumns(
        new List<string> { "cohort_label", "cohort_rule", "cohort_shape" },
        throwErrorIfNotFound: true);

    return new DistributionSimulator(parameterName, setupData);
}
```

**Once, in `SetupInstance`, and then keep them.** Constructing one parses every cohort rule and
builds a curve per cohort. Doing that per element per period produces identical numbers and takes
orders of magnitude longer, so the only symptom is the clock.

`parameterName` is used **only in error messages**. Make it the name a modeller would recognise, not
a variable name — it is what they will see when a rule fails to match.

### Draw a value

```csharp
Dictionary<string, object> cohortInputs = new Dictionary<string, object>
{
    { "material", segment.MaterialType },
    { "diameter_mm", segment.DiameterMm },
    { "age", segment.Age },
};

return subModels.BreakRateIncrementSimulator.GetSimulatedValue(cohortInputs, frameworkModel.Random);
```

---

## Three ways this goes wrong, in order of how quietly

### 1. A new `Random` — and the run stops being reproducible

```csharp
simulator.GetSimulatedValue(inputs, new Random());   // WRONG
```

**Pass `model.Random`** — or `Rando` on `DomainModelBase`, which is the same object. It is seeded
from the model configuration, and that seeding is the whole of what makes a Monte Carlo run
reproducible. A freshly constructed `Random` is seeded from the clock.

Nothing errors. Nothing warns. The model simply stops giving the same answer twice, and a forecast
that cannot be reproduced cannot be defended.

### 2. Cohort order is priority order

Rules are evaluated **in the order the setup file lists them, and the first match wins.**

A broad catch-all rule placed above a specific one silently takes every element the specific rule
was written for. The run completes, with the wrong distribution applied to a whole class of
elements, and the only way to see it is to notice that the specific cohort never fires.

**Order the file most specific to most general**, and say so in a comment row or in the file's own
documentation — the person who edits it next is a modeller, not the person who wrote it.

### 3. A rule references a column the dictionary does not carry

The rules are **text in the CSV**, so nothing checks this at compile time. A rule mentioning
`diameter_mm` against a dictionary without it throws, naming the parameter, at the first draw — which
is loud, but is several minutes into a run and after a refit has already been signed off.

The dictionary must carry **every column any rule references**, including rules for cohorts this
element will not match.

---

## Related types

Same idea, different mechanics — check the API page before choosing:

| Type | For |
|---|---|
| [`DistributionSimulator`](../framework/api/authoring/DistributionSimulator.md) | A continuous value drawn from a cohort-specific distribution |
| [`MarkovTransitionSimulator`](../framework/api/authoring/MarkovTransitionSimulator.md) | Condition that steps through **discrete states**, from a transition probability matrix |
| [`NormalGenerator`](../framework/api/authoring/NormalGenerator.md) | A plain normally-distributed draw, where no cohort structure is needed |
| [`LinearRegressionModel`](../framework/api/authoring/LinearRegressionModel.md) | A fitted mean **plus** a fitted residual spread — `PredictWithRandomError` |

All four take the framework's random generator, and all four have the same reproducibility trap.

---

## Related

- [`setup-data-from-supporting-csv.md`](setup-data-from-supporting-csv.md) — where the cohort file goes, and the guard around reading it
- [`constants-from-lookups.md`](constants-from-lookups.md) — calibration factors applied to a simulated value belong here, not in C#
- [`piecewise-linear-models.md`](piecewise-linear-models.md) — the curve type `cohort_shape` is expressed in
- [`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md) — why the cohort file is not forty rows of `lookups.xlsx`
