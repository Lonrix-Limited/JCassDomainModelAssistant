# 20 — Upload it and debug it in the browser

**Goal of this page:** your model running on the server, with a breakpoint stopping inside your own
C# while a real forecast is part-way through. Nothing is published; nothing the client runs has
changed.

You need step 10 finished: a project that builds with 0 warnings and passes `jcass-dm check`.

---

## Step 1 — Take the project lock

**Project Home → Lock project.**

The **Debug Model** item in the navigation bar stays greyed out until you hold it — hovering it
says *"Take the project lock on Project Home to open the Debug Model page."*

Add a note if the box offers one; other users see it and know why the project is held. Release it
with **Release my lock** when you finish for the day.

## Step 2 — Package the upload zip

Back in your PowerShell terminal on your own machine — the one sitting in this repository's folder
([`../orientation/running-commands.md`](../orientation/running-commands.md)):

```powershell
.\tools\jcass-dm.exe package --project ..\MyRoadModel
```

**You should see:**

```
packaged   14 files (34.7 KB)
           C:\...\MyRoadModel\MyRoadModel_for_debug.zip

It opens straight to:
  .gitignore
  MyRoadModel.csproj
  README.md
  domain_model_setup.xlsx
  Objects/

Left out:
  refs\  - the debug workspace stages its own, and these cannot be executed
  bin\  - build output
  obj\  - build output
```

The zip lands next to your `.csproj`. Re-running refuses to overwrite it; add `--force` when you
want the new one.

**Do not build this zip by hand.** Two things about it are load-bearing and both are easy to get
wrong in File Explorer:

- **It must open straight to the `.csproj`**, not to a folder containing it. Zip the *folder* and
  everything sits one level too deep; the upload rejects it with *"No .csproj file found at
  workspace root"*.
