---
name: check-my-model
description: Run jcass-dm check on a domain model and explain the result in modelling terms — which rules passed, what a NOTE or SKIPPED means, and what the check cannot see. Use for "is my model OK", "check my model", "did I miss anything", or before packaging.
---

# Check my model

**This skill is a wrapper.** One verb, and the pages that explain its output. Without it, run
`jcass-dm check` and read [`docs/conventions/silent-failures.md`](../../../docs/conventions/silent-failures.md).

## 0. Before the first step

- **Honour the verb** — [`docs/00-start-here.md` § 2](../../../docs/00-start-here.md). In a guided
  session a green check is the feedback that proves the lesson landed: say what it just verified and
  why that mattered. It is never a way to skip the teaching.
- **Stop conditions apply** — [`docs/conventions/when-to-stop.md`](../../../docs/conventions/when-to-stop.md).

## 1. Read

- [`docs/conventions/silent-failures.md`](../../../docs/conventions/silent-failures.md) — what each rule is defending against, and the four failures nothing detects.
- [`tools/README.md`](../../../tools/README.md) § `check` is an explicit subset — and the exit codes.

## 2. Run

```powershell
.\tools\jcass-dm.exe check --project ..\MyRoadModel --lookups ..\lookups.xlsx
```

**Pass `--lookups` whenever a copy of the client's `inputs\lookups.xlsx` is available** — download
it from **Files → Inputs**. Without it the `lookup sets` rule reports `SKIPPED`, which is the rule
most worth having.

## 3. Explain the result

Read it back rule by rule, in modelling terms. Three things to get right:

- **`NOTE` and `SKIPPED` are not passes and are not failures.** A `SKIPPED` rule is one the tool
  could not apply — say which, and what would let it. A rule that quietly became a no-op is worse
  than no rule, which is why it reports rather than passing.
- **A `parameters vs C#` finding is a bug, never a tidiness note.** It is the only defence that
  exists anywhere against a column of zeros that looks like a result —
  [`silent-failures.md` § 1](../../../docs/conventions/silent-failures.md#1-a-parameter-declared-in-the-bundle-but-never-written).
- **Green does not mean the model is right.** It means nothing locally visible is inconsistent. Say
  so in your own words rather than paraphrasing it into "the model is fine". The web app's
  **Check Setup** is authoritative and sees the client's real data.

## 4. Say what it did not look at

Four entries on the silent-failures list have **no detection mechanism at all**, so nothing will
raise them for you. Raise them in conversation instead, whenever the model has just changed in a way
that touches one:

| # | Worth asking out loud |
|---|---|
| [2](../../../docs/conventions/silent-failures.md#2-a-parameter-whose-clamp-range-is-too-narrow) | Is any parameter's clamp range too narrow? |
| [3](../../../docs/conventions/silent-failures.md#3-reading-nelements-nperiods-or-nparameters-during-setup) | Does anything read the element counts during setup? |
| [5](../../../docs/conventions/silent-failures.md#5-an-input-column-or-parameter-added-to-one-factory-method-but-not-the-other) | Does every property appear in **both** factory methods? |
| [10](../../../docs/conventions/silent-failures.md#10-a-privately-constructed-random-instead-of-rando) | Is there a `new Random()` anywhere? |

## 5. Never

- **Never report a `SKIPPED` rule as passed**, and never summarise a run as clean when one is present.
- **Never treat a clean check as permission to publish.** It is a local subset; publish has its own
  gate — [`docs/workflow/40-publish.md`](../../../docs/workflow/40-publish.md).
