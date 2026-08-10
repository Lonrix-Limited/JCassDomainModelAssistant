# 10 — Scaffold your model and build it

**Goal of this page:** a domain-model project on your own machine that compiles cleanly and passes
`jcass-dm check`. Nothing has been uploaded yet and nothing can break anything yet.

Picking up a model somebody else wrote? Go to
[`05-adopt-an-existing-model.md`](05-adopt-an-existing-model.md) instead and come back at step 3.

---

## Before you start

**Settle the engineering first.** [`01-plan-your-model.md`](01-plan-your-model.md) — your
treatments, your input columns, your parameters, how each one increments and resets, and your
thresholds. It is half an hour with a spreadsheet and it decides how the next fortnight goes. If you
have not done it, do it now; this page will still be here.

Then decide **one name** for your model. It becomes four things at once — the project file name, the
assembly name, the entry class, and two settings inside the model's bundle spreadsheet — and they
have to stay identical for the rest of the model's life.

Pick something in PascalCase with no spaces and no punctuation: `WaipaRoadModel`,
`BridgeConditionModel`, `MyRoadModel`. If you change your mind later, there is a command for it —
never edit the four by hand. See [`../conventions/four-names.md`](../conventions/four-names.md).

---

## Step 1 — Scaffold

### First, know where the model is about to be created

The command below writes a new folder, and **where** it lands is decided by the folder your terminal
is sitting in — not by anything you can see on screen. Get this straight before you run it and you
will not spend an afternoon looking for your model.

**Open a PowerShell terminal in this repository's folder.** In VS Code: **Terminal → New Terminal**.
It normally opens in the folder you have open, which is the right one. Confirm it:

```powershell
pwd
```

You should see the path of the `JCassDomainModelAssistant` folder — something like
`C:\Work\JCassDomainModelAssistant`. If not, `cd` to it. The whole of this is explained in
[`../orientation/running-commands.md`](../orientation/running-commands.md).

**Your model will be created next door**, as a sibling of that folder:

```
C:\Work\
    JCassDomainModelAssistant\     <- your terminal is here
    MyRoadModel\                   <- your model will be created here
```

**Check you can write there.** Open `C:\Work\` — the parent folder — in File Explorer, right-click
in the empty space, **New → Text Document**, then delete it. If both work, carry on. If they do not,
choose a folder you own (anywhere under **Documents** is safe), move the Assistant folder there, and
start again. `scaffold` will refuse with a plain message rather than writing half a model, but it is
cheaper to know now.

### Then run it

In that same PowerShell terminal:

```powershell
.\tools\jcass-dm.exe scaffold MyRoadModel --from-sample --output ..\MyRoadModel
```

Substitute your own name in both places. `..\` means *the folder next door*, so
`--output ..\MyRoadModel` puts the project **beside this repository, not inside it** — that is what
makes it safe to replace the Assistant with a newer version later without touching your work.

**You should see** a summary ending with the four names, then a `Next:` block and a list of the
lookup sets the model will need:

```
created    MyRoadModel.csproj
created    Objects\ - 10 files
created    README.md, .gitignore
created    domain_model_setup.xlsx - copied from the reference model, meta set to MyRoadModel

Scaffolded MyRoadModel at C:\...\MyRoadModel

The four names all read 'MyRoadModel', and they were all written from that one name:
  1. MyRoadModel.csproj
  2. assembly MyRoadModel   (inherited - <AssemblyName> is deliberately unset)
  3. class MyRoadModel : DomainModelBase
  4. meta.main_dll = MyRoadModel.dll, meta.main_class = MyRoadModel
```

**Read the `Scaffolded ... at ...` line and check the path is where you expected.** It is the full
path, spelled out, and this is the moment to notice if the terminal was somewhere other than you
thought. Nothing else has happened yet, so the fix is to delete that folder and run the command
again from the right place.

**Why `--from-sample`.** It carries the reference model's working logic, so the project runs end to
end before you have written anything. That is the walking skeleton — see
[`README.md`](README.md#the-walking-skeleton--do-this-before-you-model-anything) for why proving
the pipeline first is worth the extra half hour. **This is the project you keep.** There is no
throwaway and no rename later.

> Without `--from-sample` you get the same file skeleton with the method bodies left empty. That is
> for somebody who already knows this framework. If you are not sure, use `--from-sample`.

### What you got

```
MyRoadModel\
    MyRoadModel.csproj          Build settings. The filename is load-bearing.
    domain_model_setup.xlsx     The bundle — what the framework needs before it loads your code.
    refs\                       Framework reference assemblies. Never edit these, never upload them.
    README.md  .gitignore
    Objects\
        MyRoadModel.cs          Entry class. A switchboard — keep it thin.
        Constants.cs            Every threshold and rate, read from the client's lookups.xlsx.
        ModelElement.cs         What one asset is, and the state it carries between periods.
        ModelElementFactory.cs  Framework data -> ModelElement. All input column names live here.
        Initialiser.cs          Period 0 — the starting state of each element.
        Incrementer.cs          One period of deterioration.
        Resetter.cs             What a treatment does to an element.
        TreatmentsTrigger.cs    When work is due and what it costs. The engineering judgement.
        RoutineMaintenance.cs   Routine maintenance, which is triggered separately.
        TreatmentNames.cs       Treatment name constants, shared with the bundle.
