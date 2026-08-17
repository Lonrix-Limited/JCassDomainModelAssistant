# Silent failures — the checklist

**An error is self-correcting. Silence is not.** Every entry below is a way a domain model can be
wrong while the run completes, the outputs look plausible, and nothing anywhere says a word. These
are the reason this repository exists.

**Work through this list before you tell an engineer their model is finished.** Most of it is one
command:

```powershell
.\tools\jcass-dm.exe check --project ..\TheirModel --lookups <path to their inputs\lookups.xlsx>
```

Five entries are **not** mechanically detectable and are marked so. Those are the ones to raise out
loud, because nothing will raise them for you.

---

## Quick index

| # | Symptom the engineer sees | Caught by |
|---|---|---|
| [1](#1-a-parameter-declared-in-the-bundle-but-never-written) | An output column is all zeros | `jcass-dm check` — **only** this |
| [2](#2-a-parameter-whose-clamp-range-is-too-narrow) | A parameter is flat, or pinned to a round number | Nothing (degenerate ranges only) |
| [3](#3-reading-nelements-nperiods-or-nparameters-during-setup) | The model runs and forecasts nothing | Nothing |
| [4](#4-a-treatment-triggered-beyond-the-last-modelled-period) | A scheduled follow-up never appears | The run log |
| [5](#5-an-input-column-or-parameter-added-to-one-factory-method-but-not-the-other) | Right in period 0, wrong from period 1 | Nothing |
| [6](#6-assemblyname-set-in-the-csproj) | F5: *class not found in the specified .dll* | `jcass-dm check` |
| [7](#7-two-csproj-files-at-the-project-root) | F5 refuses to start, or builds the wrong project | `jcass-dm check` |
| [8](#8-a-refs-folder-inside-the-upload-zip) | F5 fails with `BadImageFormatException` | `jcass-dm package` |
| [9](#9-a-budget-category-passed-to-assignbudgetcategoryfractions-that-does-not-exist) | Run dies mid-way, `KeyNotFoundException` naming nothing | Nothing |
| [10](#10-a-privately-constructed-random-instead-of-rando) | The same run gives different answers | Nothing |
| [11](#11-a-treatment-with-no-arm-in-the-reset-switch) | A treatment is funded and reported, and changes nothing | `jcass-dm check` — when the reset is a `switch`. Loud in a scaffolded model; silent in some inherited ones |
| [12](#12-reading-a-model-parameter-at-or-above-the-period-you-were-handed) | A look-back calculation quietly reads zero | Nothing, outside a benefit-cost rollout. Inside one it throws |

---

## 1. A parameter declared in the bundle but never written

**Symptom.** The run completes. In the outputs, that parameter is `0` for every element in every
period — a column of zeros that looks exactly like a modelling result nobody has looked at closely.

**Cause.** The `parameters` sheet of `domain_model_setup.xlsx` declares it, so the framework
allocates storage for it. The element's `SetParameterValues` never writes it, so the storage stays
at its default. A text parameter comes out null rather than zero, which may surface later as an
unrelated-looking crash.

**Nothing in the framework catches this.** There is no setup rule for it. The reverse mistake —
writing a parameter the bundle does *not* declare — throws immediately, by name, which is why this
one is so easy to assume is covered too. It is not.

**Detection.**

```
jcass-dm check → "parameters vs C#"
```

That rule is **the only defence there is.** Treat a `parameters vs C#` finding as a bug, never as a
tidiness note. Note also that it can only see parameters written with a literal name; a name
assembled at run time is invisible to it.

---

## 2. A parameter whose clamp range is too narrow

**Symptom.** A parameter comes out flat, or sits exactly on a round number for long stretches, or a
deterioration curve mysteriously stops rising.

**Cause.** `minimum` and `maximum` on the `parameters` sheet are **clamps, not validation bounds**.
Every value written is forced into the range rather than rejected. That is deliberate — one
out-of-range calculation must not abort a whole run — but it means a range that is merely wrong
produces a wrong forecast rather than an error.

**Partly detected.** The framework rejects a *degenerate* range — minimum above maximum, or the two
equal — at setup, naming the parameter, because that would flatten the parameter entirely. A range
that is simply too narrow is not checked by anything, and cannot be: only the modeller knows what
the quantity can genuinely take.

**Detection.** `jcass-dm add-parameter` refuses to write a numeric parameter with no `--min` and
`--max`, so the range is at least a decision rather than a default. After that it is inspection: if
a parameter looks suspiciously flat, open its row first.

---

## 3. Reading `NElements`, `NPeriods` or `NParameters` during setup

**Symptom.** The model builds, loads, runs to completion in a normal amount of time, and forecasts
nothing. Empty outputs, or arrays with no entries, and no error anywhere.

**Cause.** Those three counts are **all still zero inside `SetupInstance`** — the framework builds
the per-element data arrays after it returns. Sizing an array off one of them at setup gives an
array of length zero; dividing by one gives an infinity rather than an exception. The full setup
order is [`../orientation/how-a-run-works.md`](../orientation/how-a-run-works.md).

Note that **lookups are not the problem here** — they are fully available at setup, and the failure
people expect in this area is the wrong one.

**Nothing catches this.** Read those counts from `Initialise` onwards, never at setup.

---

## 4. A treatment triggered beyond the last modelled period

**Symptom.** A domain model schedules a follow-up treatment some periods out — a reseal five years
after a rehabilitation, say. For elements treated late in the run, the follow-up simply never
appears. Not in the outputs, not in the costs, not in the budget.

**Cause.** The framework only accepts a treatment whose period is within the modelling horizon.
Beyond it, the treatment is discarded. The guard is correct and necessary — the budget is
dimensioned to the number of periods, so an out-of-range period would otherwise crash the run —
but the discard itself is silent.

**The defence is in your code, and it is the one to rely on.** No static check can catch this:
the offending period is computed at run time from the client's own data, which `jcass-dm` never
sees. So **compare a scheduled period against `model.NPeriods` before you schedule it**, and decide
deliberately whether to clamp it, drop it, or let it go. `NPeriods` is on
[`../framework/api/authoring/ModelBase.md`](../framework/api/authoring/ModelBase.md).

**Secondary detection: the run log.** Recent framework builds write a warning naming the treatment,
the period it was asked for, the element and the model's own horizon — shown once and then
suppressed, since it can otherwise fire once per element per period. **Do not read the absence of
that warning as proof it did not happen.** If you expect one and there is none, the framework
running the job may be older than the build stamped in
[`../../refs/FRAMEWORK-VERSION.txt`](../../refs/FRAMEWORK-VERSION.txt), in which case the discard is
still completely silent and the check above is all you have.

---

## 5. An input column or parameter added to one factory method but not the other

**Symptom.** The model is correct in period 0 and wrong from period 1 onwards. Condition curves
start sensibly and then bend, or a property is populated at the start of the run and zero
afterwards.

**Cause.** The element factory has **two** ways of building an element: one from the raw input data
at the start of the run, and one from the model parameter data in every period after that. Adding a
property to the first and forgetting the second is the classic domain-model bug, and both methods
compile perfectly.

**Nothing catches this.** Neither the framework nor `jcass-dm` compares the two methods.

**The habit that prevents it:** when you touch one factory method, open the other in the same edit,
every time. If you are working in guided mode, make that the step — have the engineer read both
methods side by side and say which properties appear in only one.

---

## 6. `<AssemblyName>` set in the `.csproj`

**Symptom.** Everything builds and everything looks right, and then F5 on the Debug Model page
fails with:

```
Domain Model class 'Whatever' was not found in the specified .dll
```

**Cause.** A model is loaded by two different routes that disagree about where its name comes from,
and `<AssemblyName>` is the one setting that can make them disagree silently — the `.csproj` still
carries the right filename while the DLL comes out under another name. The four names, and why the
two routes differ, are explained once in
[`four-names.md`](four-names.md).

**Detection.**

```
jcass-dm check → "<AssemblyName>"
```

Leave it unset. The assembly name is inherited from the `.csproj` filename, which is what you want.

---

## 7. Two `.csproj` files at the project root

**Symptom.** F5 refuses to start, or builds a project the engineer was not expecting.

**Cause.** The debug workspace is rooted at exactly one `.csproj` and will not guess between two. A
second one arrives easily — a copied project, an abandoned experiment, a backup saved beside the
original.

**Detection.**

```
jcass-dm check → "one .csproj at the root"
```

`check` reports this and carries on, deliberately, because it is often the first command run on an
inherited model. `rename` and `package` refuse outright.

---

## 8. A `refs/` folder inside the upload zip

**Symptom.** F5 fails with a `BadImageFormatException` about reference assemblies not being
loadable for execution — and it looks like nothing to do with a zip.

**Cause.** The reference assemblies in `refs/` carry the framework's public API with **no method
bodies**. They compile and give IntelliSense; they cannot be executed, by design. The debug
workspace stages its own, larger, runnable set. Uploading yours overwrites part of that set with
assemblies that will not load.

**Detection.** Do not build the zip by hand:

```powershell
.\tools\jcass-dm.exe package --project ..\TheirModel
```

`package` excludes `refs\` for you, and also gets the other half of this mistake right — the zip has
to open straight to the `.csproj` rather than to a folder containing it. Both halves, and what goes
in the *other* kind of zip, are in
[`naming-and-folders.md` § the two zips](naming-and-folders.md#part-3--the-two-zips).

---

## 9. A budget category passed to `AssignBudgetCategoryFractions` that does not exist

**Symptom.** The run dies part-way through with a bare `KeyNotFoundException` that names nothing —
no category, no treatment, no element.

**Cause.** This is the one place a budget category name is **not** validated at setup. A treatment
type's own `budget_category`, declared in the bundle, *is* checked: a category with no matching
column in the client's `budgets.xlsx` stops the run at setup with a message naming the treatment.
But the keys of the dictionary you pass to `AssignBudgetCategoryFractions` are supplied at run time,
so nothing can check them in advance.

Loud, then, rather than silent — but uselessly so, which is why it is on this list.

**The diagnosis to give.** Do not go looking for money that vanished. Compare your fraction keys
against `model.Budget.BudgetCategories`, which is the authoritative list of what exists. See
[`../framework/api/authoring/Budget.md`](../framework/api/authoring/Budget.md) and
[`../framework/api/authoring/TreatmentInstance.md`](../framework/api/authoring/TreatmentInstance.md).

**Nothing can catch this statically**, in the framework or in `jcass-dm`. The nearest useful habit
is to build fraction keys from the same constants the bundle rows were written from, never from
string literals typed a second time.

---

## 10. A privately constructed `Random` instead of `Rando`

**Symptom.** The same model, the same inputs and the same seed give a different forecast each time
it is run. Usually noticed when somebody tries to reproduce a result, often months later.

**Cause.** `DomainModelBase` gives you a `Rando` field seeded from the model's configured random
seed — that seeding is what makes a run reproducible. A `new Random()` written anywhere in the
domain model is seeded from the clock instead. The model still runs, and it quietly stops giving the
same answer twice.

**Nothing catches this.** Use `Rando`; never construct a `Random`. See
[`../framework/api/authoring/DomainModelBase.md`](../framework/api/authoring/DomainModelBase.md).

---

## 11. A treatment with no arm in the reset switch

**Symptom.** The treatment triggers, is funded, is charged to a budget and appears in the outputs as
delivered — and the element it was applied to carries on deteriorating exactly as if nothing had
happened. The forecast shows the cost and none of the benefit.

**Cause.** `Reset` is where a treatment's effect on the element is written. A treatment name that
falls through the switch changes nothing. The trigger still fires, so the treatment is visibly
*there* in the outputs — which is why this is the one of the five places in
[`../workflow/30-make-a-change.md`](../workflow/30-make-a-change.md#add-a-treatment) with no other
symptom.

**In a scaffolded model this is loud, not silent.** The `default:` arm throws, naming the treatment,
the first time one is selected with no arm. That is the whole reason it throws.

**It is silent in an inherited model that does it differently** — a `default:` that does nothing, an
if/else chain with no final `else`, or a dictionary lookup with a fallback. That is the adoption
case ([`../workflow/05-adopt-an-existing-model.md`](../workflow/05-adopt-an-existing-model.md)), and
it is where this entry earns its place on the list.

**`jcass-dm check` catches it** by comparing the bundle's `treatment_name` column against the case
arms it can find in the source. It reports `SKIPPED` rather than passing when the reset uses an
if/else chain or a dictionary instead of a `switch`, since it cannot read those — a valid way to
write it, and one you then have to check by hand.

**Two habits that go with the check:**

- **Make the `default` arm throw**, and if you are adopting a model whose default does not, change
  it. [`../patterns/routine-maintenance.md`](../patterns/routine-maintenance.md) has the shape.
- **Give a deliberately-does-nothing treatment an empty case, not a fall-through.** Routine
  maintenance usually changes nothing structural. Writing the empty case says so; letting it fall
  through says nothing and is indistinguishable from the bug.

---

## 12. Reading a model parameter at or above the period you were handed

**Symptom.** A rule that looks back — *how much has this element deteriorated since last period*,
*what was its condition before the last treatment* — comes out as though the element had a value of
zero back then. Nothing errors. The run completes in a normal time, every output column is populated,
and the forecast is simply wrong in a way that looks like modelling.

**Cause.** An **epoch is the end of a period**, and when the framework calls your domain model for
period `iPeriod`, that period has not been stepped yet. So:

> **The highest epoch you may read is `iPeriod - 1`.**

That holds for **every element, not only the one you were called about**, and it holds in every model
type — forecast, Monte Carlo, MCDA, benefit-cost — whether the run is serial or parallel. Epoch
`iPeriod` and above is storage the framework has allocated and not yet written. Numeric parameters
read back as `0.0` and text parameters as `""`.

```csharp
// Called for period iPeriod.
double previous = model.GetParameterValueNumber(iElem, ParameterNames.Condition, iPeriod - 1);  // correct
double current  = model.GetParameterValueNumber(iElem, ParameterNames.Condition, iPeriod);      // zero, silently
```

The three readers behave identically here: `GetParameterValueNumber`, `GetParameterValueText` and
`GetParameterValues`. All three are on
[`../framework/api/authoring/ModelBase.md`](../framework/api/authoring/ModelBase.md), where the rule
is written out in full.

**Nothing catches this outside a benefit-cost run.** There is no setup rule, no log line and no
exception — `jcass-dm` cannot see it either, because the epoch is computed at run time. The guard
described below is deliberately scoped to strategy rollouts only, so **for every other model type
this page is the only thing standing between you and the zeros.**

**The values you were passed are almost always the right answer.** The framework hands `Increment`
and `Reset` the current parameter values already. Reach for a reader when you genuinely need history
the call did not give you, and then only at `iPeriod - 1` or earlier.

**Inside a benefit-cost strategy rollout it is enforced instead**, and the exception is worth
recognising because it names everything:

```
Cannot read model parameter data for element 41 at epoch 12 during a BCA strategy rollout
on element 41 (base period 9, look-ahead period 4, modelling period 12). The strategy has
only reached epoch 11 ...
```

That is the guard doing its job, not a framework fault. A rollout is a what-if about **one** element,
so during it:

- **the element being rolled out** can be read back to any epoch the strategy has already passed —
  including epochs past the end of the model — and the answer comes from *that strategy's own*
  timeline. Two sibling strategies giving different answers for the same epoch is correct and is the
  entire point;
- **its own current or later epoch** is refused, because the strategy has not decided it yet;
- **any other element at or beyond the rollout's base period** is refused, because the main run has
  not stepped that far and there is nothing but zeros there.

**Note the asymmetry with treatments**, because the two rules look alike and end opposite ways. A
*treatment* question about another element during a rollout is answered from the real network, and
that is right — the real treatment set is merely **sparse**, and a period with no treatment in it
genuinely has no treatment in it. A *parameter* question about another element is refused, because
the real parameter array ahead of the main run is **empty**, and empty reads back as a number. Do not
infer one rule from the other.

**One thing that is not the rule**, and is worth saying because it is the plausible-sounding version
somebody will reach for: *"only read element indexes below your own"* is **wrong**. Elements are
stepped with `Parallel.For` when the run is configured for it, so index order guarantees nothing
about what has been computed. `iPeriod - 1` is the whole rule and it needs no ordering assumption.

---

## What `jcass-dm check` does not cover

`check` is a **local subset**, and it says so in its own output. It reads the project folder — the
`.csproj`, the C# as text, and the bundle. It cannot see the client's input CSV, their budget
columns or their configuration, and it does not compile anything, so anything assembled at run time
is invisible to it. A rule it could not apply reports as `SKIPPED` rather than passing.

The web app's **Check Setup** page is authoritative and sees the client's actual data. A green
`check` means *"nothing locally visible is wrong"*, not *"this will run"*.

---

## Found a silent failure that is not here?

**Report it to `support@lonrix.com`** — do not add it to this file. This repository is replaced
wholesale at every update, so a local edit is lost; a reported one reaches every other client.

Say plainly whether anything detects it. **An entry with no detection mechanism is an entry that
gets forgotten** — five on this list have none, and those are the ones to raise in conversation
rather than trust a checklist to surface. If a new one looks mechanically checkable, say so: that
is a gap in `jcass-dm` and it can be closed.
