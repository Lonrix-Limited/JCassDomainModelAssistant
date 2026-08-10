---
name: add-lookup-constant
description: Put a tunable number where a modeller can change it — a lookups.xlsx row plus a guarded Constants property. Also recognises when the engineer is really adding a fitted SET of coefficients, which belongs in a supporting/ CSV instead. Use for "add a threshold", "the trigger age should be 12", "where do I put this rate", or any number arriving from a regression.
---

# Add a lookup constant

**This skill is a wrapper.** Every step is a page in `docs/`. Without it, do the same job by reading
[`docs/patterns/constants-from-lookups.md`](../../../docs/patterns/constants-from-lookups.md).

## 0. Before the first step

- **Honour the verb** — [`docs/00-start-here.md` § 2](../../../docs/00-start-here.md).
- **Stop conditions apply** — [`docs/conventions/when-to-stop.md`](../../../docs/conventions/when-to-stop.md).

## 1. First decide which of the three tiers this is — and say so out loud

[`docs/conventions/where-numbers-live.md`](../../../docs/conventions/where-numbers-live.md) is the
rule, and **the boundaries are the part people get wrong**. Do not reconstruct the test from memory.

| It is | It goes | Page |
|---|---|---|
| A **tunable scalar** a modeller would change to recalibrate | `inputs\lookups.xlsx` | [`patterns/constants-from-lookups.md`](../../../docs/patterns/constants-from-lookups.md) |
| A **fitted set** regenerated as a whole by a refit | a CSV in the client's `supporting\` folder | [`patterns/setup-data-from-supporting-csv.md`](../../../docs/patterns/setup-data-from-supporting-csv.md) |
| **Structure** — a scale endpoint, a unit conversion, a bound, a sentinel | C#, as a named constant with a comment saying why | `where-numbers-live.md` § The boundary |

## 2. Raise the set case yourself — do not wait to be asked

**This is the half of the skill that gets skipped.** An engineer arriving with a fitted regression
will not know the third tier exists, and by the time the eighth coefficient is in `lookups.xlsx` the
shape is set and nobody goes back and restructures it.

Watch for it whenever: several related numbers arrive together; they came out of R, Python, a
regression or a distribution fit; or they are a per-material, per-cohort or per-treatment table.
The test is **update granularity and provenance**, not a count — `where-numbers-live.md`
§ *The third tier*. Say the reason out loud: nobody hand-edits forty lookup rows after a refit.

If it is a set, switch to
[`patterns/setup-data-from-supporting-csv.md`](../../../docs/patterns/setup-data-from-supporting-csv.md)
— or to [`logistic-coefficients.md`](../../../docs/patterns/logistic-coefficients.md) or
[`distribution-simulators.md`](../../../docs/patterns/distribution-simulators.md) if it is one of
those two shapes — and stop following this page.

## 3. The scalar case, end to end

Four steps, and they are in
[`patterns/constants-from-lookups.md`](../../../docs/patterns/constants-from-lookups.md)
§ *Adding a number, end to end*: the `lookups.xlsx` row, the guarded `Constants` property, the
reference in place of the literal, and **telling the engineer where the number now lives and that
they can change it on the Tuning page without asking anyone.** That last sentence is the point of
the whole exercise; do not drop it.

Two mechanics from the same page that are not optional: **guard before you index**, naming the set
and the key, and **`Convert.ToDouble`, never a cast** — `setting_value` arrives as text whatever the
cell looks like in Excel.

**There is no `jcass-dm` verb for this, deliberately.** `lookups.xlsx` is the client's file, and the
route to prefer is the engineer editing it on the web app's **Tuning** page — because that is the
route they will use again, every time they recalibrate.

## 4. Then

```powershell
dotnet build ..\MyRoadModel\MyRoadModel.csproj -c Debug --no-incremental
.\tools\jcass-dm.exe check --project ..\MyRoadModel --lookups ..\lookups.xlsx
```

The `lookup sets` rule compares the set names the C# asks for against the ones the file actually has
— which is what catches a set name typed two ways. It needs `--lookups` or it reports `SKIPPED`.

## 5. Never

- **Never write the number into C# because the engineer told you what it is.** Being told is not
  permission — `where-numbers-live.md` § *Asking the engineer for the value is not enough*.
- **Never choose the value.** If they have not given one, the row and the property still get
  scaffolded and the value is theirs to supply.
- **Never read the counterexamples in the reference model as a shape to copy.** `DomainModelSample`
  hard-codes three numbers on purpose and labels each one at the point of use.
