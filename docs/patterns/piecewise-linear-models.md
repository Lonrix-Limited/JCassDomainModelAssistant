# Piecewise linear models

**Compiling example:** [`PiecewiseLinearModels.cs`](../../examples/ExamplesLibrary/PiecewiseLinearModels.cs)
**API:** [`../framework/api/authoring/PieceWiseLinearModel.md`](../framework/api/authoring/PieceWiseLinearModel.md)

---

## When to reach for it

**When the relationship is a shape rather than a formula.**

Most relationships a modeller wants to calibrate are not naturally an equation. They are *flat until
here, then it climbs, then it saturates*. A piecewise-linear model says exactly that, and its entire
definition is a short string — which means it can live in `lookups.xlsx` or in a `supporting\` CSV
instead of in C#.

Three common uses, and all three are in the working models:

- a **scoring curve** — mapping a rank or an index to a suitability score;
- a **distribution shape** — `cohort_shape` in a [`DistributionSimulator`](distribution-simulators.md) setup file is one of these;
- a **fitted relationship** with more break points than anybody wants to read as a string.

---

## The setup-code string

```
x,y|x,y|x,y
```

Pairs separated by pipes, x and y by a comma. **x values must be ascending and unique**; whitespace
around the parts is tolerated. So `"1,0|3,50|5,100"` is a curve that runs from 0 at x=1, through 50
at x=3, to 100 at x=5.

That compactness is the point: the whole curve is one spreadsheet cell.

---

## Three constructors, and which to use when

`PieceWiseLinearModel` is one of the few framework types that genuinely has overloads. Check
[the API page](../framework/api/authoring/PieceWiseLinearModel.md) for the exact signatures; the
choice between them is:

| Overload | Use when |
|---|---|
| `PieceWiseLinearModel(string setupString, bool canExtrapolate)` | The curve is a handful of break points, held as a string in `lookups.xlsx` or assembled from `Constants` |
| `PieceWiseLinearModel(List<double> x, List<double> y, bool canExtrapolate)` | The curve came out of a fit with more points than are readable as a string — build the lists from a `supporting\` CSV |
| `PieceWiseLinearModel()` | Parameterless. Pairs with `SetupFromXYPairs` for deferred construction; rarely what you want |

The first two produce the same object. The difference is only where the numbers are readable, and
that is a question about who maintains them.

---

## The extrapolation flag is a modelling decision, not a default

`canExtrapolate` is the second argument, and it is easy to pass without thinking about.

| Value | Outside the fitted range |
|---|---|
| `false` | Returns the nearest end value — the curve goes flat |
| `true` | The end gradient continues |

**`false` is right for something fitted over a finite range.** Beyond the data you have no evidence,
and inventing some by continuing a gradient is how a model produces a negative roughness or a
condition grade of 9.

**`true` is right for a scoring curve you defined rather than fitted**, and the reason is specific:
with `false`, every element below the first break point gets the *same* score. A scoring curve that
returns identical values for a whole band of elements hands the optimiser a mass of ties it has to
break arbitrarily, so the ordering within that band comes from element index rather than from
anything modelled. Extrapolating keeps them separated.

The example uses both, deliberately, one per curve.

---

## Building a curve from constants, not from literals

This is the shape to copy when the curve is a **policy** rather than a fit:

```csharp
string replaceSetup =
    $"{constants.ReplaceConditionGreaterThan},{ScoreMinimum}|{RankScaleMaximum},{ScoreMaximum}";

subModels.ReplaceSuitabilityCurve = new PieceWiseLinearModel(replaceSetup, canExtrapolate: true);
```

Break points come from [`Constants`](constants-from-lookups.md), so a modeller reshapes the curve on
the Tuning page and re-runs. Only the **scale endpoints** are in C#, because they are structural —
changing them would break the correspondence with the rank the curve is fed, not change the
forecast. They are named rather than inlined so the next reader can see the choice was deliberate.

> **Why assemble the string here rather than store the whole string in one lookup row?**
> Because then each break point is its own named, individually editable value. A modeller nudging
> one break point edits one cell. The alternative asks them to retype
> `3.2,0|100,100` correctly, with the punctuation, and a stray space or a missing pipe is a setup
> failure rather than a different curve. Store the whole string only when the curve is genuinely
> atomic — a fitted distribution shape, for instance, where nobody nudges one point.

---

## Building a curve from a CSV

```csharp
List<double> xValues = new List<double>();
List<double> yValues = new List<double>();

for (int iRow = 0; iRow < setupData.Count; iRow++)
{
    Dictionary<string, object> row = setupData.Row(iRow);
    xValues.Add(SetupDataFromSupportingCsv.GetNumber(row, xColumn, fileName, iRow));
    yValues.Add(SetupDataFromSupportingCsv.GetNumber(row, yColumn, fileName, iRow));
}

if (xValues.Count < 2)
{
    throw new Exception($"'{fileName}' needs at least two rows to define a curve.");
}

return new PieceWiseLinearModel(xValues, yValues, canExtrapolate);
```

**The rows must be in ascending x order, and x must be unique.** The constructor rejects both
violations — which is worth knowing, because a CSV sorted by something else, or exported with a
duplicated boundary point, looks perfectly reasonable in Excel and fails at setup with a message
about lists rather than about the file.

The one-point check is there because the constructor's own objection would also be about lists. One
point is not a curve, and the modeller needs to hear that about their file.

---

## Reading a value

```csharp
double score = curve.GetValue(rank);
```

There is nothing more to it, and that is the point: all the calibration is in the curve, so the call
site stays readable and the modeller owns the shape.

`GetMinimumX`, `GetMaximumX`, `GetMinimumValue` and `GetMaximumValue` are there when you need to
normalise against the curve's own range.

---

## Related

- [`constants-from-lookups.md`](constants-from-lookups.md) — where break points come from
- [`setup-data-from-supporting-csv.md`](setup-data-from-supporting-csv.md) — where a fitted curve's points come from
- [`treatment-suitability-scoring.md`](treatment-suitability-scoring.md) — the most common use of a curve in a real model
- [`distribution-simulators.md`](distribution-simulators.md) — `cohort_shape` is a curve in this format
- [`../framework/api/authoring/PieceWiseLinearModelGeneric.md`](../framework/api/authoring/PieceWiseLinearModelGeneric.md) — the same idea over explicit x/y lists, in `JCass_Core.JFunctions`
