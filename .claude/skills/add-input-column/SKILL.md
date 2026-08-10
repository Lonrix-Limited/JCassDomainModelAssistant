---
name: add-input-column
description: Declare a new column in the client's input CSV and read it into the element. Bundle input_headers row, the element property, and BOTH factory methods — missing the second is the classic bug. Use for "add an input column", "the CSV has a new field", "the model needs traffic counts".
---

# Add an input column

**This skill is a wrapper.** Every step is a page in `docs/` plus a `jcass-dm` verb. Without it, do
the same job by reading
[`docs/workflow/30-make-a-change.md` § Add an input column](../../../docs/workflow/30-make-a-change.md#add-an-input-column)
and running the verb it names.

## 0. Before the first step

- **Honour the verb** — [`docs/00-start-here.md` § 2](../../../docs/00-start-here.md).
- **Stop conditions apply** — [`docs/conventions/when-to-stop.md`](../../../docs/conventions/when-to-stop.md).
  Use the `draft-support-request` skill if one fires.

## 1. Read

- [`docs/workflow/30-make-a-change.md` § Add an input column](../../../docs/workflow/30-make-a-change.md#add-an-input-column) — the three places, and the procedure.
- [`docs/conventions/silent-failures.md` § 5](../../../docs/conventions/silent-failures.md#5-an-input-column-or-parameter-added-to-one-factory-method-but-not-the-other) — the classic bug, in full.

## 2. Run

```powershell
.\tools\jcass-dm.exe add-input-header ..\MyRoadModel\domain_model_setup.xlsx `
    --column traffic_count --type number --example 4200 --comment "AADT"
```

The column name must match the client's CSV header exactly. **Ask** rather than inferring it from
the engineer's prose — `traffic_count`, `TrafficCount` and `aadt` are three different columns.

## 3. Both factory methods. Every time.

`GetFromInputData` builds elements in period 0; `GetFromModelData` rebuilds them in every period
after that. Adding the read to one and not the other compiles perfectly and produces a model that is
correct in period 0 and wrong from period 1 — which looks like a modelling problem, gets debugged in
the wrong place, and **nothing catches it**, in the framework or in `jcass-dm`.

`workflow/30-make-a-change.md` gives the line and which dictionary it comes from. The habit that
prevents this, from `silent-failures.md` § 5: **open both methods in the same edit, every time.** In
guided mode, make that the step — have the engineer read the two side by side and say which
properties appear in only one.

## 4. The client's data

The framework rejects blanks in a numeric column, so a CSV with gaps needs a sentinel — and the code
has to recognise it. `workflow/30-make-a-change.md` names the convention. Whether this column can
have gaps is the engineer's answer, not yours.

## 5. Then

```powershell
.\tools\jcass-dm.exe dump ..\MyRoadModel\domain_model_setup.xlsx --sheet input_headers
dotnet build ..\MyRoadModel\MyRoadModel.csproj -c Debug --no-incremental
.\tools\jcass-dm.exe check --project ..\MyRoadModel
```

**Say plainly that `check` does not cover this change.** No rule compares the two factory methods.
The verification is reading them, and then seeing the column carry values past period 0 at F5.

## 6. Never

- **Never edit one factory method without opening the other** — § 3.
- **Never invent the column name or the sentinel** — § 2 and § 4.
