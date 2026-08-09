# `jcass-dm`

Reads and writes `domain_model_setup.xlsx` — the bundle file that sits beside your `.csproj`
and tells the framework how to load your model.

```powershell
.\tools\jcass-dm.exe dump ..\MyRoadModel\domain_model_setup.xlsx
```

Nothing to install. `jcass-dm.exe` is committed here, already compiled, and carries its own
copy of everything it needs.

---

## Why this exists

The bundle is a binary Excel file. That makes it invisible to an AI coding assistant: it
cannot read it, cannot edit it, and cannot diff it. Without this tool, every bundle change
is *"open Excel, go to the treatments sheet, add a row, type the name exactly as it appears
in your C#"* — a five-step procedure done by hand, three steps of which are strings that
must match code character for character.

`dump` is the half of that problem worth solving first. It turns the bundle into text, so
you can compare what the bundle declares against what your code does, and see what a change
actually did.

---

## The verbs

Run `jcass-dm` with no arguments for the full help. In short:

| Verb | What it does |
|---|---|
| `dump <bundle> [--sheet <name>]` | Prints the whole bundle as text. Stable and ordered, so two dumps can be compared. |
| `set-meta <bundle> [--main-dll x] [--main-class y] [--display-name z]` | Sets which DLL to load, which class in it, and the name shown in the web app. |
| `add-treatment <bundle> --name x --budget-category y` | Declares a treatment. |
| `add-parameter <bundle> --name x --min n --max n` | Declares per-element state carried between periods. |
| `add-input-header <bundle> --column x --type number\|text` | Declares a column expected in the client's input CSV. |

`scaffold`, `rename`, `check` and `package` are not here yet.

### What every write guarantees

- **It is idempotent.** Running `add-treatment` twice with the same values adds one row and
  reports the second as `unchanged`. The file is not even rewritten, so a re-run does not
  show up as a modified binary in git.
- **It never silently overwrites.** If the row is there with different values, nothing is
  written, the differences are printed as a table, and the exit code is `3`. `--force` is
  the only way past.
- **It touches only the cells it was asked to.** No other row, sheet, or piece of formatting
  is rewritten.
- **It writes all of an operation or none of it.** `set-meta` sets three rows; a conflict on
  one of them writes none of them. Half a rename is worse than no rename.
- **It refuses a bundle missing any of the five required sheets**, naming the one that is
  missing.

### Exit codes

Agents branch on these, so they are a contract rather than an implementation detail.

| Code | Meaning | What to do |
|---|---|---|
| `0` | Done. Includes "already correct, nothing to write". | Carry on. |
| `1` | The command line was wrong — unknown option, missing or unparseable value. | Read the message and fix the command. |
| `2` | The bundle is unusable — missing file, missing sheet, missing column. | Fix the bundle. The message names what is missing. |
| `3` | The row exists with different values and `--force` was not given. | Decide: overwrite with `--force`, or use a different name. |
| `9` | `jcass-dm` itself failed. | A bug in the tool. Report it — see the escalation route in the docs. |

An unrecognised option is always `1`, never ignored. `--budget_category` instead of
`--budget-category` would otherwise leave a treatment with a blank budget category, which is
a treatment that is never funded and never complains.

---

## What it does not do

It does not check whether your model makes sense. The web app's **Check Setup** owns that,
and it is authoritative. `jcass-dm` answers *"is this a well-formed bundle"* — sheet present,
column present, row well-formed, name free of the trailing space that would stop it matching
your C#.

It also does not touch your C#. When a bundle change needs matching code, the tool prints
what is left to do rather than attempting it.

---

## For maintainers: rebuilding the exe

```powershell
.\scripts\build-jcass-dm.ps1
```

That runs the tests, publishes `win-x64` self-contained single-file, copies the result to
`tools\jcass-dm.exe`, and writes `tools\jcass-dm.build.txt` recording the version, the commit
it was built from, and the size. Commit the exe and the stamp together.

The source is in [`src/JcassDm/`](src/JcassDm/), the tests in
[`src/JcassDm.Tests/`](src/JcassDm.Tests/). Run them directly with:

```powershell
dotnet test tools\src\JcassDm.Tests\JcassDm.Tests.csproj
```

Tests always work on a **copy** of `reference-model/DomainModelSample/domain_model_setup.xlsx`.
`ReferenceBundleGuard` fails if the original is ever modified — that file is committed content
the documentation depends on.

### Three decisions worth not re-opening

**The exe is committed, at about 37 MB.** Improvements reach engineers by re-downloading this
repository, so anything not in the download is a step somebody has to be told about and will
skip. A GitHub release asset would keep the repository small and break that property. Git LFS
is not an escape route either: GitHub's source ZIP contains LFS *pointer* files, so a
downloaded copy would carry a text stub where the tool should be. The cost is real — each
rebuild adds another ~37 MB to history permanently — so **rebuild only when `tools/src`
actually changes**, not as a routine step.

**It uses ClosedXML directly, not `JCass_Excel`.** The framework's Excel facade reaches this
repository only as a *reference assembly* (`refs/JCass_Excel.dll`): it compiles and it gives
IntelliSense, and the runtime refuses to load it, so a tool built against it would fail on
its first call. ClosedXML is the same engine that facade wraps, and `SheetTable` deliberately
mirrors its read semantics — header on row 1, data rows until the first row with an empty
first cell — so `jcass-dm` sees exactly what the framework sees.

**It is not trimmed.** Trimming would roughly halve the file, and ClosedXML resolves some
types by reflection, so a trimmed build can publish cleanly and then fail at run time on a
workbook feature the tests did not happen to exercise. Not a trade worth making for a tool
whose job is to be trusted with somebody's model.

### One thing the tool cannot promise

**Byte-for-byte preservation of the workbook is not achievable**, by this tool or any other.
Any OpenXML library rewrites the whole package on save, so XML attribute order shifts and
shared-string indexes renumber even in sheets nothing touched. What *is* preserved, and what
the tests assert cell by cell, is everything anybody can observe: every value, every fill,
font, border, number format and alignment, on every sheet the verb did not write to.
