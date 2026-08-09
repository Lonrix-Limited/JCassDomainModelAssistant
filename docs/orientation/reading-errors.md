# Reading errors

**The engineer cannot read a stack trace, and does not need to learn how.** Your job is to read it,
say what it means in one sentence of modelling language, and tell them exactly what to change.

Never paste a raw trace at them and never say "as you can see from the exception". Say: *"the model
asked for a lookup called `unit_rates` and your `lookups.xlsx` does not have one — add a sheet
called `lkp_unit_rates` with these three columns."*

---

## Which line of a stack trace matters

A stack trace is a list of what called what, **most recent first**. The framework's own frames are
usually at the bottom and are not the problem.

**Read down the list and stop at the first line naming a file in the engineer's project.** That is
where their code was when it failed. The line number on it is the line to open.

If *no* line names their project, the failure is in setup or configuration rather than in their
logic — a bundle sheet, a lookup, a column name — and the message text matters more than the trace.

---

## The four failures worth recognising on sight

### "Object reference not set to an instance of an object"

A `NullReferenceException`. Something that should have had a value had nothing. In a domain model it
is almost always one of four things:

| Cause | Tell |
|---|---|
| Framework access from the **constructor** | The trace names their entry class's constructor. `model` is not assigned until afterwards. Move the work into `SetupInstance` — [`how-a-run-works.md`](how-a-run-works.md). |
| A **lookup set or key that does not exist** | Usually a typo in `lookups.xlsx`, or a sheet not prefixed `lkp_`. Guard your reads and name the set and key, and this becomes a readable message instead. |
| A **text input column** that was blank in the client's CSV | Text comes through as null. The framework rejects nulls in *numeric* columns, so those show up differently. |
| A **text model parameter never written** in `SetParameterValues` | Null rather than zero. See [`../conventions/silent-failures.md` § 1](../conventions/silent-failures.md#1-a-parameter-declared-in-the-bundle-but-never-written). |

### "Reference assemblies cannot be loaded for execution"

A `BadImageFormatException`. **Something tried to run the framework where it cannot be run.**

Locally, this is expected and is not a fault — the assemblies in `refs\` compile but cannot execute,
by design. Reassure and redirect to the Debug Model page.

On the server at F5, it means a `refs\` folder was included in the upload zip and overwrote runnable
assemblies —
[`../conventions/silent-failures.md` § 8](../conventions/silent-failures.md#8-a-refs-folder-inside-the-upload-zip).

### "Domain Model class 'X' was not found in the specified .dll"

The four names disagree. Not a code problem at all —
[`../conventions/four-names.md`](../conventions/four-names.md). Run `jcass-dm check`; it names which
of the four are out of step.

### A build error, before anything runs

Build errors are the friendly kind: the compiler names the file, the line, and usually the fix.
Read the **first** error and ignore the rest — later ones are frequently consequences of the first.

Give the engineer the file, the line, and the corrected line. Do not explain the compiler.

---

## Breakpoints that will not bind

A **hollow breakpoint** — an outline rather than a solid dot — means the debugger could not match
that line to the code actually loaded. It is not a problem with their logic; nothing at that line
has run yet.

Almost always a stale or mismatched build. Rebuild, then check the timestamps and the name:

```powershell
dotnet build MyRoadModel.csproj -c Debug --no-incremental
dir bin\Debug\net9.0\
```

The `.dll` and the `.pdb` should both be there, both stamped seconds ago, and the `.dll` should be
named after the project. If it is not, that is the four-name rule —
[`../conventions/four-names.md`](../conventions/four-names.md).

A **solid breakpoint that never fires** is different and usually correct behaviour: that code path
did not run. Ask which element and which period they expected it to fire on, then check whether the
condition above it was ever true.

---

## When the model runs and the answer is wrong

**This is the hard case, and it is where the real damage lives.** No error, no trace, plausible
outputs. Work the checklist rather than reading code:
[`../conventions/silent-failures.md`](../conventions/silent-failures.md).

The three shapes worth recognising immediately:

| The outputs show | Start at |
|---|---|
| A column that is all zeros | A declared parameter never written — § 1 |
| A parameter that is flat or pinned to a round number | A clamp range that is too narrow — § 2 |
| Correct in period 0, wrong from period 1 | One factory method updated and not the other — § 5 |

---

## When you cannot explain it

Do not narrate a plausible cause. An unfamiliar framework failure explained confidently and wrongly
costs more than an honest stop, because the engineer cannot tell the difference and will act on it.

Stop, and draft the support request —
[`../conventions/when-to-stop.md`](../conventions/when-to-stop.md).
