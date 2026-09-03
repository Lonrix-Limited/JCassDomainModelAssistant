# Changelog

What changed in each release of the Domain Model Assistant, and — the part worth your time —
**what to re-check in your own model** because of it.

**Releases are named by date**, because the only question anyone actually asks is *"is the one I am
holding older than the one on GitHub?"* and two dates answer that without anybody having to know a
numbering convention. Your copy's date is in [`ASSISTANT-VERSION.txt`](ASSISTANT-VERSION.txt) at the
root, beside the commit it was cut from. Note that the `jcass-dm` tool carries a version number of
its own (`jcass-dm version`) and it is a different thing.

**Taking a newer release without losing anything is a short procedure, not a straight swap** —
[`docs/orientation/updating-the-assistant.md`](docs/orientation/updating-the-assistant.md). Read it
before you download.

---

## Unreleased

**Headline: you no longer start a model from scratch — Lonrix sets up a starter model in your
project and you begin from that, with your own setup files open beside it.**

### Changed — the way you start, and it is a real change

- **There is no "start from scratch" any more, and your assistant will not offer one.** A custom
  domain model cannot exist in Juno Cassandra on its own: it needs a project around it with network
  data, budget columns, configurations, lookups, a registry entry and a publish grant. Building all
  of that is ours. So **Lonrix sets up a starter model for your client, publishes it, and runs it
  online at least once** before you begin — and that one run is the point, because it proves the
  input files, the budget columns and the configurations agree with each other. Every failure you
  meet after that is attributable to a change you just made.
