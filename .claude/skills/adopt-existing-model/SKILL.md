---
name: adopt-existing-model
description: Pick up a Juno Cassandra domain model somebody else wrote. Diagnoses it with jcass-dm check before changing anything, offers a rename only when the four names actually disagree, maps it onto the canonical skeleton, and surfaces the takeover publish warning. Use for "refactor the model in folder X", an inherited model, or one that only exists on the server.
---

# Adopt an existing model

**This skill is a wrapper.** Every step is a page in `docs/` plus a `jcass-dm` verb. Without it,
do the same job by reading [`docs/workflow/05-adopt-an-existing-model.md`](../../../docs/workflow/05-adopt-an-existing-model.md)
and running the verbs it names.

## 0. Before the first step

- **Read the takeover warning now, not at step 40.**
  [`docs/workflow/40-publish.md`](../../../docs/workflow/40-publish.md#-before-a-first-publish-on-a-client-that-already-runs-a-custom-model).
  An inherited model is by definition the takeover case: the client may have a production model that
  a practice publish would replace with a sample. Prove the pipeline as far as F5 and stop.
- **Honour the verb** — [`docs/00-start-here.md` § 2](../../../docs/00-start-here.md). In guided
  mode, walk `workflow/05-adopt-an-existing-model.md` a step at a time.
- **Stop conditions apply throughout** — [`docs/conventions/when-to-stop.md`](../../../docs/conventions/when-to-stop.md).
  An inherited model is where they fire most: a framework call that is not in the API reference is a
  stop, and inferring its signature from the code around it is invention, not composition. Use the
  `draft-support-request` skill.

## 1. Read

- [`docs/workflow/05-adopt-an-existing-model.md`](../../../docs/workflow/05-adopt-an-existing-model.md) — the procedure this skill runs, all six steps.
- [`docs/conventions/silent-failures.md`](../../../docs/conventions/silent-failures.md) — **all twelve.** This is the page adoption exists to apply.
- [`docs/conventions/four-names.md`](../../../docs/conventions/four-names.md) — before you offer a rename.

## 2. Diagnose before you change anything

```powershell
.\tools\jcass-dm.exe check --project ..\TheirModel
```

**First action, always** — before reading a line of the C#. Then read the result back to the
engineer in plain terms, rule by rule; `workflow/05` § step 2 has a row per rule and what a
non-OK result means. The `check-my-model` skill does this part.

## 3. Rename only if the names actually disagree, and only after asking

**A rename changes what the registry loads. Never do it unprompted.**

- `the four names` **OK** → there is nothing to do. Do not offer a rename, do not tidy the name, do
  not rename to something that reads better.
- `the four names` **not OK** → say which of the four disagrees, say what a rename would set them
  all to, and ask. `workflow/05` § step 3 covers choosing the name — if in doubt it is whatever the
  `.csproj` is already called.

```powershell
.\tools\jcass-dm.exe rename <Name> --project ..\TheirModel
```

All four together or none. **Never walk an engineer through doing it by hand.**

## 4. Get it building

`refs\`, and what a build error naming a missing framework type means — `workflow/05` § step 3,
second half. That error is a signal to stop and say so, not a puzzle to solve by substituting a
call.

## 5. Find out what it needs, and map it

`workflow/05` steps 4 and 5. Three questions answered from the model itself — lookup sets
(`check --lookups`), side-car CSVs, input columns (`dump --sheet input_headers`) — then fill in the
canonical-skeleton table on that page and give it to the engineer. Frame it as *"here is where
deterioration happens"*, not as a code review.

## 6. Check the reset default arm — this one is yours to look at

`check` compares treatments against the case arms it can find, and reports `SKIPPED` rather than
passing when the reset is an if/else chain or a dictionary. **A `default:` that does nothing, or a
chain with no final `else`, is the adoption case for**
[`silent-failures.md` § 11](../../../docs/conventions/silent-failures.md#11-a-treatment-with-no-arm-in-the-reset-switch):
the treatment is funded, reported, and changes nothing. Read the reset yourself, and recommend
making the default throw.

While you are in there, the three other things `workflow/05` § step 5 asks you to report —
hard-coded numbers that belong in `lookups.xlsx`, anything reading `NElements` / `NPeriods` /
`NParameters` at setup, and bundle-relative CSV paths. **Report; do not move them.** Moving a
threshold changes the forecast and that is the engineer's decision.

## 7. Never

- **Never rename without asking**, and never when the four names already agree.
- **Never publish** as part of learning the workflow on an inherited model. See § 0.
- **Never relocate numbers, CSVs or logic silently.** Recommend, with the reason, and let them decide.

## 8. Done when

`workflow/05-adopt-an-existing-model.md` § Done when — including one green F5 run **before** any
change. Then [`30-make-a-change.md`](../../../docs/workflow/30-make-a-change.md).