```

Which file the framework calls when: [`../orientation/how-a-run-works.md`](../orientation/how-a-run-works.md).

## Step 2 — Add it to your VS Code window

**This is the moment the model folder finally exists**, so it is the moment to put it beside the
Assistant in one window.

You already have the Assistant open — that is the window you have been typing commands into. Add
your model to it:

**File → Add Folder to Workspace…** → choose `MyRoadModel` → say yes if VS Code offers to save the
workspace.

**You should see** two folders in the Explorer on the left, `Assistant` and `MyRoadModel`. That is
what lets your assistant read the guidance and edit your model without either of you switching
windows, and it keeps the terminal where it is.

> Opening the model on its own with **File → Open Folder → `MyRoadModel`** also works, and costs
> you the docs and the terminal's folder. Prefer the two-folder window.
>
> The workspace file has a commented recipe at the top if you would rather edit it by hand —
> [`../../assistant.code-workspace`](../../assistant.code-workspace). You do not need to.

**You should see** no red squiggles, and hovering `DomainModelBase` in `Objects\MyRoadModel.cs`
should show its documentation. If it does not, give VS Code a few seconds to finish loading the
C# extension, then try again.

## Step 3 — Build

Back in your PowerShell terminal — the one still sitting in the Assistant folder — run:

```powershell
dotnet build ..\MyRoadModel\MyRoadModel.csproj -c Debug --no-incremental
```

**You should see** `Build succeeded.` with `0 Warning(s)` and `0 Error(s)`. The first build takes a
minute or two while it restores; later ones are seconds.

**Treat a warning as a failure.** Warnings in a domain model are almost always a real mistake —
an unused variable that was meant to be assigned, a comparison that is always true — and letting
one sit means the next one is invisible.

If it does not build, [`../orientation/reading-errors.md`](../orientation/reading-errors.md) covers
which line of the output matters and what the common messages mean.

> **You cannot *run* the model here, only build it.** The assemblies in `refs\` are reference
> assemblies: the complete public API with the method bodies removed. They compile and give full
> IntelliSense, and the .NET runtime refuses to execute them. Running and debugging happen on the
> server, in step 20. This is the design, not a fault — `refs\README.md` explains it.

## Step 4 — Check

Same terminal again:

```powershell
.\tools\jcass-dm.exe check --project ..\MyRoadModel
```

**You should see** a row per rule and, at the end, `No problems.`

```
  one .csproj at the root  OK        MyRoadModel.csproj
  the four names           OK        all read 'MyRoadModel'
  <AssemblyName>           OK        not set, which is correct
  bundle structure         OK        five sheets, all required columns present
  parameters vs C#         OK        3 declared, all written
  treatments vs C#         OK        3 declared, matched against TreatmentNames in both directions
  treatment reset arms     OK        all 3 have a case arm
  budget categories        NOTE      every treatment names one: repair, replace.
  lookup sets              SKIPPED   no lookups.xlsx given. The C# asks for: ...
```

**`NOTE` and `SKIPPED` are not failures.** A `NOTE` is something worth knowing that the tool cannot
decide for you. A `SKIPPED` rule is one it could not apply — here, because it has not been shown
the client's lookups file. Both are printed in full underneath, with what to do about them.

**What this rules out** is the whole class of mistakes where your C#, your bundle spreadsheet and
the client's lookups have quietly stopped agreeing with each other: a parameter declared but never
written, a treatment name typed differently in two places, a treatment with no `case` arm in
`Resetter.cs`. Those produce a model that builds, runs to completion, and is wrong — see
[`../conventions/silent-failures.md`](../conventions/silent-failures.md).

**What it does not rule out** is anything that depends on the client's actual data. `jcass-dm
check` reads your project folder and nothing else, and it reads your C# as text rather than
compiling it. The web app's **Check Setup** page is authoritative. The tool says so itself at the
bottom of every run; do not paraphrase it into "the model is fine".

### Check it against the client's lookups too

If you have a copy of the client's `inputs\lookups.xlsx` — download it from **Files → Inputs** —
point the check at it and the last rule stops being skipped:

```powershell
.\tools\jcass-dm.exe check --project ..\MyRoadModel --lookups ..\lookups.xlsx
```

```
  lookup sets              OK        all 7 found in lookups.xlsx
```

A scaffolded `--from-sample` model needs seven lookup sets: `repair_thresholds`,
`replace_thresholds`, `maintenance_thresholds`, `deterioration_rates`, `replacement_rates`,
`rate_factors` and `unit_rates`. A missing one stops the run at setup and names itself, so this is
not a silent failure — it is just much cheaper to find now than after an upload.

There is a sample `lookups.xlsx` in
[`../../reference-model/sample-inputs/`](../../reference-model/sample-inputs/) if you want to see
the shape.

---

## Where the numbers go — read this before you write your first threshold

Every number a modeller would ever change to recalibrate the forecast — a trigger age, a condition
limit, a unit rate, a deterioration rate — goes in the client's `inputs\lookups.xlsx` and is read
through `Objects\Constants.cs`. **Not as a `const` in C#.**

That is not a style preference. A number in `lookups.xlsx` is one the modeller changes themselves
on the Tuning page and re-runs in a minute. The same number in C# needs a developer, a rebuild, an
upload and a publish. The full rule, the boundary (plenty of numbers legitimately stay in C#), and
the third tier for coefficient *sets*: [`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md).

---

## Done when

- [ ] `dotnet build` says `Build succeeded`, 0 warnings, 0 errors.
- [ ] `jcass-dm check` says `No problems`.
- [ ] Your model folder is **beside** this repository, not inside it.
- [ ] **You can say the full path of your model folder out loud** — `C:\...\MyRoadModel`. Every
      later step reaches it with `..\MyRoadModel` from the Assistant folder, and knowing the real
      path is what lets you tell a wrong turn from a broken command.

Next: [`20-upload-and-debug.md`](20-upload-and-debug.md).
