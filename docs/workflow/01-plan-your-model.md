# 01 — Plan the model before you build it

**This page is engineering, not code.** Nothing here is typed into a terminal. It is the half hour
with a spreadsheet that decides whether the next fortnight goes well, and it is the step engineers
most often skip because it does not feel like progress.

Work through the four guidelines below. When you can answer all four, go to
[`10-scaffold-and-build.md`](10-scaffold-and-build.md) and the model almost writes itself. Arrive at
step 10 without them and you will be designing the model and learning the framework at the same
time, and every problem will look like both.

**Adopting a model somebody else wrote?** This page is still worth twenty minutes — read it as a set
of questions to ask about the model you have inherited. Then go to
[`05-adopt-an-existing-model.md`](05-adopt-an-existing-model.md).

---

## 1. Start simple, and get the simple version running end to end first

> **Gall's Law:** *a complex system that works is invariably found to have evolved from a simple
> system that worked. A complex system designed from scratch never works and cannot be patched up
> to make it work.*

Your first model should be **almost embarrassingly simple** — one or two treatments, a handful of
input columns, two or three parameters. Its job is not to forecast anything useful. Its job is to
prove that the whole path works on your machine, in your client, with your account:

- you can build it locally;
- you can upload it and initialise the debug workspace;
- you can put a **breakpoint** in your own code and have it stop there;
- you can **publish** it into your client's workspace and run it.

**Only then** start adding parameters, treatments and rules — one at a time, with a working build
after each. Every problem you hit from that point on has one obvious cause, because everything else
was working ten minutes ago.

The alternative is the trap: two weeks of modelling, a first upload, a failure at F5, and no way to
tell whether the C# is wrong, the bundle is wrong, the input data is wrong or the permissions are
wrong. Four unknowns and no way to separate them.

