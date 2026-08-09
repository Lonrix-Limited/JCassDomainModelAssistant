# How a run works — when the framework calls what

**Use this to answer "where does this logic go?"** — the commonest question in domain-model work,
and the one that produces the worst answers when guessed.

The stages themselves, and the modelling reasoning behind them, are
[`../framework/concepts/03-execution-stages.md`](../framework/concepts/03-execution-stages.md). The
exact method signatures are
[`../framework/api/authoring/DomainModelBase.md`](../framework/api/authoring/DomainModelBase.md).
This page maps both onto **the files in front of you**, and covers the setup order, which is not
documented anywhere as a sequence and is where the expensive mistakes are.

---

## Setup, in order — this is where things go wrong

| # | What happens | What is ready |
|---|---|---|
| 1 | The framework creates your class with its **parameterless constructor** | **Nothing.** `model` is not assigned yet. Anything touching inputs, lookups or configuration throws a `NullReferenceException`. **Do no work here.** |
| 2 | The framework calls `SetupBase`, which assigns `model`, seeds `Rando`, and calls **`SetupInstance`** | Lookups, treatment rates, the budget and the configuration are **all ready**. This is where your own setup goes. |
| 3 | The framework builds the **per-element data arrays** | Only now do `NElements`, `NPeriods` and `NParameters` have real values. |
| 4 | `Initialise` is called, once per element | Everything. |
| 5 | The period loop begins | Everything. |

**The trap is step 2 to step 3.** Inside `SetupInstance`, `model.NElements`, `model.NPeriods` and
`model.NParameters` are all still **zero**. Size an array off one of them there and you get an empty
array and a model that runs to completion forecasting nothing, with no error anywhere —
[`../conventions/silent-failures.md` § 3](../conventions/silent-failures.md#3-reading-nelements-nperiods-or-nparameters-during-setup).

The framework loads the domain model **last** on purpose, precisely so that `SetupInstance` can read
project data. Lookups are not the problem there; the counts are.

---

## The per-period loop, mapped to your files

For each modelling period, for each element:

| Stage | What it decides | Scaffolded file |
|---|---|---|
| **Initialise** — once, before period 1 | What state each element starts in, read out of the raw input data | `Objects\Initialiser.cs` |
| **Trigger** | What work is *due* on this element this period, and what it would cost | `Objects\TreatmentsTrigger.cs` |
| *(the optimiser)* | Which of those candidates actually get funded, within budget | **Not yours.** The framework does this. |
| **Reset** — if a treatment was funded | How the element's condition recovers | `Objects\Resetter.cs` |
| **Increment** — if it was not | How the element decays over the period | `Objects\Incrementer.cs` |
| **Routine maintenance** — after the above | Work that happens regardless of the budget competition | `Objects\RoutineMaintenance.cs` |
| **End of period** — once per period, after all elements | Network-wide sums, rankings, proportions | `DoEndOfPeriodCalculations` on the entry class |

Supporting cast, called from the stages rather than by the framework:

| File | Holds |
|---|---|
| `Objects\<YourModel>.cs` | The entry class. A switchboard — keep it thin. |
| `Objects\<Element>.cs` | What an asset *is*: the state that carries between periods. |
| `Objects\<Element>Factory.cs` | Turns framework dictionaries into an element. **All input column names live here.** |
| `Objects\Constants.cs` | Every tunable number, read from `lookups.xlsx` at setup. |
| `Objects\TreatmentNames.cs` | Treatment name constants, shared with the bundle. |

**A model that does not follow this file layout is still valid.** The framework cares about the
methods on the entry class, not about which file they live in, and real models vary.

The reference model is one of those variations, and the difference trips people up: it folds
initialise, increment and reset into its element class, and it names its trigger
`TreatmentTrigger.cs` — **singular**, where the scaffolder and every production model write
`TreatmentsTrigger.cs`. Neither is wrong. Read whichever the engineer's own project actually has,
rather than the one you expected.

---

## Two things about the loop worth knowing before you write into it

**Your trigger proposes; the optimiser disposes.** Returning a treatment candidate is not the same
as it happening. What the optimiser does with what you return depends on the model type, and it is
worth understanding before writing a trigger that assumes its output is a decision —
[`../framework/concepts/07-bca-model.md`](../framework/concepts/07-bca-model.md).

**Nothing survives a period unless it is a model parameter.** A field on your element object is
rebuilt from the parameter store every period. If a value has to be remembered and cannot be
recomputed from the inputs and the other parameters, it must be declared on the bundle's
`parameters` sheet, written in `SetParameterValues`, and read back in the factory. Miss the middle
step and it is silently zero for the whole run —
[`../conventions/silent-failures.md` § 1](../conventions/silent-failures.md#1-a-parameter-declared-in-the-bundle-but-never-written).
The raw-input-versus-parameter distinction, if the engineer is unclear on it, is
[`../framework/concepts/04-data-components.md`](../framework/concepts/04-data-components.md).
