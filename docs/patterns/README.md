# Patterns

**The canonical shape of each recurring piece of a real model.** Ten of them, mined from the domain
models already running in production and rewritten in a neutral fictional domain — a buried water
main network — so that no client's calibration travels with the shape.

Every example on these pages is a real file in
[`../../examples/ExamplesLibrary/`](../../examples/ExamplesLibrary/), and that project is built in
CI. An example that does not compile rots, and a rotted example is worse than no example because it
gets quoted with confidence.

> **The examples library is a library, not a template.** Do not copy the folder and rename it. To
> start a model, run `jcass-dm scaffold` — [`../workflow/10-scaffold-and-build.md`](../workflow/10-scaffold-and-build.md).

---

## Which page

| You are | Read |
|---|---|
| Writing anything with a number in it | [`constants-from-lookups.md`](constants-from-lookups.md) — **start here; every other page depends on it** |
| Loading a fitted set of coefficients from a CSV | [`setup-data-from-supporting-csv.md`](setup-data-from-supporting-csv.md) |
| Building a Monte Carlo model's stochastic deterioration | [`distribution-simulators.md`](distribution-simulators.md) |
| Turning attributes into a probability from a fitted logistic model | [`logistic-coefficients.md`](logistic-coefficients.md) |
| Expressing a relationship as a curve a modeller can shape | [`piecewise-linear-models.md`](piecewise-linear-models.md) |
| Constructing a treatment | [`treatment-instances.md`](treatment-instances.md) — **the most error-prone thing a domain model does** |
| Splitting one treatment's cost across two budget categories | [`multi-budget-cost-split.md`](multi-budget-cost-split.md) — rare, non-obvious, high consequence |
| Deciding what the optimiser gets to choose between | [`candidate-strategies.md`](candidate-strategies.md) |
| Telling the optimiser how badly a candidate is wanted | [`treatment-suitability-scoring.md`](treatment-suitability-scoring.md) |
| Modelling work that happens outside the capital budget | [`routine-maintenance.md`](routine-maintenance.md) |

---

## Two rules that run through all ten

### 1. No example hard-codes a tunable number

Every threshold and every rate in every example is read from `inputs\lookups.xlsx` through the
`Constants` pattern. Not as a stylistic flourish: **these pages are what gets copied**, so a single
`const` here would quietly undo the rule everywhere downstream.

Where a number legitimately stays in C# — a scale endpoint, a unit conversion, a structural
count — the example names it and says in a comment why it is structural. The distinction, and the
test for it, is [`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md).

### 2. Guard before you index, and name what was missing

Every read of a lookup set, a lookup key, a CSV column or a budget category in these examples is
preceded by a check, and every check throws a message naming the set and the key, or the file and
the column.

**This is a rule in its own right, not a defensive habit.** The alternative is one of two failures,
and both are worse than they look:

| Instead of a guard | What the modeller gets |
|---|---|
| Indexing straight into the dictionary | `KeyNotFoundException` naming nothing, part way through a run |
| Falling back to a default | A run that completes with a silently wrong number in it |

A typo in a spreadsheet is the most common failure in this whole system, and it is one the modeller
can fix themselves in seconds **provided they are told which cell**. Everything about the guard idiom
follows from that.

Two mechanics that go with it:

- **`Convert.ToDouble`, never a cast.** `setting_value` arrives as **text** regardless of how the
  cell looks in Excel. `(double)set[key]` throws an `InvalidCastException` that says nothing about
  a spreadsheet.
- **Check existence before reading, not after.** For a file, `File.Exists` first, then read. One
  working model has these the wrong way round, and the result is that a missing side-car CSV
  surfaces as whatever the CSV reader happens to throw instead of as "this file is not in
  `supporting\`".

---

## Where these sit

| Tier | Answers |
|---|---|
| [`../conventions/`](../conventions/) | *Should* it be this way — the rules, and what fails silently |
| [`../workflow/`](../workflow/) | *When* do I do it — the end-to-end procedures |
| **`patterns/`** (here) | *How* is it written — the shape, and why it is that shape |
| [`../framework/api/`](../framework/api/README.md) | *What is the exact signature* — generated, authoritative |

These pages link into the API reference rather than restating it. If a signature here and a
signature there ever disagree, **the API reference is right** — it is generated from the assemblies
you are compiling against.
