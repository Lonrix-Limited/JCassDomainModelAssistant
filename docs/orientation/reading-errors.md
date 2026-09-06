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

## The five failures worth recognising on sight

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

### "Couldn't find a debug adapter descriptor for debug type 'coreclr'"

Usually followed by *(extension might have failed to activate)*, with buttons offering to open
`launch.json`. **Nothing is wrong with the model, and nothing is wrong with the installation.** The
browser editor is running the folder in **Restricted Mode**, and in that state its C# support loads
but cannot supply a debugger. F5 — or the **Run and Debug** ▶ button, which fails identically —
then reports the debugger as missing.

**The fix is two steps, and the second one is what everybody misses:**

1. **Ctrl+Shift+P** → **Workspaces: Manage Workspace Trust** → **Trust**
2. **Ctrl+Shift+P** → **Developer: Reload Window**

**Trusting on its own changes nothing you can see.** The editor does not reload its extensions when
trust is granted, so the trust page will say *"You trust this folder"* while F5 goes on failing in
exactly the same way. Anyone who trusts the folder, tries again, and sees no change will reasonably
conclude that trust was not the problem — and be wrong. **Reload the window before you believe
that.** This has cost several days of the wrong investigation.

Do not touch `launch.json`. It is written for you by **Initialize workspace** and there is nothing
in it that can supply a debugger.

**You may never meet this.** Editor profiles created from September 2026 onward are set up so the
question is never asked. If you do meet it, it is on a profile that predates that, and it is a
one-off — once the folder is trusted and the window reloaded, it stays fixed.

If trust and a reload do not clear it, stop. That is the moment for
[`../support-request-template.md`](../support-request-template.md), and say in it that you did both.

### A build error, before anything runs

Build errors are the friendly kind: the compiler names the file, the line, and usually the fix.
Read the **first** error and ignore the rest — later ones are frequently consequences of the first.

Give the engineer the file, the line, and the corrected line. Do not explain the compiler.

---

## Checks that look like evidence and are not

The failure above is worth a second look, because of *how* it hid. In one real case it survived days
of investigation and more than one support call, and in that time four separate checks were run and
all four came back clean while debugging was flatly impossible:

| The check | Why it proved nothing |
|---|---|
| `dotnet build` succeeded in the editor's terminal | That is the .NET SDK compiling a project. It never touches the debugger. |
| The breakpoint appeared as a solid red dot | The editor allows breakpoints in C# because a declaration in the extension's manifest says the language supports them. That declaration is read whether or not the extension is running. |
| `launch.json` was accepted and **Debug domain model** appeared in the dropdown | Same reason. The debug type is declared in a manifest; the code that supplies the actual debugger is separate, and it was the part not running. |
| The C# extension was listed as active, with an activation time | Restricted activation is still activation. It loads, reports itself, and does almost nothing. |

**The habit worth keeping: before offering a check as evidence, ask which step of the failing path it
actually exercises.** If the honest answer is "none of it", the check is reassurance rather than
information — and reassurance forwarded in a support request costs somebody else an afternoon
chasing a fault that was already ruled out on paper and never in fact.

The same reasoning applies well beyond this one error. A model that builds is not a model that runs.
A parameter that is declared is not a parameter that is written —
[`../conventions/silent-failures.md`](../conventions/silent-failures.md) is the same idea applied to
results instead of tooling.

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
