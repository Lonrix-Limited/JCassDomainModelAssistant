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

## The failures worth recognising on sight

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

Under **F5** this almost always means the **entry class** is not named after the `.csproj` (or
`<AssemblyName>` is set), because a debug run takes the class name from the project name and nothing
else. **Check bundle cannot see this one** — it compares the setup spreadsheet with the project name,
not the C#. `jcass-dm check` can.

### Check bundle fails *"Bundle main_dll + main_class match the C# project"*

The message reads *"main_dll is 'X' but Y.csproj builds 'Y.dll'"*, or *"main_class is 'X' but
Y.csproj implies a model class named 'Y'"*. **The setup spreadsheet names a different model than
the project builds** — nearly always a rename that reached the C# and not the `meta` sheet, or the
other way round.

F5 runs perfectly with this in place (it ignores both values and only prints a
`[DebugMode] Overriding DM DLL name` warning), so a green F5 is not evidence against it. A normal
run would fail, which is why publish refuses until the two agree.

Two details that make a value that *looks* right still fail:

- **`main_dll` is matched exactly, letter case included** — *"They differ only in letter case"* on
  the end of the message means `toymodel.dll` against `ToyModel.dll`. The server runs Linux, where
  those are two different files.
- **`main_class` is the bare class name, no namespace.** `MyRoads.ToyModel` fails; `ToyModel`
  passes. Letter case does not matter here.

The fix is local, then one upload: run `jcass-dm rename <ProjectName>` with the name the `.csproj`
already has — it rewrites only what disagrees — then send the workbook with **Upload bundle
file(s)** and check again. The full sequence is
[`../conventions/four-names.md` § After a rename](../conventions/four-names.md#after-a-rename--carry-it-to-the-web-app).

### Publish refuses: *"Bundle names the wrong DLL"* or *"Bundle names the wrong model class"*

The same disagreement as above, caught one step later. Nothing was published; the live model is
untouched. Fix it exactly as above, then **Check bundle** before publishing again —
[`../workflow/40-publish.md`](../workflow/40-publish.md#the-refusal-that-catches-people).

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

### Network function setup errors

The run stops at setup, and the message names the problem. Each one is fixed in
`domain_model_setup.xlsx`.

| The message says | What to change |
|---|---|
| `Network Function error: Input parameter 'x' is not defined on the parameters sheet.` | The `network_functions` row names a parameter that does not exist. Correct the name or add the parameter. It can say `Output` instead |
| `Network Function error: Output parameter 'x' is not a numeric parameter.` | Set that parameter's `data_type` to `number`, or name a numeric one. It can say `Input` instead |
| `Network function type 'x' is not valid.` | Use `z_score`, `percent_rank` or `percent_rank_approx`, spelled exactly |

### A build error, before anything runs

Build errors are the friendly kind: the compiler names the file, the line, and usually the fix.
Read the **first** error and ignore the rest — later ones are frequently consequences of the first.

Give the engineer the file, the line, and the corrected line. Do not explain the compiler.

**One build error is the exception, and it is next.** The compiler names a file and a line that are
not where the fix is.

### error CS1705 — "uses … which has a higher version than referenced assembly"

**What they see.** The build fails, usually with many errors, and the first one reads something like:

```
error CS1705: Assembly 'JCass_ModelCore' with identity '...' uses 'System.Runtime, Version=10.0.0.0'
which has a higher version than referenced assembly 'System.Runtime' with identity
'System.Runtime, Version=9.0.0.0'
```

`jcass-dm check` reports the same situation before the build does, as a **PROBLEM** under the rule
**target framework**: *"your model targets .NET 9 (net9.0); this Assistant's references are built
for .NET 10 (net10.0)."*

**What it means.** The model's `.csproj` targets an older .NET than the framework reference
assemblies in its `refs\` folder were built for. Juno Cassandra moved from .NET 9 to .NET 10, and
this Assistant's references are .NET 10. A model scaffolded on .NET 9 hits this the first time its
`refs\` is refreshed. **Nothing the engineer has already published is affected** — published models
keep running in the web app. This is about building on their machine.

**Do not fix it by editing references.** Not a `<Reference>` path, not a `HintPath`, not a package
version, and do not copy older assemblies back into `refs\`. Those all "work" by putting the model
back on a framework the server no longer documents. The fix is one line.

**What you do, in this order:**

1. **Say what happened in one sentence** — Juno Cassandra moved to .NET 10, and the model's project
   file still says .NET 9; what they have published keeps running.
2. **Check for the .NET 10 SDK yourself, before you ask anything** — run `dotnet --list-sdks`. If no
   line starts `10.`, **stop there** and send them to
   [`prerequisites.md`](prerequisites.md) for the download — nothing else will build until it is
   installed, and changing the project file first would only replace one error with another.
3. **Ask before you change their `.csproj`**, and say exactly what will change: *"I'll change
   `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in
   `MyRoadModel.csproj` — that line only. Shall I?"* It is their project file and an unexpected edit
   to it is the kind they cannot check. When they say yes, change that line and nothing else.
   (`refresh-model-refs.ps1 -UpdateTargetFramework` makes the same one-line edit, if they would
   rather run it themselves.)
4. **Refresh refs, rebuild, check — run these yourself**, then tell them what `check` reported. From
   this Assistant folder, in PowerShell: `.\scripts\refresh-model-refs.ps1 -Project ..\MyRoadModel`,
   then `dotnet build` in the model folder, then
   `.\tools\jcass-dm.exe check --project ..\MyRoadModel`. The target framework rule should now read
   OK.

**A solution file, if you ever make one, is `.sln`, never `.slnx`.** A domain model does not need a
solution file at all — every command in these pages builds the `.csproj` directly. But the .NET 10
SDK's `dotnet new sln` now creates the newer `.slnx` format by default, and the C# extension in the
web app's Debug Model editor cannot open one, so IntelliSense and breakpoints stop working there. If
one is wanted, create it with `dotnet new sln --format sln`. **Never convert an existing `.sln` to
`.slnx`.**

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
dir bin\Debug\net10.0\
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

The shapes worth recognising immediately:

| The outputs show | Start at |
|---|---|
| A column that is all zeros | A declared parameter never written — § 1 |
| A parameter that is flat or pinned to a round number | A clamp range that is too narrow — § 2 |
| Correct in period 0, wrong from period 1 | One factory method updated and not the other — § 5 |
| A row in a setup sheet seems to do nothing | An empty cell in column A at or above it — § 14 |

---

## When you cannot explain it

Do not narrate a plausible cause. An unfamiliar framework failure explained confidently and wrongly
costs more than an honest stop, because the engineer cannot tell the difference and will act on it.

Stop, and draft the support request —
[`../conventions/when-to-stop.md`](../conventions/when-to-stop.md).