- **You start by downloading two things**, and they are not interchangeable:
  [`docs/workflow/02-the-starter-model.md`](docs/workflow/02-the-starter-model.md) is the new page.
  The **model source** is a complete C# project we hand over; it builds, and it is what you edit. A
  **project snapshot** — Postprocessing → *Snapshot / Archive Setup and Outputs* — is a read-only zip
  of your client's real setup and input files. **The snapshot is not a build source**: it
  deliberately strips `refs\` and `.vscode\`, so the copy of the source inside it does not compile.
- **Your model no longer has to be a sibling folder.** Beside the Assistant is still the easy
  default, because it is what makes `..\YourModel` work in every command on every page. But a model
  we handed you, or a snapshot unpacked somewhere short to dodge Windows' path limit, can sit
  anywhere you can reach — you type full paths instead. **The rule that has not changed is that your
  model is never *inside* the Assistant folder.**

### Added — your assistant can now read your setup files

- **Give it your project snapshot and it will check your C# against what your model will actually
  meet** — the lookup sets that exist, the budget columns that exist, the input columns that exist.
  Half the ways a domain model fails are disagreements between the code and the spreadsheets, and
  every one of them is invisible to somebody reading only the code. Until now it found them at your
  first upload.
- **`jcass-dm check --lookups <snapshot>\inputs\lookups.xlsx`** stops reporting `SKIPPED` on the rule
  most worth having.
- **What it will not do with them**, deliberately: it reads the *header row* of
  `model_input_data.csv` and never the rows, and it will not tell you what condition your network is
  in. That is engineering judgement about your assets, and the web app's **Analyse Input** page is
  built for it and does it properly.
  [`docs/conventions/input-files-in-scope.md`](docs/conventions/input-files-in-scope.md).

### Changed — where a unit rate goes, and it is more specific than before

- **A treatment's unit rate belongs in the `lkp_unit_rates` sheet of `inputs\lookups.xlsx`.** Not any
  `lkp_` sheet — that one, by name. Every `lkp_*` sheet is merged before your code sees it, so from
  the model's point of view the sheet genuinely does not matter; **the Tuning page's *Treatment
  Rates* tab is the exception and reads that one sheet by name.** A rate anywhere else loads, costs
  your treatments correctly, and is invisible on the page you were told to edit rates on. Nothing
  errors. Group rates into several *sets* inside that sheet — the tab's dropdown is those set names.
- **When the effective rate varies — by material, distress or traffic — vary the *quantity*, not the
  rate.** One rate out of `lkp_unit_rates`, and a quantity your code works out, with the factors that
  produce it in lookups like everything else. A rate computed in C# is a rate you can no longer
  change, which is the whole point of the convention.
- **A `(set, key)` pair has to be unique across the whole workbook.** The web app scans every `lkp_*`
  sheet when it saves, and the same pair in two of them makes the save refuse rather than pick one.
  So when you move a rate into `lkp_unit_rates`, move it — do not copy it.

### Fixed

- **When your assistant hits something it cannot do, it now writes you the email** — the body, in its
  reply, ready to paste to `support@lonrix.com`, with the framework version stamp and the model name
  filled in. It used to *offer* to write one, which left you holding nothing if the conversation
  ended there.
- **Asking your assistant to check or explain a model no longer makes it stop and ask for a notes
  file first.** It answers the question, then tells you the notes are missing and asks for them
  before it changes anything. A diagnosis cannot be wrong for lack of notes; an edit can.
- **"Clone from Git" in the Debug workspace overlay is now named as unsupported.** The page described
  the overlay's three choices and steered you past the first without saying it is not a supported
  route, so it read like a real option. It is not one — zip in, zip out, both ways.
- **`jcass-dm` now names the `lkp_unit_rates` sheet** in the scaffolded `Constants.cs`, in the
  scaffolded README, and in what `add-treatment` prints. Those files previously said the sheet a
  lookup row sits in is "only an organisational convenience", which is true of your code and
  misleading about the Tuning page.

### What to re-check in your model

- **Look at where your treatment unit rates actually live.** Open `inputs\lookups.xlsx` and check the
  rows are in a sheet called `lkp_unit_rates`. If they are in another `lkp_` sheet your model works
  and your forecasts are correct — but those rates are not on the Tuning page's **Treatment Rates**
  tab, which is probably not what you wanted. **Move the rows; do not copy them**, or the next save
  from that tab will refuse as ambiguous.
- **Check whether anything in your C# computes a unit rate.** If it does, consider moving the
  variation into the quantity instead so the modeller keeps control of the rate.
- **Nothing else in your model needs changing.** No C# convention changed, no bundle sheet changed,
  and the `refs\` advice from the previous release still stands.

### Also in this release — `jcass-dm check` and your model's `refs\` folder

**`jcass-dm check` now tells you when your model is compiling against an older framework than this
Assistant documents.**

### Added

- **`check` has a new `framework reference` rule.** It reads the framework build stamped on the
  assemblies in your model's `refs\` folder and compares it with the Assistant's own, and says so
  when they differ. Until now the only thing between you and a model quietly built against a
  superseded framework was having read step 5 of the update page. It reports as a **NOTE**, not a
  problem: a stale reference still builds and still runs, and refusing to check your model over it
  would block work for something that is not wrong yet.
- **It also catches a `refs\` folder holding two different framework builds**, which is what copying
  files in over the old ones leaves behind. That one is not harmless — the project references
  `refs\*.dll` with a wildcard, so the leftover is compiled against alongside its replacement.
- **[`docs/conventions/silent-failures.md`](docs/conventions/silent-failures.md) § 13** describes
  the failure and the fix.

### Fixed

- **`scaffold` printed the wrong instruction** when it could not find the Assistant's `refs\` folder
  — it said to copy the files across by hand, which tops the folder up rather than replacing it. It
  now names `scripts\refresh-model-refs.ps1` and the project folder to pass it. This only ever
  appeared when `jcass-dm.exe` had been copied out of the Assistant on its own.

#### And re-check this too

- **Run `.\tools\jcass-dm.exe check --project ..\YourModel` and read the `framework reference`
  line.** If it says your model is on a different build from this Assistant, run
  `.\scripts\refresh-model-refs.ps1 -Project ..\YourModel` and rebuild.

---

## 2026-09-03

**Headline: your assistant can now remember things about your model across an update, and there is a
procedure for updating that does not lose them.**

### Added

- **[`model-knowledge/`](model-knowledge/)** — a folder at the root where your assistant writes what
  it learns about *your* models, one `.md` file per model. Your lookup set names, your budget
  categories, what an input column actually measures, and the reasoning behind decisions you made
  weeks ago. It reads the file when you ask about an existing model, and asks you for it when there
  isn't one. **It is the only place in the Assistant your assistant writes to.**
- **[`docs/orientation/updating-the-assistant.md`](docs/orientation/updating-the-assistant.md)** —
  how to move to a newer release. The single most important instruction on it is *unpack the new one
  to exactly the same folder path as the old one*, because your assistant's memory of every previous
  conversation is filed under that path and moving the folder orphans all of it.
- **[`scripts/refresh-model-refs.ps1`](scripts/refresh-model-refs.ps1)** — updates your model's own
  `refs\` folder from the Assistant's. **This is the gap that mattered most**: your model has a
  private copy of the framework reference assemblies, made when it was scaffolded, and until now
  nothing ever refreshed it. Downloading a newer Assistant left your model still compiling against
  the older framework, with no error and nothing to notice.
- **[`ASSISTANT-VERSION.txt`](ASSISTANT-VERSION.txt)** — says which release you are holding. Quote it
  when you email support.
- **This changelog.**

### Fixed

- **The claim that the Assistant is "entirely stateless with respect to your work" was wrong**, and
  it is what made all of the above invisible. It is true of your *model*, which lives in its own
  folder next door and is never touched. It was never true of your *assistant*, whose notes and
  conversation history live inside the folder an update replaces.
- **Adopting an inherited model** now refreshes `refs\` with the script rather than copying files in
  over whatever was there. The old instruction topped the folder up; because the project references
  `refs\*.dll` with a wildcard, a leftover assembly from an older framework was compiled against
  alongside its replacement.

### What to re-check in your model

- **Run `.\scripts\refresh-model-refs.ps1 -Project ..\YourModel` and rebuild.** Do this whether or
  not you think your model is current. If your model was scaffolded with an Assistant from before
  2026-08-17 it is compiling against an older framework than this release documents, and you would
  have no way of telling from the outside.
- **Nothing else in your model needs changing.** No C# convention changed in this release, no bundle
  sheet changed, and `jcass-dm` is unchanged.

---

## 2026-08-11 — first beta

The release the first engineers worked with: the framework reference assemblies and generated API
reference, the concepts mirror, the conventions and pattern library, the end-to-end workflow, the
`DomainModelSample` reference model, `jcass-dm`, and the nine Claude skills.

**If this is what you are holding**, the update procedure above is written for you, and the thing it
is protecting is your assistant's memory of the conversations you have already had.
