---
name: add-parameter
description: Add a model parameter — per-element state carried from one period to the next. Bundle row with a clamp range, the write in SetParameterValues, and the read-back in the factory. Use for "add a parameter", "the model needs to remember X between periods".
---

# Add a model parameter

**This skill is a wrapper.** Every step is a page in `docs/` plus a `jcass-dm` verb. Without it, do
the same job by reading
[`docs/workflow/30-make-a-change.md` § Add a model parameter](../../../docs/workflow/30-make-a-change.md#add-a-model-parameter)
and running the verb it names.

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
  Use the `draft-support-request` skill if one fires.

## 1. Read

- [`docs/workflow/30-make-a-change.md` § Add a model parameter](../../../docs/workflow/30-make-a-change.md#add-a-model-parameter) — the three places, and the procedure.
- [`docs/conventions/silent-failures.md` § 1](../../../docs/conventions/silent-failures.md#1-a-parameter-declared-in-the-bundle-but-never-written) and [§ 2](../../../docs/conventions/silent-failures.md#2-a-parameter-whose-clamp-range-is-too-narrow) — the two failures this change can ship.

## 2. First, is it actually a parameter?

A parameter is state that must **survive** from one period to the next. If the value can be
recalculated from the inputs and the other parameters, it does not need to be one. Ask, and say
which it is — `workflow/30-make-a-change.md` opens that section with the test.

## 3. Run

```powershell
.\tools\jcass-dm.exe add-parameter ..\MyRoadModel\domain_model_setup.xlsx `
    --name par_iri --min 0 --max 20 --decimals 2 --comment "Roughness, IRI m/km"
```

**`--min` and `--max` are required, and they are clamps rather than validation** — every value
written is forced into the range, quietly and by design. **Ask the engineer for the physical range
of the quantity.** A range you guessed produces a parameter that pins at a limit and a forecast that
is wrong in a way that looks plausible; nothing anywhere reports it.

## 4. The two code edits, and the dangerous one

Both are in `workflow/30-make-a-change.md`. The one to get right is the **write** in
`SetParameterValues`: a parameter declared in the bundle and never written is left at zero for every
element in every period, the run completes, and the output column looks exactly like a modelling
result. `jcass-dm check`'s `parameters vs C#` rule is the only defence that exists anywhere.

The read-back belongs in `GetFromModelData` and comes from `numModelData` — the parameters this
model wrote last period — **never from `numInputs`**. That page says what happens if you get it
wrong, and it is the failure that looks like a modelling problem.

## 5. Then

```powershell
.\tools\jcass-dm.exe dump ..\MyRoadModel\domain_model_setup.xlsx --sheet parameters
dotnet build ..\MyRoadModel\MyRoadModel.csproj -c Debug --no-incremental
.\tools\jcass-dm.exe check --project ..\MyRoadModel
```

`check`'s `parameters vs C#` rule is the one that matters here. Note that it can only see parameters
written with a literal name.

## 6. Never

- **Never choose the clamp range yourself** — § 3.
- **Never read evolving state from `numInputs`** — § 4.
- **Never add the property to one factory method only.** That is
  [`silent-failures.md` § 5](../../../docs/conventions/silent-failures.md#5-an-input-column-or-parameter-added-to-one-factory-method-but-not-the-other),
  and nothing catches it.
