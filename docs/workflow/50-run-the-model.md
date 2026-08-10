# 50 — Run the model and read the result

**Goal of this page:** a normal forecast, queued and completed against the published model, with
outputs a modeller can look at. This is the step that closes the walking skeleton — it proves the
whole pipeline works, not just the debugging half of it.

You need [`40-publish.md`](40-publish.md) done. A run uses the **published** model; it does not see
your debug workspace.

---

## Step 1 — Queue the run

**Run Model** in the navigation bar.

| Field | What to pick |
|---|---|
| **Job type** | `Single` for one forecast. `Multiple` and `GoalSeek` are the other model types — [`../framework/concepts/05-model-types.md`](../framework/concepts/05-model-types.md) |
| **Model version** | Your custom model. It has exactly one version |
| **Config tag(s)** | Which configuration from the client's `inputs\configurations.xlsx` to run |

Then **Queue run**.

> **This is where the config tag is your choice.** A debug F5 run silently uses the first tag in
> alphabetical order and tells you which in its console output. A real run does not choose for
> you. If a debug run and a real run disagree about something, check that they used the same
> configuration before you look anywhere else.

If the button is unavailable, an inline message says why. The usual reason is the project lock —
either you do not hold it, or somebody else does.

## Step 2 — Watch it

A progress panel appears with a live log. A model run takes minutes rather than seconds; the first
one on a client takes longer, because it stages the model bundle before it starts.

**You should see** the log reach a completed state. If it fails, the log names the stage it failed
in, and setup failures are the informative ones — a missing lookup set, a budget category with no
column, a missing input column all name themselves here.

> **The run log redacts absolute paths.** If you are chasing a path problem, the debug console at
> F5 is what shows you the literal path; the run log will not.

## Step 3 — Look at the outputs

**Files → Outputs**, or the **Postprocessing** page for charts and summaries.

**This is the step that catches what nothing else does.** Everything up to here proves the model
*runs*. Only the outputs tell you whether it modelled anything. Three things worth looking at
before you call it working:

- **A column of zeros.** A parameter declared in the bundle and never written in
  `SetParameterValues` is allocated and left at zero for every element in every period. It looks
  exactly like a modelling result.
  [`../conventions/silent-failures.md` § 1](../conventions/silent-failures.md#1-a-parameter-declared-in-the-bundle-but-never-written).
- **A parameter that is suspiciously flat, or sits on a round number.** That is the clamp range in
  the `parameters` sheet, pinning it. [§ 2](../conventions/silent-failures.md#2-a-parameter-whose-clamp-range-is-too-narrow).
- **A treatment that never appears.** Either it is never triggered, or it is scheduled beyond the
  last modelled period and discarded. [§ 4](../conventions/silent-failures.md#4-a-treatment-triggered-beyond-the-last-modelled-period).

**Nothing in the framework or the tooling can tell you the forecast is wrong.** A model that
deteriorates twice as fast as reality runs perfectly and reports nothing. That judgement is the
engineer's, and it is the reason the whole design keeps the numbers in `lookups.xlsx` where they
can be changed and re-run in a minute:
[`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md).

## Step 4 — Release the lock

**Project Home → Release my lock**, when you are finished for the day. The lock blocks other
people's writes as well as protecting yours.

---

## The walking skeleton is now proven

If this was the first pass — a `--from-sample` model taken end to end — the pipeline is verified.
Everything from here on is your own engineering, and every failure from here on is attributable to
the change you just made.

Go to [`30-make-a-change.md`](30-make-a-change.md) and start replacing the sample's logic with
yours, one file at a time, keeping the build green at every step.

## Done when

- [ ] A run completed against the published model.
- [ ] The outputs contain what you expect: no unexplained zeros, no flat parameters, the
      treatments you meant to see.