This is also why [`10-scaffold-and-build.md`](10-scaffold-and-build.md) starts you with
`scaffold --from-sample` rather than an empty project — it hands you a model that already runs, so
the pipeline can be proven before you have written a line. See
[`README.md`](README.md#the-walking-skeleton--do-this-before-you-model-anything).

## 2. Know your three lists before you write any code

Before the simple starter model, you need three things settled. A spreadsheet is the right tool, and
you will transfer these into the model's `domain_model_setup.xlsx` bundle once the Assistant has
scaffolded it.

### List A — the treatments

Every distinct piece of work the model can decide to do: `reseal`, `rehabilitation`,
`replacement`, `do_nothing`. For each one, note what it costs and which budget it comes out of.

**Start with two or three.** A model with twenty treatments and no working pipeline is much harder
to debug than the same model built up two treatments at a time.

### List B — the input columns

What the model needs to know about each asset **at the start of the run** in order to set up: age,
length, width, surface type, last condition survey, whatever your engineering needs. These become
columns in the client's network data CSV, and they are read in one place in the code.

For each, note the column name, whether it is a number or text, and what it means. Watch for the
column that is *nearly* always present — a model that assumes a value which is blank on 3% of rows
fails in a way that is tedious to trace.

### List C — the model parameters

**This is the list engineers under-think, and it is the most important of the three.** A model
parameter is a value the model carries from one period to the next and updates as it goes: surface
age, roughness, rut depth, condition index, years since last treatment.

The test that separates List B from List C:

> **Does this change as the model steps into the future?**

**No** — it describes the asset and is read once at the start → **input column** (List B).
**Yes** — the model updates it each period → **model parameter** (List C).

Some values are both: a *current* roughness read in at period 0 as an input column, then updated
every period thereafter as a parameter. That is normal and expected.

Numeric parameter names conventionally start `par_` —
[`../conventions/naming-and-folders.md`](../conventions/naming-and-folders.md).

## 3. For every parameter, know how it moves and how it resets

For each parameter in List C, you need two rules. Write them on paper, as a flow chart, or as
equations — whichever you think in. The form does not matter; having them ready does.

| Rule | The question it answers |
|---|---|
| **The increment** | With **no treatment** applied, what is this parameter at the end of the period, given what it was at the start? |
| **The reset** | When **each** treatment is applied, what does this parameter become? |

The reset rule is per parameter **per treatment**. A rehabilitation and a reseal do not do the same
thing to roughness, and saying so early is much cheaper than discovering it in the forecast.

A useful grid to fill in — parameters down the side, treatments across the top, one reset rule in
each cell:

| | reseal | rehabilitation | replacement |
|---|---|---|---|
| `par_surface_age` | back to 0 | back to 0 | back to 0 |
| `par_roughness` | unchanged | improves by *x* | back to as-new |
| `par_rutting` | ... | ... | ... |

**If a reset rule is complicated, write down the simple version first** and build the model with
that. "Roughness goes back to as-new" is a fine starting rule even when you know the truth is a
function of the pre-treatment value, the material and the traffic. Get the simple rule running,
confirm the forecast moves the way you expect, then add the nuance. The complicated version written
first is untestable, because there is nothing working to compare it against.

Which file each of these two rules lives in: `Incrementer.cs` and `Resetter.cs` —
[`../orientation/how-a-run-works.md`](../orientation/how-a-run-works.md).

## 4. List your thresholds and constants separately, and group them

Any specific number in your rules is a **constant**, and the ones you will change while calibrating
are **thresholds**. *"If rut depth is greater than 10 mm, trigger a rehabilitation"* — the 10 is a
threshold, and it belongs on this list rather than buried in the sentence.

List them separately from the rules, because they go somewhere else entirely: **the client's
`inputs\lookups.xlsx`**, not the C# code. That is what lets you change 10 mm to 12 mm on the web
app's **Tuning** page and re-run in minutes, instead of needing a code change, a rebuild and a
republish for every calibration step. The rule and its boundaries:
[`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md).

**Group them from the beginning.** A `lookups.xlsx` value is addressed by a **set name** and a
**key**, and the set name is yours to choose. A mature model has hundreds of constants, and the
difference between a model you can calibrate and one nobody dares touch is largely whether they were
grouped on day one:

| Set name | Holds |
|---|---|
| `trigger_thresholds` | the condition and age limits that trigger work |
| `deterioration_rates` | how fast each parameter moves per period |
| `unit_rates` | what each treatment costs per unit |
| `candidate_selection` | anything governing which candidates are offered to the optimiser |
| `reset_values` | what condition each treatment restores an asset to |

Names are entirely up to you — group by what a modeller would want to see together when they open
the file to recalibrate. Sets can be reorganised later without touching code, so this is a cheap
decision to get roughly right and an expensive one to skip.

Bring the list as three columns and your assistant can turn it into lookup rows and the matching
`Constants.cs` properties directly:

| `lookup_set_name` | `setting_key` | `setting_value` |
|---|---|---|
| `trigger_thresholds` | `rut_depth_gt` | `10` |
| `trigger_thresholds` | `surface_age_gt` | `12` |
| `unit_rates` | `reseal_per_m2` | `18.5` |

---

## Ready when

- [ ] You have a **deliberately simple** first model in mind, and you know it is not the final one.
- [ ] **List A** — two or three treatments, with a cost basis and a budget for each.
- [ ] **List B** — the input columns needed to set an asset up at period 0.
- [ ] **List C** — the parameters the model carries forward, each one passing the *does it change as
      the model steps forward?* test.
- [ ] An **increment rule** for every parameter, and a **reset rule** for every parameter/treatment
      pair — simple versions are fine and are preferred.
- [ ] A **grouped list of thresholds and constants**, kept separate from the rules.

Next: [`10-scaffold-and-build.md`](10-scaffold-and-build.md).

---

## For the assistant — walk these one at a time, and do not fill them in

When an engineer says *"I want to start a new domain model"*, **this page comes before any command
is typed.** Do not scaffold first and plan afterwards; the scaffolded bundle is where these answers
land, so the ordering is real rather than pedagogical.

Take the four guidelines **one at a time**, and wait for an answer before moving on. Reading all four
at somebody in one message produces agreement and no lists.

**The content of the lists is theirs, not yours.** Which treatments a network needs, what
deteriorates and how fast, what a reseal costs — that is the engineering judgement you never supply
([`00-start-here.md` § 1](../00-start-here.md)). You are helping them see the *shape* of the answer,
and specifically:

- pushing back, once and politely, when the first model is not simple — five treatments and fifteen
  parameters before anything has run is the failure this page exists to prevent;
- catching values that are on the wrong list — the input column that is really a parameter is the
  one that costs a rewrite later;
- **recognising a set of fitted coefficients when you see one**, and saying so straight away. A
  regression with eleven constants is not eleven lookup rows; it is a CSV in `supporting\` —
  [`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md) § the third tier.

If they would rather read the page themselves than be walked through it, point them at it and pick
up at the checklist. Either is fine. Skipping it is not.
