# 30 — Make a change

**Goal of this page:** the four changes you will make over and over, each one with **every place it
touches listed up front**.

Almost every mistake in a domain model is a change made in three places out of four. So the shape
of each section below is the same: the table of places first, then the procedure, then what catches
you if you miss one.

> **File names on this page are the ones `jcass-dm scaffold` produces.** If you are working on an
> older model the names may differ — `SampleElement.cs` for `ModelElement.cs`,
> `TreatmentTrigger.cs` for `TreatmentsTrigger.cs`. What matters is which file owns which stage,
> and that mapping is in
> [`../orientation/how-a-run-works.md`](../orientation/how-a-run-works.md).

**Before you start, and after you finish:** you must already have proved the pipeline end to end
([`README.md`](README.md#the-walking-skeleton--do-this-before-you-model-anything)). Every change
below ends the same way — `dotnet build`, `jcass-dm check`, then back through
[`20-upload-and-debug.md`](20-upload-and-debug.md).

---

## Change a threshold or a rate

**Places it touches: none in C#, most of the time.**

| Where | What |
|---|---|
| The client's `inputs\lookups.xlsx` | The number itself |

**This is the change you want to be making**, because it needs no rebuild, no upload and no
publish. Edit the value on the web app's **Tuning** page — which writes to `lookups.xlsx` for you —
and queue a new run.

**If the number is not in `lookups.xlsx` yet, that is the actual task**, and it is three steps:

1. `inputs\lookups.xlsx` → the right `lkp_` sheet → add a row with a set name and a setting key.
2. `Objects\Constants.cs` → read it in the constructor, next to its neighbours:
   ```csharp
   this.ResealAgeGreaterThan = GetNumber(lookupSets, ResealThresholds, "age_gt");
   ```
3. Use `constants.ResealAgeGreaterThan` where the number was.

**Never write the number into the C# instead.** Being *told* the value by the engineer is not
permission to hard-code it — an assistant that asks "what age threshold?" and then writes
`const int ResealAgeYears = 12;` has done the wrong thing while looking cooperative. The rule, its
reason, and the numbers that legitimately *do* stay in C#:
[`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md).

---

## Add a treatment

**Five places, and missing one is silent in four of the five.**

| # | Where | What |
|---|---|---|
| 1 | `Objects\TreatmentNames.cs` | A `const string` for the name |
| 2 | `domain_model_setup.xlsx` → `treatments` | A row using **exactly** that string, and a budget category |
| 3 | `Objects\TreatmentsTrigger.cs` | When it fires, and what it costs |
| 4 | `Objects\Resetter.cs` | A `case` arm — what it does to the element |
| 5 | The client's `inputs\lookups.xlsx` → `unit_rates` | A rate row keyed by the treatment name, plus any thresholds it needs |

### The procedure

1. **`Objects\TreatmentNames.cs`** — add the constant. Every other place refers to this, so the
   string is typed once in the whole model:
   ```csharp
   /// <summary>Resurfacing without structural work.</summary>
   public const string Reseal = "reseal";
   ```

2. **The bundle** — add the row with the tool rather than by hand, so the name cannot drift:
   ```powershell
   .\tools\jcass-dm.exe add-treatment ..\MyRoadModel\domain_model_setup.xlsx `
       --name reseal --budget-category surfacing --description "Reseal"
   ```
   `--budget-category` must name a column that exists in the client's `inputs\budgets.xlsx`. Get
   it wrong and the run stops at setup naming the treatment — loud, but only after an upload and a
   wait. `jcass-dm check` and the web app's **Check Setup** both find it sooner.

3. **`Objects\TreatmentsTrigger.cs`** — add an `AddResealIfValid(...)` beside the existing
   `AddRepairIfValid` / `AddReplaceIfValid`, and **call it from `GetTriggeredTreatments`**. Copy
   the shape of the neighbours: read the thresholds off `Constants`, build the cost from
   `constants.GetUnitRate(TreatmentNames.Reseal)` and the element's quantity, and return a
   `TreatmentInstance`.

   > Adding the method and forgetting the call is the commonest version of this mistake. The model
   > builds, runs, and simply never proposes the treatment.

4. **`Objects\Resetter.cs`** — add the `case`:
   ```csharp
   case TreatmentNames.Reseal:
       // what the treatment does to the element's condition, age, etc.
       break;
   ```
   The `default:` arm throws, so a treatment with no arm fails **loudly** the first time it is
   applied. This is the one place in the five where forgetting is not silent.

5. **`inputs\lookups.xlsx`** — add the `unit_rates` row keyed `reseal`, and any thresholds the
   trigger reads. Upload it on **Files → Inputs**, or edit it on the **Tuning** page.

### Then

```powershell
dotnet build ..\MyRoadModel\MyRoadModel.csproj -c Debug --no-incremental
.\tools\jcass-dm.exe check --project ..\MyRoadModel --lookups ..\lookups.xlsx
```

`check` cross-references the `treatments` sheet against `TreatmentNames` **in both directions**,
and confirms every treatment has a `case` arm — so places 1, 2 and 4 are covered mechanically.
**Places 3 and 5 are not**, and no tool can cover them: only you know that a treatment which never
fires is wrong. Watch for it at F5.

---

## Add an input column

The client's network CSV gains a column, say `traffic_count`.

**Three places, and the second one is two edits, not one.**

| # | Where | What |
|---|---|---|
| 1 | `domain_model_setup.xlsx` → `input_headers` | Declare the column and its type |
| 2 | `Objects\ModelElementFactory.cs` | Read it in **both** `GetFromInputData` **and** `GetFromModelData` |
| 3 | `Objects\ModelElement.cs` | The matching property |

### The procedure

1. **The bundle:**
   ```powershell
   .\tools\jcass-dm.exe add-input-header ..\MyRoadModel\domain_model_setup.xlsx `
       --column traffic_count --type number --example 4200 --comment "AADT"
   ```

2. **`Objects\ModelElement.cs`** — add the property:
   ```csharp
   /// <summary>Annual average daily traffic, from the input column 'traffic_count'.</summary>
   public double TrafficCount { get; set; }
   ```

3. **`Objects\ModelElementFactory.cs`** — add the same line to **both** factory methods:
   ```csharp
   TrafficCount = numInputs["traffic_count"],
   ```
   Numeric columns come from `numInputs`, text columns from `textInputs`.

> **Adding it to one factory method and not the other is the classic bug in this framework.**
> `GetFromInputData` builds elements in period 0; `GetFromModelData` rebuilds them every period
> after that. Miss the second and the model is correct in period 0 and wrong from period 1 —
> which looks like a modelling problem, not a plumbing one, and gets debugged in the wrong place
> for an afternoon.
> [`../conventions/silent-failures.md` § 5](../conventions/silent-failures.md#5-an-input-column-or-parameter-added-to-one-factory-method-but-not-the-other).

**One thing about the client's data:** the framework rejects blanks in a numeric column. A CSV with
gaps needs a sentinel value instead — `-999` is the convention for an invalid coordinate — and your
code has to recognise it.

---

## Add a model parameter

Parameters are the per-element state that **survives from one period to the next**. If a value has
to be remembered, it is a parameter. If it can be recalculated from the inputs and the other
parameters, it does not need to be one.

**Three places, and the second one is the single most dangerous omission in the framework.**

| # | Where | What |
|---|---|---|
| 1 | `domain_model_setup.xlsx` → `parameters` | Declare it, **with a clamp range** |
| 2 | `Objects\ModelElement.cs` → `SetParameterValues` | **Write** it. Nothing anywhere checks that you did |
| 3 | `Objects\ModelElementFactory.cs` → `GetFromModelData` | Read it back |

### The procedure

1. **The bundle:**
   ```powershell
   .\tools\jcass-dm.exe add-parameter ..\MyRoadModel\domain_model_setup.xlsx `
       --name par_iri --min 0 --max 20 --decimals 2 --comment "Roughness, IRI m/km"
   ```
   `--min` and `--max` are required for a numeric parameter, and the tool refuses without them.

   > **`minimum` and `maximum` are clamps, not validation.** Every value written to the parameter
   > is forced into that range, quietly and by design, so that one out-of-range calculation cannot
   > abort a whole run. Set the range too narrow and there is no error: the parameter simply pins
   > at a limit for the periods that hit it, and the forecast is wrong in a way that looks
   > plausible. If a parameter comes out suspiciously flat or sits exactly on a round number,
   > check this row first.
   >
   > A *degenerate* range — minimum above maximum, or the two equal — the framework does reject,
   > at setup, by name.

   Numeric parameter names conventionally start with `par_`.

2. **`Objects\ModelElement.cs`** — add the property, then write it in `SetParameterValues`:
   ```csharp
   numModParamValues("par_iri", this.Roughness);
   ```

   > **This is the one to get right.** A parameter declared in the bundle and never written here
   > is allocated and left at **zero for every element in every period**. The run completes, the
   > outputs carry a column of zeros, and that column looks exactly like a modelling result.
   > Nothing in the framework catches it — `jcass-dm check`'s `parameters vs C#` rule is the only
   > defence that exists anywhere.
   > [`../conventions/silent-failures.md` § 1](../conventions/silent-failures.md#1-a-parameter-declared-in-the-bundle-but-never-written).

3. **`Objects\ModelElementFactory.cs`** → `GetFromModelData` — read it back into the element, from
   `numModelData` (the parameters this model wrote last period), **not** from `numInputs`:
   ```csharp
   Roughness = numModelData["par_iri"],
   ```

   > Reading evolving state from `numInputs` instead is how a model quietly stops changing: every
   > period restarts from the original survey value, and the forecast comes out flat.

### Then

```powershell
.\tools\jcass-dm.exe dump ..\MyRoadModel\domain_model_setup.xlsx --sheet parameters
.\tools\jcass-dm.exe check --project ..\MyRoadModel
```

`dump` prints the sheet as text so you can compare it against `SetParameterValues` without opening
Excel. `check` compares the two for you and is the rule that matters here.

---

## Adding a whole *set* of numbers — coefficients, not thresholds

If what you are adding is a **fitted set** — regression coefficients, logistic model parameters,
distribution definitions, a per-material or per-cohort table — it does not belong in
`lookups.xlsx` and it certainly does not belong in C#. It belongs in a CSV in the client's
`supporting\` folder, loaded at setup.

The test is not how many values there are; it is **how they change**:

> Does this change one value at a time, or as a whole set?
> Was it chosen by judgement, or produced by a fit?

Refit the regression and every coefficient moves together, arriving from R or Python as a file.
Nobody hand-edits forty lookup rows after a refit — they do it wrongly or they do not do it.

**Raise this yourself when you see the shape**, rather than waiting to be asked; by the time the
eighth coefficient has been pasted into `lookups.xlsx` the shape is set and nobody goes back. The
rule is [`../conventions/where-numbers-live.md` § the third tier](../conventions/where-numbers-live.md#the-third-tier--a-set-of-coefficients-belongs-in-a-csv);
the implementation pattern is in [`../patterns/`](../patterns/).

---

## When the change is not one of these four

Compose freely from what is in [`../framework/api/`](../framework/api/README.md) — most real work
is composition, and that is the normal case. But if you need a framework call that is **not in the
API reference**, stop: that is the moment you would be inventing a signature, and the engineer
cannot tell that you did.
[`../conventions/when-to-stop.md`](../conventions/when-to-stop.md).

## Done when

- [ ] Every row of the relevant table above is ticked off.
- [ ] `dotnet build` clean, `jcass-dm check` clean.
- [ ] The change is visible at F5 — the treatment fires, the parameter is not zero, the column has
      values.

Next: back through [`20-upload-and-debug.md`](20-upload-and-debug.md), then
[`40-publish.md`](40-publish.md).
