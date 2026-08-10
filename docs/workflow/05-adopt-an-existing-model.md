# 05 — Adopt a model somebody else wrote

*"Help me refactor the domain model in folder X."*

This is a first-class way in, not a special case. It differs from starting fresh in one way that
changes everything else: **you begin by finding out what you have, not by creating something.**
An inherited model is already running somebody's forecasts, or is about to, and you do not yet
know which of its conventions are deliberate.

You rejoin the main path at [`20-upload-and-debug.md`](20-upload-and-debug.md), and from there it
is identical.

---

## ⚠ Read this before anything else

**A client with an existing custom domain model has a production model that a publish will
replace.** This is the takeover case, and the practice publish the walking skeleton recommends is
exactly the thing that must not happen here.

Go and read [`40-publish.md`](40-publish.md#-before-a-first-publish-on-a-client-that-already-runs-a-custom-model)
**now**, not when you get to step 40. It is short. The whole risk of this page is in it.

Short version: prove the pipeline as far as **F5**, which changes nothing outside the debug
workspace, and stop there. Publish only when you have a change you actually mean to put live.

---

## Step 1 — Get the source, if you do not have it

You may have been handed a folder. You may have nothing but what is on the server.

**If it only exists on the server:** open the **Debug Model** page (you will need the project lock
— [`00-prerequisites.md`](00-prerequisites.md#2-you-must-hold-the-project-lock)) and click
**Download source zip** on the ribbon, beside **Upload zip**.

Unzip it **beside this repository**, never inside it:

```
C:\...\somewhere\
    JCassDomainModelAssistant\
    TheirModel\               <- here
```

The zip opens straight to the `.csproj`, so unzipping it into an empty folder gives you the project
directly, with no wrapper folder to remove.

> **The download deliberately leaves out `refs\`.** Those are the server's own framework
> assemblies and must never end up in a local project. Your local build needs a `refs\` folder of
> its own — see step 3.
>
> It also leaves out the editor configuration and some server-side state files. Nothing is lost:
> **Initialize workspace** rewrites those on demand.

If **Download source zip** reports that there is nothing to download, the server workspace has
never been initialised or nothing was ever uploaded to it — the model exists in the registry but
not as source you can edit. That is a real gap and it needs Juno: **support@lonrix.com**.

## Step 2 — Check it, before you read a line of it

```powershell
.\tools\jcass-dm.exe check --project ..\TheirModel
```

**Do this first, always.** It is what tells you — and the engineer — what state the model is
actually in, and it takes seconds. Reading the C# first means forming an opinion about a model
whose bundle you have not looked at.

Read the result out to the engineer in plain terms. What each rule is telling you:

| Rule | If it is not OK |
|---|---|
| `one .csproj at the root` | Two project files, or none. Nothing else can be trusted until this is resolved |
| `the four names` | **Deal with this before anything else** — see step 3 |
| `<AssemblyName>` | Set explicitly, which breaks the four names silently. [`silent-failures.md` § 6](../conventions/silent-failures.md#6-assemblyname-set-in-the-csproj) |
| `bundle structure` | The setup spreadsheet is missing a sheet or a column. The model will not load at all |
| `parameters vs C#` | A declared parameter is never written — a column of zeros in the outputs that looks like a result. **The single most valuable thing this tool finds** |
| `treatments vs C#` | A treatment name typed differently in the bundle and the C#. It will simply never be produced |
| `treatment reset arms` | A treatment with no `case` arm. This one fails loudly, the first time it is applied |
| `budget categories` | Reported as a `NOTE`; the tool cannot see the client's budget sheet |
| `lookup sets` | `SKIPPED` unless you pass `--lookups`. Do pass it — step 4 |

**A clean check does not mean the model is good.** It means nothing locally visible is
*inconsistent*. It says nothing about whether the engineering is right, and it says so itself at
the bottom of every run.

## Step 3 — Fix the names, and the build

### If the four names disagree

```powershell
.\tools\jcass-dm.exe rename TheirModel --project ..\TheirModel
```

`rename` changes all four together — the project file name, the entry class and its file, and the
two `meta` settings in the bundle — or it changes none of them. **Never talk an engineer through
doing this by hand.** A half-renamed model is worse than the original mismatch, and this is the
single most common way a working model gets broken.

Pick the name carefully; it is the model's identity everywhere. If in doubt, rename to whatever
the `.csproj` is already called, since that is the name a debug run derives its own from.
[`../conventions/four-names.md`](../conventions/four-names.md).

### If it does not build for want of `refs\`

A downloaded or handed-over project usually has no `refs\` folder. Copy this repository's:

```powershell
Copy-Item -Recurse -Force .\refs\* ..\TheirModel\refs\
```

If `..\TheirModel\refs\` does not exist yet, create it first with **File Explorer**, or:

```powershell
New-Item -ItemType Directory -Force ..\TheirModel\refs | Out-Null
```

Then:

```powershell
dotnet build ..\TheirModel\<TheirModel>.csproj -c Debug --no-incremental
```

If the `.csproj` references framework assemblies some other way — a `<ProjectReference>`, a
hard-coded path, a NuGet package — the reference model's project file shows the shape it should
have, and why:
[`../../reference-model/DomainModelSample/DomainModelSample.csproj`](../../reference-model/DomainModelSample/DomainModelSample.csproj).

**A build error naming a framework type that does not exist is a signal, not a puzzle.** It usually
means the model was written against an older framework than the one in `refs\`. Say so plainly and
stop rather than inventing a replacement call:
[`../conventions/when-to-stop.md`](../conventions/when-to-stop.md).

## Step 4 — Find out what the model needs from the client

Three questions, and the answers come from the model itself rather than from anyone's memory.

**Which lookup sets does it require?** Download the client's `inputs\lookups.xlsx` from
**Files → Inputs** and re-run the check against it:

```powershell
.\tools\jcass-dm.exe check --project ..\TheirModel --lookups ..\lookups.xlsx
```

The `lookup sets` rule stops being skipped and names anything missing. Missing sets are not silent
— they stop the run at setup — but finding them now is much cheaper.

**Which side-car CSVs does it read?** Search the C# for file reads:

```powershell
Select-String -Path ..\TheirModel\Objects\*.cs -Pattern ".csv" -SimpleMatch
```

Then look at the folder each path is built against.

> **A path built against the bundle folder is a live problem for you, not a style point.** Under a
> debug F5 run the bundle folder is a different folder from the one a normal run uses, so a
> bundle-relative CSV read works in exactly one of the two. On a client that has never had a
> normal run, F5 fails at setup with a *"Could not find file"* naming a `domain_model/` path.
>
> The fix is to move those CSVs to the client's `supporting\` folder and resolve them against
> `WorkFolder`, which is the client root under both kinds of run — measured, not inferred.
> [`../conventions/naming-and-folders.md`](../conventions/naming-and-folders.md#supporting-versus-the-bundle-for-a-side-car-csv).
> **Recommend it; do not do it silently.** It changes where the modeller uploads a file.

**Which input columns does it expect?**

```powershell
.\tools\jcass-dm.exe dump ..\TheirModel\domain_model_setup.xlsx --sheet input_headers
```

`dump` prints any sheet of the bundle as text, in a stable order — so you can also take a dump
before and after a change and compare the two line by line.

## Step 5 — Map it onto the canonical skeleton

Every real domain model converges on the same shape, whatever its files are called. Your job here
is to answer, for this model, **which file owns which stage** — and to write the answer down for
the engineer, because they will need it every time they change anything.

| Execution stage | The canonical file | This model's… |
|---|---|---|
| Entry class — the switchboard the framework loads | `<ModelName>.cs` | ? |
| Thresholds and rates, read from `lookups.xlsx` | `Constants.cs` | ? |
| What one asset is, and the state it carries | `ModelElement.cs` | ? |
| Framework data → element, **two methods** | `ModelElementFactory.cs` | ? |
| Period 0 starting state | `Initialiser.cs` | ? |
| One period of deterioration | `Incrementer.cs` | ? |
| What a treatment does to an element | `Resetter.cs` | ? |
| When work is due, and what it costs | `TreatmentsTrigger.cs` | ? |
| Routine maintenance | `RoutineMaintenance.cs` | ? |
| Treatment name constants | `TreatmentNames.cs` | ? |

The stages themselves, and what the framework calls when:
[`../orientation/how-a-run-works.md`](../orientation/how-a-run-works.md). A model that folds
several stages into one class is not wrong — the reference model does exactly that — it just means
one file appears in several rows.

**Frame this as questions you answer for the engineer, not as a code review.** They have inherited
this model and have to work in it; "here is where deterioration happens, here is where a treatment
is decided" is useful on day one. "This class has too many responsibilities" is not, and it is not
what was asked for.

Three more worth answering while you are in there:

- **Which treatments exist**, and where each one is triggered.
- **Which numbers are hard-coded in C# that should be in `lookups.xlsx`.** Report them; do not move
  them yet. Moving a threshold changes the forecast, and that is the engineer's decision —
  [`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md).
- **Anything that reads `NElements`, `NPeriods` or `NParameters` during setup**, which is zero at
  that point and produces an empty model that runs to completion.
  [`../conventions/silent-failures.md` § 3](../conventions/silent-failures.md#3-reading-nelements-nperiods-or-nparameters-during-setup).

## Step 6 — Now go and prove it runs

Join the main path at [`20-upload-and-debug.md`](20-upload-and-debug.md): package, upload,
initialise, F5.

**Prove the model you inherited runs unchanged before you change anything.** Same reason as the
walking skeleton: if you refactor first and F5 fails, you cannot tell whether you broke it or
whether it was already broken. Get one green debug run out of the code as you received it, and
every failure after that is attributable.

Then, and only then, [`30-make-a-change.md`](30-make-a-change.md).

---

## When an inherited model does something these docs do not cover

This is where it happens. Inherited models use framework calls that are not in any pattern here,
and some of those calls are not in the API reference at all.

- **In the API reference, not in the patterns** → fine. Composition is the normal case. Work with
  it, and say plainly that it is not one of the canonical shapes if you build something new around
  it.
- **Not in the API reference** → **stop.** Do not infer the signature from the surrounding code.
  That is invention wearing composition's clothes, and the engineer cannot tell the difference.
  Draft a support request instead: [`../conventions/when-to-stop.md`](../conventions/when-to-stop.md),
  [`../support-request-template.md`](../support-request-template.md), **support@lonrix.com**.

The same applies to a failure these pages do not describe. An unfamiliar framework failure
diagnosed by inspection is how the wrong root cause gets fixed for an afternoon.

## Done when

- [ ] `jcass-dm check` runs clean, or every remaining item is understood and reported.
- [ ] The four names agree.
- [ ] It builds locally, 0 warnings.
- [ ] You can say which file owns which stage, which lookup sets it needs, and which side-car
      files it reads.
- [ ] It has had one green F5 run **before** you changed anything.
