# The client's setup files are in scope — as evidence, never as data

**Ask for the project snapshot. Read the setup files in it. Do not read the network data.**

Those three sentences are the whole rule, and the third one is the one that needs the reasoning,
because it looks like caution and is actually the difference between the rule being usable and being
switched off.

Where the snapshot comes from, and how to unzip it:
[`../workflow/02-the-starter-model.md`](../workflow/02-the-starter-model.md) § step 3.

---

## Why an assistant needs them at all

A domain model is C# **plus** a set of spreadsheets, and half the ways it fails are disagreements
between the two. A treatment charged to a budget category with no column in `budgets.xlsx` fails at
setup, naming the treatment. A `Constants` property reading a lookup set that is not in
`lookups.xlsx` fails at setup, naming the set. An input column the factory reads that is not in the
network data CSV's header fails, or worse, quietly reads a blank.

**Every one of those is invisible to somebody looking only at the C#.** They are discovered at an
upload and an F5, an hour and a round trip after the mistake was made, and each one is trivially
detectable when the two halves are in front of you at once.

So: ask for the snapshot folder — or, when only one file is needed,
**Files → Inputs** in the web app downloads them individually.

> **Ask for the folder; do not go looking for it.** The engineer names the path. Do not guess at one,
> and do not go hunting through their disk for something that looks like a Juno Cassandra project.

---

## What is in `inputs\`, and what each file settles

**Five files the framework refuses to start a run without:**

| File | Settles |
|---|---|
| `model_input_data.csv` | The network. One row per element. **Its header row is what the factory can read.** |
| `configurations.xlsx` | Run settings — model type, number of periods, discount rate, which budget sheet is used |
| `lookups.xlsx` | Every tunable number, as `(lookup_set_name, setting_key, setting_value, comment)` across the `lkp_*` sheets. **Unit rates live in `lkp_unit_rates`** — [`where-numbers-live.md`](where-numbers-live.md) |
| `budgets.xlsx` | One column per budget category, one row per period. **Every treatment's `budget_category` has to name a column here** |
| `kpi_setups.xlsx` | KPI definitions for post-processing, and the grouping setup |

**And the optional ones, which are worth knowing exist:**

| File | Settles |
|---|---|
| `mcda_setups.xlsx` | Weights and objective types for MCDA optimisation |
| `goalseek_setup.xlsx` | Variable ranges and objectives for goal-seeking runs |
| `multi_column_lookups.xlsx` | Lookup *tables* rather than scalars — each `lkp_*` sheet is read whole |
| `committeds.csv` | Treatments already committed, loaded ahead of the optimiser |
| `styles.xlsx` | Presentation only — treatment colours and traffic-light bins for the result tables. Three independently optional sheets: `treatment_colours`, `input_columns`, `model_parameters`. Nothing here changes a forecast |
| `budget_categories.xlsx` | Per-client reassignment of a treatment's budget category, overriding the bundle |

A file in `inputs\` whose name is not on either list is not read by the framework at all — it is a
candidate for `supporting\`, which is where side-car data belongs
([`naming-and-folders.md`](naming-and-folders.md)).

---

## The line: evidence, not data

**You may read a setup file to find out what it declares. You may not read the network data to find
out what it says.**

| Fine | Not fine |
|---|---|
| The set names and keys in `lookups.xlsx` | Anything about the *distribution* of a value |
| The column names in `budgets.xlsx`, and the periods | Totals, averages, trends |
| **The header row** of `model_input_data.csv` | **Its rows.** Any of them, in any quantity |
| Sheet names, and which optional files exist | A judgement about the network's condition |
| The `comment` column, because it says what a number means | Statistics of any kind, however small |

**Why the network data is a hard no, and it is two separate reasons.**

**It is enormous.** `model_input_data.csv` is one row per asset — tens of thousands of rows on a real
network, each with dozens of columns. *"Read all the files in `inputs\`"*, taken literally, pulls the
whole thing into a conversation, which is slow, expensive, and pushes out the context you were
actually using. **Read the header row and stop.** `Get-Content -TotalCount 1` is the whole
interaction:

```powershell
Get-Content C:\work\<snapshot>\inputs\model_input_data.csv -TotalCount 1
```

**And it is not yours to interpret.** It is a client's asset register. The moment an assistant starts
profiling it — *"63% of your chipseal is over 15 years old, so I'd suggest..."* — it has crossed from
plumbing into engineering judgement, which is the one thing this Assistant never supplies
([`../00-start-here.md`](../00-start-here.md) § 1). The engineer decides what the network needs. The
web app has an **Analyse Input** page built for exactly that question, with proper statistics and
histograms, and it is the right answer to *"what does my data look like?"* every time.

**The same restraint applies to the outputs.** A snapshot carries `outputs\`, `logs\` and
`r-outputs\` from the last run. Nothing on this path needs them, they are the largest thing in the
zip, and reading a forecast to form an opinion about it is the same crossing in a different folder.

---

## What to do with them, in order

**1. Check the C# against them.**

```powershell
.\tools\jcass-dm.exe check --project ..\NelsonRoads --lookups C:\work\<snapshot>\inputs\lookups.xlsx
```

`--lookups` is what turns the `lookup sets` rule from `SKIPPED` into a real comparison, and it is the
rule most worth having. Without the file it cannot run at all, which is why asking for the snapshot
is the first thing rather than an optimisation.

The budget-category rule is a `NOTE` for the same reason in reverse: `jcass-dm` does not read
`budgets.xlsx`. **You can, now.** Open it, list the columns, and compare them yourself against the
`budget_category` values in the bundle's `treatments` sheet, which is one command:

```powershell
.\tools\jcass-dm.exe dump ..\NelsonRoads\domain_model_setup.xlsx --sheet treatments
```

Then say plainly which categories match a column and which do not. A mismatch stops the run at setup
naming the treatment, so finding it here saves an upload and a wait.

**2. Send them to the authority for anything that matters.** The web app's **Check Setup**, on the
Tuning page's Configurations tab, runs the framework's own loader over the client's real files and
reports twenty-five named checks. **Its answer beats yours**, always
([`../design-rules.md`](../design-rules.md) rule 6), and it sees things a local check cannot: the
per-tag configuration merge, the KPI and MCDA setups, the styles file, the budget-category overrides.
There is a second button with the same validators — **Check bundle**, on the Debug Model page — which
runs them against the bundle being edited rather than the last one staged.

**Do not reimplement any of that locally.** Two implementations of one rule drift, and the one the
engineer runs first is the one that will be wrong.

**3. Let them make the edits.** These are the client's files and the engineer's job. Guide, explain
what a sheet is for, say exactly which row to add and where — and prefer the **Tuning** page over
Excel whenever both would work, because that is the route they will use again every time they
recalibrate. An engineer who has never added a lookup row cannot maintain the model.

**4. Never write into the snapshot.** It is a download, it is stale the moment the client changes
anything, and an edit there reaches nothing. Real changes go through the web app, or through a file
uploaded on **Files → Inputs**.

---

## Related

- [`../workflow/02-the-starter-model.md`](../workflow/02-the-starter-model.md) — where the snapshot comes from
- [`where-numbers-live.md`](where-numbers-live.md) — which file a number belongs in
- [`naming-and-folders.md`](naming-and-folders.md) — `inputs\` versus `supporting\` versus the bundle
- [`silent-failures.md`](silent-failures.md) — the disagreements between C# and setup that nothing reports
- [`../design-rules.md`](../design-rules.md) — rule 6 (the web app is authoritative) and rule 28