- **It must not contain `refs\`.** Those are reference assemblies that cannot execute. The server
  skips an uploaded `refs\` for you now, but a hand-made zip that carries one is a zip you will
  try to use somewhere else. See
  [`../conventions/silent-failures.md` § 8](../conventions/silent-failures.md#8-a-refs-folder-inside-the-upload-zip).

## Step 3 — Open the Debug Model page

**Debug Model** in the navigation bar.

The first time, an overlay appears — *"Set up the Debug workspace"* — offering three ways to bring
source in: clone from Git, upload a zip, or skip. **Choose "I'll upload a zip".** It only dismisses
the overlay so the editor can mount; it does not upload anything by itself.

That overlay also mentions **Restricted Mode**. The editor opens folders untrusted until you tell
it otherwise, which is normal and gets in the way of building. Trust the workspace when it asks.

## Step 4 — Pick the model version, then upload

At the top of the page:

1. **Model version** — pick your client's custom model. A custom domain model has **exactly one
   version**; if the dropdown is empty, see
   [`00-prerequisites.md`](00-prerequisites.md#1-your-model-must-already-exist-in-juno-cassandra-as-a-custom-domain-model).
2. **Upload zip** — choose `MyRoadModel_for_debug.zip`.

Uploading extracts into the server's `domain_model_source/`. `bin/`, `obj/`, `.git/`, `.vs/`,
`node_modules/` and `refs/` are skipped, and your `.csproj`'s framework references are rewired to
the server's own `refs/` automatically — so you do not edit the project file to move between your
machine and the server.

## Step 5 — Initialize workspace

**Initialize workspace.**

It seeds the debug bundle from the model version you picked, stages the framework DLLs the server
build needs, and writes the two editor configuration files that make F5 work.

**You should see** the state chip beside the buttons change to report a ready workspace, and the
editor mount in the frame below.

> **Initialize does not delete your source.** It is safe to run again whenever something looks
> wrong — a missing `.vscode` folder, an editor that will not launch. The two buttons that *do*
> delete are **Reset source** (wipes your uploaded C#) and **Reset debug bundle** (wipes the
> setup spreadsheets). Neither is part of the normal loop.

## Step 6 — Build on the server

**This one is not on your machine.** In the editor frame *in the browser*, open a terminal
(**Terminal → New Terminal**) — a terminal on the server, not the PowerShell one on your machine,
and it is already sitting in your uploaded project. Then:

```bash
dotnet build
```

**You should see** `Build succeeded.` It is the same project you built locally, so it should — and
if it does not, the difference is the environment, not your code, and that is worth saying out
loud rather than editing C# until it goes away.

> F5 builds for you before it launches, so this step is not strictly required. Do it anyway the
> first time: a build failure and a launch failure look nothing alike once you have separated
> them, and identical once you have not.

## Step 7 — F5

Open a file you want to stop in — `Objects/TreatmentsTrigger.cs` is the interesting one — and click
in the gutter to the left of a line number to set a **breakpoint** (a red dot).

Press **F5**. Pick the **"Debug domain model"** configuration if you are asked.

**You should see**, in the debug console, lines beginning `[DebugMode]` — the client folder, the
bundle file, the inputs folder — then your breakpoint hits and the editor stops with the current
values on screen. Step through with **F10**, continue with **F5**.

At the end:

```
[DebugMode] Success. Unique stamp: ...
[DebugMode] Output folder: ...
```

### Three things about a debug run that are not obvious

- **It runs one configuration only** — the first config tag in alphabetical order — and says which
  in the console: `[DebugMode] Loaded 3 configuration(s); running: 'base'.` A real run
  ([`50-run-the-model.md`](50-run-the-model.md)) is where you choose.
- **It writes real outputs** into the client's `outputs/` folder, exactly like a normal run. It is
  a real forecast; it is not a dry run.
- **It ignores the bundle's `meta.main_dll` and `meta.main_class`** and binds to whatever your
  `.csproj` just built. That is deliberate — under debugging your source has moved on from the
  published assembly. It is also why a name mismatch can hide here and first surface at publish:
  see [`40-publish.md`](40-publish.md#the-refusal-that-catches-people).

### If the breakpoint does not bind

A hollow circle instead of a solid red dot means the debugger has your file but not matching
compiled code. Build again (step 6) and press F5 again. If it persists,
[`../orientation/reading-errors.md`](../orientation/reading-errors.md) § *Breakpoints that will not
bind* covers the rest.

### If setup fails on a missing CSV

A message naming a file under `domain_model/` — singular, not `debug_domain_model/` — means your
model is resolving a side-car CSV against the bundle folder, and that folder is only populated by a
**normal** run. On a client that has never had one, it is empty.

**The durable fix is to read side-car CSVs from the client's `supporting\` folder instead**, which
resolves to the same path under a debug run and a normal one, and which the modeller can update
without a rebuild. Measured, not assumed:
[`../conventions/naming-and-folders.md`](../conventions/naming-and-folders.md#supporting-versus-the-bundle-for-a-side-car-csv).
Upload those CSVs on **Files → Inputs**, in the **Supporting Files** panel.

---

## Step 8 — Check the bundle before you go further

On the ribbon's second row: pick a **Config tag** and click **Check bundle**.

This runs the web app's own setup validators against the bundle you are editing, using the
client's real inputs, budgets and configuration — everything `jcass-dm check` cannot see. A report
panel appears below the ribbon.

**Read it now, not at publish time.** It is the same validator set as the Tuning page's
**Check Setup**, and it is authoritative in a way the local check deliberately is not.

---

## The loop from here

You are now in the loop you stay in:

```
edit locally ─> dotnet build ─> jcass-dm check ─> jcass-dm package ─> Upload zip ─> dotnet build ─> F5
```

Small changes are often faster to make directly in the browser editor and bring home afterwards —
that is what [`60-get-your-code-back.md`](60-get-your-code-back.md) is for. Either way, **the local
project stays the source of truth**; a fix left only on the server is a fix you will lose.

## Done when

- [ ] A breakpoint in your own C# stops a running forecast.
- [ ] The run reaches `[DebugMode] Success`.
- [ ] **Check bundle** reports no failures.

Next — if you are proving the walking skeleton, go straight to [`40-publish.md`](40-publish.md) and
read it in full before you press anything. If the skeleton is already proven and you are here to
change the model, go to [`30-make-a-change.md`](30-make-a-change.md).
