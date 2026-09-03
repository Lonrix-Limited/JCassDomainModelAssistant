---
name: add-lookup-constant
description: Put a tunable number where a modeller can change it — a lookups.xlsx row plus a guarded Constants property. Also recognises when the engineer is really adding a fitted SET of coefficients, which belongs in a supporting/ CSV instead. Use for "add a threshold", "the trigger age should be 12", "where do I put this rate", or any number arriving from a regression.
---

# Add a lookup constant

**This skill is a wrapper.** Every step is a page in `docs/`. Without it, do the same job by reading
[`docs/patterns/constants-from-lookups.md`](../../../docs/patterns/constants-from-lookups.md).

## 0. Before the first step

- **This skill changes the model, so the `model-knowledge` check happens before the change** —
  [`docs/00-start-here.md` § 4](../../../docs/00-start-here.md). Three steps, in order, and step 3
  is the one that regresses:

  1. Read `model-knowledge/<ModelName>.md` if it is there, and use it.
  2. If it is not there, list the folder that *contains* this repository and look for a sibling
     `JCassDomainModelAssistant*-old` or `*-main` holding one.
  3. **Stop. Ask them to copy it across — naming the exact folder if step 2 found one — and wait
     for their reply.** Do not start the edit and mention it afterwards; afterwards is too late,
     because the edit is what the notes were for.

  **Skip all three only if you scaffolded this model yourself in this session, or this conversation
  has already done the check for it.** Invoking this skill is not a way past the stop — half the
  answers you are about to ask them for are often already in that file.
- **Honour the verb** — [`docs/00-start-here.md` § 2](../../../docs/00-start-here.md).
- **Stop conditions apply** — [`docs/conventions/when-to-stop.md`](../../../docs/conventions/when-to-stop.md).

## 1. First decide which of the three tiers this is — and say so out loud

[`docs/conventions/where-numbers-live.md`](../../../docs/conventions/where-numbers-live.md) is the
rule, and **the boundaries are the part people get wrong**. Do not reconstruct the test from memory.

| It is | It goes | Page |
|---|---|---|
| A **tunable scalar** a modeller would change to recalibrate | `inputs\lookups.xlsx`, any `lkp_` sheet | [`patterns/constants-from-lookups.md`](../../../docs/patterns/constants-from-lookups.md) |
| A **treatment unit rate** | `inputs\lookups.xlsx`, the **`lkp_unit_rates`** sheet by name — § 2b | `where-numbers-live.md` § *Unit rates go in one named sheet* |
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

## 2b. A unit rate has a fixed sheet, and it is the only number that does

**A treatment's cost per unit goes in the `lkp_unit_rates` sheet of `inputs\lookups.xlsx`.** Not any
`lkp_` sheet — that one, spelled that way.

Every `lkp_*` sheet is merged before your C# sees it, so the sheet makes no difference to the model.
It makes all the difference to the modeller: the Tuning page's **Treatment Rates** tab reads
`lkp_unit_rates` by name, and a rate anywhere else loads, costs treatments correctly, and **is not on
the page they were told to edit rates on**. Nothing errors; they simply cannot find it.

- **Group with sets, not sheets.** The tab's dropdown is the distinct `lookup_set_name` values in
  that sheet, so several sets give a modeller several short tables without leaving the sheet.
- **Fill in the `comment` column.** The tab renders it beside the value, and it is the only place a
  modeller learns what the rate is priced per — *"$/m² for thin AC"*.
- **Move a rate, never copy it.** A `(set, key)` pair appearing in two `lkp_*` sheets makes the web
  app's save refuse as ambiguous rather than choose one.

**If they want the rate to vary — by material, by distress, by traffic — the answer is to vary the
quantity, not the rate.** `TreatmentInstance` takes `quantity` and `unitRate` separately and costs
them as a product, so the shape is a single rate from `lkp_unit_rates` and a quantity the C#
computes: a repaired length rather than the segment length, a measured area, an extent fraction. The
factors that produce the quantity are themselves tunable numbers and follow § 3 like any other. A
rate computed in C# is a rate the modeller has lost, which is the thing this whole skill exists to
prevent. [`patterns/treatment-instances.md`](../../../docs/patterns/treatment-instances.md)
§ *quantity and unitRate*.

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
