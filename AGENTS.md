# Agent instructions

This repository is the **Juno Cassandra Domain Model Assistant**. It gives an AI coding assistant
the context it does not otherwise have: how the Juno Cassandra framework expects a **domain model**
to be written, which of its conventions fail silently, and the tooling to check your work.

**Read [`docs/00-start-here.md`](docs/00-start-here.md) before doing anything else.** It is the
single entry point and it routes you from there.

Three things to know before you read it, because they change what you should do:

- **The engineer's model is a sibling folder, never inside this repository.** You edit their model;
  you do not edit the Assistant.
- **You do the plumbing. You do not supply engineering judgement.** Deterioration rates, trigger
  thresholds and unit rates come from the engineer, and they go in `inputs/lookups.xlsx` — never
  hard-coded into C#.
- **`model-knowledge/` is the one place in this repository you write to.** It holds what a previous
  session learned about the engineer's own models, one `<ModelName>.md` per model. Checking it is
  your first action on any model that already exists — see below.
- **You may read the client's setup files. You may not read their network data.** If they give you a
  project snapshot or an `inputs\` folder, read `lookups.xlsx`, `budgets.xlsx`, the bundle and the
  other setup files freely — that is what they are for. **`model_input_data.csv` is the exception:
  read its header row and nothing else** — see below.
- **You never write to the client's setup files, even when told to.** You change their model; the
  row that belongs in `lookups.xlsx` you hand to them, exactly — see below.

## Before you work on a model that already exists

**The engineer names a model, or says "my model", or points you at its folder. Do this first, before
reading their code.**

1. **Look for `model-knowledge/<ModelName>.md`.** The `README.md` in that folder is not a notes file
   — a folder holding only the README is a folder with no notes in it.
2. **If it is missing, list the folder that *contains* this repository** and look for a sibling
   `JCassDomainModelAssistant*-old` or `*-main` with `model-knowledge` files in it. After an update
   that is where their notes usually are.
3. **What you do next depends on what they asked for, and this is the part to get right.**

| They asked you to | You |
|---|---|
| **Change the model** — add a treatment, a parameter, an input column or a lookup value; rename; fix something | **Stop.** Ask them to copy the notes across — naming the exact folder if step 2 found one — and **wait for their reply before making the change.** Not a remark at the end, and not after the edit |
| **Answer a question** — run `check`, explain how something works, diagnose a failure, tell them whether something is right | **Answer it first.** Then say the notes file is missing and ask for it **before anything is changed** |
| **Take over a model somebody else wrote** — *"help me refactor the model in folder X"*, an inherited model | **Diagnose first, then stop.** Run `jcass-dm check` and give them what it found; then say the notes file is missing, ask for it, and **wait before any rename or refactor**. And say plainly that if the client already runs a custom model, a publish replaces it — so nothing is published while they are learning the workflow |

**Why the split.** A read-only answer cannot be wrong for lack of notes — `check` reads the project
file, the bundle and the C#, and no note an engineer writes changes what it reports. A stop in front
of it costs a round trip, buys nothing, and lands on what is often somebody's first contact with the
Assistant. **An edit is different**: the notes are where the reasons behind the model's decisions
are, and changing it without them is how a deliberate choice gets tidied away. **A takeover is both
rows in order**: the diagnosis is what they came for and cannot be wrong for lack of notes; the
refactor is the change, and waits.

If they say the model is new to them too, believe them, offer to start the file, and do not raise it
again in that session.

**And when they say "remember that..." about their model, that goes in
`model-knowledge/<ModelName>.md` too — an ordinary edit to a file in this repository.** Not into a
memory feature of your own, wherever your tool keeps one. A note the engineer cannot open is a note
they cannot correct, and the next session may not be the same assistant you are. Say which file you
wrote it to.

**Why this is a stop, what to write in the file, and what belongs upstream to Lonrix instead:**
[`docs/00-start-here.md`](docs/00-start-here.md) § 4.

## The client's data: read the setup, never write it, never read the network data

**This is a hard line and it is easy to cross while being helpful.** An engineer who hands you a
project snapshot is asking you to check their C# against their setup, and that is exactly right:
lookup sets, budget columns, input column names, sheet contents — read them, and cross-check freely.
[`docs/conventions/input-files-in-scope.md`](docs/conventions/input-files-in-scope.md) is the full
rule, and it is worth opening.

**Three things you never do, whatever the engineer asks:**

1. **Never read the rows of `model_input_data.csv`.** The header row is what you need — it tells you
   which columns exist. The rows are a client's asset register, tens of thousands of them. `head -1`,
   `Get-Content -TotalCount 1`, `Read` with a limit of 1. **No `cat`, no whole-file `Read`, and no
   `awk`, `wc`, `sort` or anything else that walks the rows** — including to count them or to
   summarise them "just for context". The same goes for anything in `outputs\`.
2. **Never tell them what condition their network is in.** Not a distribution, not an average, not a
   percentage in poor condition, not "a moderately worn network". That is engineering judgement about
   their own assets, and it is theirs — the same rule as refusing to name a deterioration rate.
   **Decline it and point at the web app's Analyse Input page**, which is built for that question and
   does it properly.
3. **Never write to the client's setup files** — `lookups.xlsx`, `budgets.xlsx`, anything in
   `inputs\` or `supporting\`, wherever the folder is. **This holds when they say *"do it"*.** Do the
   C# in full, then give them the row to add — sheet, set name, key, value, comment — and say you
   have left the file for them to edit in Excel and upload on **Files → Inputs**. The Tuning page
   changes a value that already exists; it cannot add a row. No Python, no helper project, and no
   script for them to run instead. **Why:** a workbook rewritten by anything but Excel can break rows
   you never touched — `openpyxl` saves every formula with an empty result, Excel recalculates on
   open so the file looks fine, and the model run reads blanks without an error. The bundle,
   `domain_model_setup.xlsx`, is part of the model, and the `jcass-dm` verbs still write it.

**The second one usually arrives as an afterthought** — *"…and while you're in there, tell me what
sort of state the network is in"* — attached to a request you were right to say yes to. Answer the
first half; decline the second, in one sentence, and name Analyse Input.

## A build that fails with CS1705, and the engineer's project file

**CS1705 — *"… has a higher version than referenced assembly"* — means their model still targets
.NET 9 and this Assistant's references are .NET 10.** The fix is one line in their `.csproj`, and it
is still their file: **ask before you edit it, even when they said *"fix it"*.**

1. **Say what happened in one sentence**, and say that **what they have already published keeps
   running** — this is only about building on their machine.
2. **Check for a .NET 10 SDK** — `dotnet --list-sdks`, in their PowerShell terminal. No line starting
   `10.` → stop, and point them at the SDK download in `docs/orientation/prerequisites.md`.
3. **Ask, naming the one line** — `<TargetFramework>net9.0</TargetFramework>` to
   `<TargetFramework>net10.0</TargetFramework>` in their `.csproj` — **and wait for the yes.** Then
   change that line and nothing else. Never a `<Reference>`, a `HintPath` or a package version, and
   never older assemblies copied into `refs\`: each makes the build pass on a framework nobody
   documents.
4. **Refresh refs, rebuild, run `jcass-dm check`**, and tell them the `target framework` rule now
   reads OK.

**Why the ask, when the edit is obviously right:** an unexpected change to a project file is the kind
an engineer cannot check, and *"fix it"* was said about an error they could not read, not about their
`.csproj`. The full entry is [`docs/orientation/reading-errors.md`](docs/orientation/reading-errors.md) § CS1705.

## Everything else

Conventions, patterns, the `jcass-dm` tool, when to stop and escalate — all in `docs/`.

**If you are changing the Assistant itself** — a document, a convention, a skill, or the `jcass-dm`
tool — read [`docs/design-rules.md`](docs/design-rules.md) **first**. It carries the twenty-eight
design rules and, more importantly, the reasoning behind each one.

Beyond the procedure above, this file carries no knowledge of its own: three copies of a fact become
three different facts.
