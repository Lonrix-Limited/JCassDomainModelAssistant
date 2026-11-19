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

## 2026-11-20

**Headline: Juno Cassandra is now on .NET 10. Install the .NET 10 SDK and change one line in your
model's `.csproj`. Anything you have already published keeps running.**

### What to re-check in your model

Do these in this order. [`docs/orientation/updating-the-assistant.md`](docs/orientation/updating-the-assistant.md)
step 5 has the same list with the commands.

- **Install the .NET 10 SDK.** Check with `dotnet --list-sdks`: one line must start `10.`. Your .NET 9
  SDK can stay installed beside it.
- **Change your model's target to .NET 10.** In your `.csproj`, change
  `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`. Nothing
  else in the file changes. Your assistant will offer to make the change and ask first.
  `refresh-model-refs.ps1 -UpdateTargetFramework` makes the same one-line change.
- **Refresh your model's `refs\` folder and rebuild** —
  `.\scripts\refresh-model-refs.ps1 -Project ..\YourModel`, then `dotnet build`. No public signature
  changed, so the model should build without other edits.
- **Run `jcass-dm check`.** The new `target framework` rule should read OK.

**If you skip the target change**, the build fails with error CS1705, *"… uses … which has a higher
version than referenced assembly …"*. It means your project targets .NET 9 and the references are
.NET 10. The fix is the one line above, not editing references —
[`docs/orientation/reading-errors.md`](docs/orientation/reading-errors.md#error-cs1705--uses--which-has-a-higher-version-than-referenced-assembly).

### Changed in `jcass-dm` (now 1.2.0)

- **`check` has a new `target framework` rule.** It fails the check (exit code 4) when your model
  targets an older .NET than this Assistant's references are built for, and tells you the one line to
  change. It is separate from the `framework reference` note, which is about which framework *build*
  your `refs\` holds. A model that is behind on both gets both findings.
- **New scaffolded models target .NET 10.**
- **The tool now carries .NET 10.0.15.**

### Changed in the scripts

- **`refresh-model-refs.ps1` warns when your project's target does not match the references it just
  copied.** It changes your `.csproj` only if you answer `y` at its prompt or pass
  `-UpdateTargetFramework`. When an AI assistant runs it there is no prompt to answer, so the file is
  left untouched.

### Newly documented

- **Do not make a `.slnx` solution file.** A domain model does not need a solution file. If you want
  one, the .NET 10 SDK makes a `.slnx` by default, and the Debug Model editor in the web app cannot
  open that format. Use `dotnet new sln --format sln`.
  [`docs/orientation/reading-errors.md`](docs/orientation/reading-errors.md#error-cs1705--uses--which-has-a-higher-version-than-referenced-assembly).

### Also in this release

- **`refs\` and the generated API reference are rebuilt on .NET 10.** The framework's public API is
  unchanged. One description is new: when a treatment's cost is split across budget categories, the
  parts are listed in category-name order. The web app takes up that ordering with its next framework
  update; until then their order can differ between runs. Totals are the same either way.

---

## 2026-11-19

**Headline: a new procedure for network functions, clearer setup errors, and an updated `jcass-dm`.**

### What to re-check in your model

- **Look for empty cells in column A of your setup sheets.** Reading stops at the first row where
  column A is empty, with no error, and that row and everything below it are ignored. This is how the
  framework has always worked, but `check` used to miss a row with values in its other columns. Run
  `jcass-dm check` for the bundle, and look through the client's workbooks yourself.
- **If your project sets a `Minimum Treatment Suitability Score`, confirm your model checks it.** A
  model that does not check it ignores the setting, with no error. Models scaffolded from this
  Assistant do not check it.
  [`docs/patterns/treatment-suitability-scoring.md`](docs/patterns/treatment-suitability-scoring.md#the-configured-minimum).
- **Refresh your model's `refs\` folder and rebuild** —
  `.\scripts\refresh-model-refs.ps1 -Project ..\YourModel`. No public signature changed, so your model
  should build without edits.
- **Update your .NET 9 SDK to the latest release.** It carries security fixes. This affects your own
  machine only, and does not change forecast results.

### Changed in the framework

- **Bug fix in the `z_score` network function.** It gave every element the same value. It now gives
  each element's distance from the network mean, in standard deviations.
- **Network function setup errors now name the problem.** A missing or non-numeric parameter gives, for
  example, *"Network Function error: Input parameter 'x' is not defined on the parameters sheet."* A
  missing parameter used to surface as a general dictionary error.
  [`docs/orientation/reading-errors.md`](docs/orientation/reading-errors.md#network-function-setup-errors).

### Changed in `jcass-dm` (now 1.1.2)

- **`check` and `dump` report a row with values and an empty first column**, and commands that add a row
  refuse to write over it.
- **`check` no longer reports network function outputs as unwritten.** The framework writes them, so
  your code does not need to. Ignore any earlier advice from `check` to write one or delete its row.
- **`add-parameter` now says so too.** Its next steps tell you not to write a network function output.
- **Notes to the right of a table are left alone.** They are no longer reported as stray content, and
  they no longer stop commands from adding a row.
- **The tool now carries .NET 9.0.23**, the latest security patch.
- **New scaffolded models carry corrected comments** about the suitability score.

### Newly documented — existing behaviour, not changes

- **[Adding a network function](docs/workflow/30-make-a-change.md#add-a-network-function)** now has its
  own procedure. A network function's position on the `parameters` sheet does not matter. Its row order
  on the `network_functions` sheet does, and the concepts page has been corrected to say so.
- **Two new entries on the
  [silent failures list](docs/conventions/silent-failures.md#14-a-row-with-an-empty-first-cell)** cover
  the empty column A and the unchecked minimum score.
- **The suitability scoring guidance is corrected.** A score belongs to a treatment on a particular
  element, and it is one ranking criterion among several. An unscored candidate is ranked on zero. An
  invalid score stops an MCDA run with an error when the score has a weight.
- **The web app's Help has new articles:** *Network functions*, *The MCDA model* and *The Treatment
  Suitability Score*.

### Also in this release

- **`refs\` and the generated API reference now match the framework running on the server.** Nothing
  about `TreatmentInstance`, triggers, resets or lookups changed.

---

## 2026-09-16

**Headline: ask your assistant to refactor a model you have inherited and it now tells you what is
wrong with it first — then asks for your notes before changing anything.**

### Changed — taking over a model

- **"Help me refactor the model in folder X" now gets a diagnosis on the first reply.** Your assistant
  runs `jcass-dm check`, tells you what it found, and then — if it has no notes on that model — asks
  for them and waits before renaming or refactoring anything. Until now it asked for the notes first
  and told you nothing, which is a poor answer to somebody who has just been handed a model.
- **It also says, up front, that publishing replaces the model your client is already running.** That
  warning was always in the adoption procedure; your assistant was stopping before it got there.

### What to re-check in your model

- **Nothing in your model needs changing.** This release changes how your assistant behaves, not what
  it writes.

---

## 2026-09-15

**Headline: your assistant no longer edits your client's spreadsheets, even when you ask it to. It
changes your model and tells you exactly which row to add.**

### Changed — who writes `lookups.xlsx`

- **Ask your assistant to turn a number into a lookup and it now does the code, then hands you the
  row** — sheet, set name, key, value and comment — and says it has left the file for you. You add the
  row in Excel and upload the file on **Files → Inputs**. This holds for every file in `inputs\` and
  `supporting\`, and it holds when you say *"do it"*.
- **Why, because it will feel like a step backwards.** An assistant that wrote the rows itself did so
  with a Python spreadsheet library that cannot calculate. The rows it added were correct, but saving
  the workbook wiped the stored result of **every formula in it**. Excel recalculates when you open a
  file, so the workbook looked fine; the model run reads the stored results, so it ran with blank
  lookup values and reported nothing.
  [`docs/conventions/input-files-in-scope.md`](docs/conventions/input-files-in-scope.md) § 3.
- **Your model's `domain_model_setup.xlsx` is not affected.** It is part of the model, and the
  `jcass-dm` verbs still write it.

### Fixed

- **The Tuning page cannot add a lookup row.** Four pages said it could. It changes values in rows that
  already exist; a new row goes in through Excel and **Files → Inputs**.

### What to re-check in your model

- **If an assistant has ever written to your `lookups.xlsx`**, open it in Excel, save it, and upload it
  again on **Files → Inputs**. That puts back the stored result of every formula. A file an assistant
  touched that has no formulas in it is unaffected.
- **Refresh your model's `refs\` folder and rebuild** —
  `.\scripts\refresh-model-refs.ps1 -Project ..\YourModel`, in your PowerShell terminal in the
  Assistant folder. See below for why this release needs it.

### Also in this release — the framework reference now matches what the server runs

- **The reference assemblies in `refs\` and the generated API reference in `docs\framework\api\` moved
  from a framework build of 17 August to the one running on the server.** Until your model's own
  `refs\` is refreshed, it compiles against the older one.
- **Three public types and one method no longer exist in the framework:** `PreTreatVsThreshold`,
  `JunoViewerFwpPrepV2`, `PlotSetup` and `OutputSet.GetParameterPlotData`, all from post-processing.
  A domain model rarely touches these. **If yours does, it will stop compiling after the refresh** —
  which is the point: it would already fail on the server.
- **Nothing about `TreatmentInstance`, triggers, resets or lookups changed.** The only other prose
  change is a clearer description of the benefit-cost treatment-selection setting on
  `ModelConfiguration`.

---

## 2026-09-07

**Headline: if F5 in the browser editor ever tells you it cannot find a debug adapter, the fix is to
trust the folder *and* reload the window — and the reload is the half everybody misses.**

Documentation only. **Nothing to re-check in your model** — no rule changed, no tool changed, no
reference assembly moved. If your model builds and debugs today it is unaffected.

### Added — one more error worth recognising on sight

- **"Couldn't find a debug adapter descriptor for debug type 'coreclr'"** now has an entry in
  [`docs/orientation/reading-errors.md`](docs/orientation/reading-errors.md). It means the editor has
  your folder in **Restricted Mode**, not that anything is broken, and it is cleared by *Manage
  Workspace Trust → Trust* followed by *Developer: Reload Window*.
  [`docs/workflow/20-upload-and-debug.md`](docs/workflow/20-upload-and-debug.md) carries the short
  version at the step where you would meet it.
- **Why trusting the folder appears to do nothing.** The editor does not reload its extensions when
  trust is granted, so the trust page says *"You trust this folder"* while F5 fails in exactly the
  same way as before. Trust, retry, no change, conclude trust was not the problem — that reasoning is
  natural and it is wrong, and it cost several days of investigation before it was written down.
  Reload the window before you believe it.
- **You may never see any of this.** Editor profiles created from September 2026 onward no longer ask
  the question. The entry is here for profiles that predate that, and for anyone who meets the
  message and searches for it.

### Added — a way of judging your own checks

- **[`docs/orientation/reading-errors.md`](docs/orientation/reading-errors.md) § *Checks that look
  like evidence and are not*.** In the case above, four checks were run and all four came back
  clean while debugging was impossible: the project built, the breakpoint showed as a solid red dot,
  the launch configuration validated, and the C# extension reported itself active. Not one of them
  touches the debugger. The section says why each is hollow and gives the habit that catches the
  next one — *before offering a check as evidence, ask which step of the failing path it actually
  exercises.*

### Changed — the support request asks for two things first

- **[`docs/support-request-template.md`](docs/support-request-template.md)** now names the two
  one-minute checks to rule out before escalating anything on the Debug Model page: trust-and-reload,
  and one click of *Initialize workspace*. Both are the recipient's first two questions, so a
  request that has already answered them saves a day of turnaround. They belong under *what we
  tried*, not under *hypothesis*.

---

## 2026-09-03b

> **The second release of 2026-09-03**, and the trailing `b` exists only because two went out
> on the same day. If [`ASSISTANT-VERSION.txt`](ASSISTANT-VERSION.txt) in your copy says
> `2026-09-03b` you have this one; if it says `2026-09-03` you have the release below and this
> page is what you are missing.

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
