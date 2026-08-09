# Where the numbers live

**Three places, and the boundaries between them matter as much as the rule itself.**

| Kind of number | Where it goes |
|---|---|
| A **tunable scalar** — a trigger age, a condition limit, a unit rate | `inputs\lookups.xlsx`, read through the `Constants` pattern |
| A **set of fitted coefficients** — a regression, a logistic curve, a per-material table | A CSV in the client's `supporting\` folder, loaded once at setup |
| **Structure** — array bounds, unit conversions, sentinels | C#, as a properly named constant |

---

## The rule, and why it is not a style preference

**A tunable number never goes in C#.**

A number hard-coded in C# is a number the modeller cannot change without a developer, a rebuild and
a republish. A number in `lookups.xlsx` is one they change themselves on the web app's **Tuning**
page and re-run in minutes.

That is the whole distance between a model somebody can calibrate and a model they can only file
tickets against. Recalibration is not an occasional event — moving a trigger from age 15 to age 12,
looking at the result, and trying age 13 *is* the work. A model that makes each of those a
developer request does not get recalibrated; it gets abandoned or, worse, trusted while stale.

**The worked example is [`Constants.cs`](../../reference-model/DomainModelSample/Objects/Constants.cs)
in the reference model.** How `lookups.xlsx` is structured, how a value is addressed by
(set name, key), why `Convert.ToDouble` is not optional, and why you guard before you index, are all
explained once in
[`DomainModelSample/README.md` § 7](../../reference-model/DomainModelSample/README.md#7-where-the-numbers-live--read-this-before-you-write-a-threshold).
Read it before writing a threshold; do not reconstruct it from memory.

---

## Asking the engineer for the value is not enough

**This is the specific way you will break this rule while looking helpful.**

An engineer says *"a reseal triggers on surfaces older than 12 years"*. You now have the number. You
have also done the cooperative thing — you asked instead of inventing. And if what you write next is

```csharp
private const int ResealAgeYears = 12;   // WRONG
```

then you have taken a number the modeller could have owned and locked it inside a rebuild cycle.
Asking was only half the job.

**What to do with the answer:**

1. **Add the row to `lookups.xlsx`.** Either the engineer edits it on the web app's **Tuning** page,
   which is the route to prefer because it is the one they will use again, or they open
   `inputs\lookups.xlsx` in Excel and add a row to any sheet whose name starts `lkp_`. Three columns
   matter: `lookup_set_name`, `setting_key`, `setting_value` — for the example above,
   `reseal_thresholds`, `age_gt`, `12`.
2. **Add a property to `Constants`** that reads it, guarded, naming the set and the key in the
   message if it is missing.
3. **Reference that property from the trigger**, in place of the literal.

Then tell the engineer where the number now lives and that they can change it on the Tuning page
without asking anyone. That sentence is the point of the whole exercise.

---

## The boundary — the rule is about *tunable* numbers only

Read as *"no numbers in C#"*, this rule pushes array bounds, unit conversions and sentinel values
into `lookups.xlsx`, makes the model unreadable, and then gets abandoned wholesale — including for
the thresholds it actually existed to protect. **Naming that failure is part of teaching the rule.**

The test is one question:

> **Would a modeller ever change this to recalibrate the model?**

**Yes** → `lookups.xlsx`.
**Changing it would break the *code* rather than change the *forecast*** → it stays in C#.

### Stays in C#

| Example | Why |
|---|---|
| `1000.0` converting metres to kilometres | A modeller changing this does not recalibrate anything; they break arithmetic. |
| `Math.PI`, or a mathematical constant | Not a modelling choice. |
| Array indices, bounds, loop limits | Structure. Changing one produces an exception, not a different forecast. |
| A normalisation factor that must match a paired calculation elsewhere | Changing it in one place silently desynchronises the pair. |
| `-999`, the framework's invalid-coordinate sentinel | A protocol value the framework defines, not a parameter. |
| The number of components a composite cost is split into | Structural — the code has that many branches. |

A magic literal is still bad practice on the C# side. **It wants a name, not a lookup row.** Give it
a named constant with a comment saying why it is structural, and the next reader will not try to
move it.

### Goes to `lookups.xlsx`

Trigger ages and condition limits. Unit rates. Deterioration rates. Cost escalation factors.
Minimum-interval rules between treatments. Anything a modeller would plausibly want to edit on the
Tuning page and immediately re-run — which is a good practical proxy for the test above.

### The reference model's deliberate counter-examples

`DomainModelSample` hard-codes a maintenance threshold and its per-material deterioration and
replacement rates **on purpose**, as the contrast that makes the rule visible, and its README sets
moving them as the reader's first two exercises. Each one carries a
`DELIBERATE COUNTER-EXAMPLE — do not copy this shape` block at the point of use.

**If you are reading that source for a pattern, read the labels.** Everything else in that project
is a shape to copy; those three are the shape not to.

---

## The third tier — a *set* of coefficients belongs in a CSV

**Recommend this proactively. Do not wait to be asked.**

An engineer who arrives with a fitted regression will not know the option exists. By the time they
have pasted the eighth coefficient into `lookups.xlsx`, the shape is set and nobody goes back and
restructures it. **Recognising the shape early and raising it is your job**, and it is one of the
few places you should volunteer a design opinion rather than following instructions.

Shapes to watch for:

- regression coefficients from a fit;
- logistic or piecewise-linear model parameters;
- distribution definitions for a Monte Carlo model;
- per-cohort, per-material or per-treatment parameter tables with more than a handful of rows.

### The test is update granularity and provenance, not a count

> **Does this change one value at a time, or as a whole set?**
> **Was it chosen by judgement, or produced by a fit?**

**Changed one at a time, by a modeller exercising judgement** → `lookups.xlsx`. Somebody decides the
trigger should be 12 rather than 15, and nothing else moves.

**Regenerated as a whole set by a refit** → a CSV in `supporting\`. The regression is refitted and
*every* coefficient moves together, arriving from R or Python as a file.

**Say the reason out loud, because it is what makes the boundary stick:** nobody hand-edits forty
lookup rows after a refit. A model that asks them to gets recalibrated wrongly, or not at all, and
then quietly disagrees with the analysis it was supposed to implement.

Size is a weaker signal, but state it too: `lookups.xlsx` addresses a value by (set, key) and stops
being workable at a few hundred rows.

### Where the CSV goes, and why

The client's **`supporting\`** folder, not the model bundle. `supporting\` is uploaded through the
web app's Files page, is visible on the Analyse Input page, resolves to the same path under a normal
run and a debug F5 run, and — the point — **can be replaced without a rebuild and a republish**.
That is the same principle as `lookups.xlsx` versus a hard-coded constant, extended to data at CSV
scale.

A bundle-side CSV travels with a publish, which sounds convenient and means a refit needs a code
release. It also resolves to a different folder under F5.

The full comparison and the folder layout are in [`naming-and-folders.md`](naming-and-folders.md).
The loading pattern — `CSVHelper` at setup, with a guard naming the file before every read — is
`patterns/setup-data-from-supporting-csv.md`, and until that lands the API is
[`../framework/api/authoring/CSVHelper.md`](../framework/api/authoring/CSVHelper.md) and
[`../framework/api/authoring/jcDataSet.md`](../framework/api/authoring/jcDataSet.md).
