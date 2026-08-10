# Constants from lookups

**Compiling example:** [`ConstantsFromLookups.cs`](../../examples/ExamplesLibrary/ConstantsFromLookups.cs)
**Rule it implements:** [`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md)
**Also in every scaffolded project:** `Objects\Constants.cs`

---

## When to reach for it

**Always.** This is the one universal pattern — every model in the corpus has a `Constants` class,
and every other page in this folder links back here for its numbers.

You need it the moment your model has a threshold, a rate, a factor, a limit or a weight in it.
Which is to say, immediately.

---

## Why it exists

A number hard-coded in C# is a number the modeller cannot change without a developer, a rebuild and
a republish. The same number in `inputs\lookups.xlsx` is one they change themselves on the web app's
**Tuning** page and re-run in minutes.

That difference is the whole distance between a model somebody can calibrate and a model they can
only file tickets against — and recalibration is not an occasional event. Moving a trigger from
grade 3.5 to grade 3.2, looking at the result, and trying 3.3 **is the work**. A model that makes
each of those a developer request does not get recalibrated; it gets abandoned, or worse, trusted
while stale.

`Constants` is the seam that makes the second thing possible. That is all it is, and it is why it
is worth a class of its own rather than a scattering of `model.Lookups[...]` reads.

---

## The shape

```csharp
public class PipeConstants
{
    private const string ReplaceThresholds = "replace_thresholds";
    private const string UnitRates = "unit_rates";

    private readonly Dictionary<string, object> _unitRates;

    public double ReplaceConditionGreaterThan { get; }

    public PipeConstants(Dictionary<string, Dictionary<string, object>> lookupSets)
    {
        this.ReplaceConditionGreaterThan = GetNumber(lookupSets, ReplaceThresholds, "cond_gt");
        _unitRates = GetSet(lookupSets, UnitRates);
    }

    public double GetUnitRate(string treatmentName) => Resolve(_unitRates, treatmentName, UnitRates);
}
```

Read the full file for the guard helpers and the rest of the properties. Four things in it are the
pattern:

### 1. Read it in `SetupInstance`, and not before

```csharp
public override void SetupInstance()
{
    this.Constants = new PipeConstants(this.model.Lookups);
}
```

`SetupInstance` is the **earliest** place lookups may be read, and they are fully available there —
the framework loads the domain model last, deliberately, so that its setup can use project data.

A constructor is too early: `model` is not assigned until the framework's own setup runs afterwards,
so reading anything through it there throws a `NullReferenceException`. That failure is loud and
therefore self-correcting; it is not the one to worry about.

> **The quiet one at that point is the element counts.** `model.NElements`, `model.NPeriods` and
> `model.NParameters` are all still **zero** during `SetupInstance`. Sizing an array off one of them
> produces an empty array and a model that runs to completion with nothing in it. Nothing errors and
> nothing reports it. Read those counts in `Initialise` or later.
> [`../framework/api/authoring/DomainModelBase.md`](../framework/api/authoring/DomainModelBase.md).

### 2. Two shapes, and choosing between them is the interesting part

| Shape | Use when | Cost of adding a value |
|---|---|---|
| **Unpacked into a property** in the constructor | The value is used in one specific comparison | A lookup row **and** a property |
| **Set kept whole**, resolved by key at the point of use | The values are all used the same way — a rate per material, a rate per treatment | A lookup row, and nothing in C# |

The second is what makes a model extensible by a modeller. `GetUnitRate(treatmentName)` means a new
treatment needs a row in `lookups.xlsx` and a constant in `TreatmentNames` — and nothing at all in
`Constants`.

The first is what makes a *missing* value fail loudly at setup rather than at the moment it is first
needed. Use it for the handful of thresholds the model cannot start without.

### 3. Set names are named constants, not inline strings

```csharp
private const string ReplaceThresholds = "replace_thresholds";
```

Each set name is used at least twice — in the read, and in the error message when the read fails.
This is a structural constant, not a tunable number: changing it would break the correspondence with
the spreadsheet, not change the forecast.

### 4. Guard, and name the set and the key

```csharp
if (!set.ContainsKey(key))
{
    throw new Exception($"'{key}' has no value in lookup set '{setName}' in lookups.xlsx.");
}

return Convert.ToDouble(set[key]);
```

**Both halves matter.**

- **The guard** turns a spreadsheet typo into a message a modeller can act on, at setup, before a
  single period is modelled. Without it: `KeyNotFoundException`, naming nothing.
- **`Convert.ToDouble`, never a cast.** `setting_value` arrives as **text** whatever the cell looks
  like in Excel. `(double)set[key]` throws an `InvalidCastException` whose message mentions nothing
  about spreadsheets and sends the engineer looking in the wrong place entirely.

---

## How `lookups.xlsx` is structured

Any sheet whose name starts `lkp_` is read, and all of them are merged into **one flat table**.
Three columns matter:

| Column | Is |
|---|---|
| `lookup_set_name` | The set a row belongs to |
| `setting_key` | The key within that set |
| `setting_value` | The value, always read as text |

A value is addressed by the pair **(set name, key)**. Which sheet a row sits in is only an
organisational convenience, so sheets can be regrouped freely without touching any C#.

`jcass-dm check --lookups <path-to-lookups.xlsx>` compares the set names your `Constants` class asks
for against the ones the file actually has, before you upload anything.

---

## Adding a number, end to end

An engineer says *"replace anything worse than grade 4."* You now have the number. Asking was the
right thing to do — **and writing `private const double ReplaceGrade = 4.0;` next would undo it.**

1. **Add the row to `lookups.xlsx`.** Either they edit it on the Tuning page, which is the route to
   prefer because it is the one they will use again, or they open `inputs\lookups.xlsx` and add a
   row to any `lkp_` sheet: `replace_thresholds`, `cond_gt`, `4`.
2. **Add a property to `Constants`** that reads it, guarded, naming the set and the key.
3. **Reference that property** from the trigger, in place of the literal.
4. **Tell them where the number now lives** and that they can change it on the Tuning page without
   asking anyone. That sentence is the point of the whole exercise.

Full procedure, with the other three common changes:
[`../workflow/30-make-a-change.md`](../workflow/30-make-a-change.md#change-a-threshold-or-a-rate).

---

## When a number should *not* come from here

Two exits, and both are common enough that missing them makes the rule unusable.

**It is structural** — a scale endpoint, a unit conversion, an array bound, a sentinel like the
framework's `-999` invalid-coordinate marker. Changing it would break the *code* rather than change
the *forecast*. It stays in C#, as a named constant with a comment saying why.

**It is a fitted set** — regression coefficients, distribution definitions, a per-cohort table.
Regenerated as a whole set by a refit, arriving from R or Python as a file. Those go in a CSV in the
client's `supporting\` folder: [`setup-data-from-supporting-csv.md`](setup-data-from-supporting-csv.md).

The test for each is in [`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md).
Do not reconstruct it from memory; the boundaries are the part people get wrong.

---

## Related

- [`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md) — the rule, and both boundaries
- [`setup-data-from-supporting-csv.md`](setup-data-from-supporting-csv.md) — the third tier
- [`../framework/api/authoring/ModelBase.md`](../framework/api/authoring/ModelBase.md) — `Lookups`, `GetLookupValueNumber`, `MultiColumnLookups`
- [`../framework/api/authoring/DomainModelBase.md`](../framework/api/authoring/DomainModelBase.md) — what is and is not ready in `SetupInstance`
- [`../../reference-model/DomainModelSample/README.md`](../../reference-model/DomainModelSample/README.md) § 7 — the same pattern in a complete model, with two deliberate counter-examples
