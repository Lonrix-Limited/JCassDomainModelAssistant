# 02 — Start from the starter model, not from an empty folder

**Goal of this page:** the two folders you need on your machine before you write a line of C# — the
**starter model** that Lonrix set up for your client, and a **project snapshot** of the client's real
setup and input files.

**There is no "start from scratch", and that is deliberate rather than a missing feature.** A custom
domain model does not exist on its own in Juno Cassandra: it needs a client project around it with
input data, budget columns, configurations, lookups, a registry entry and a publish grant. Somebody
has to create all of that, and it is not you. Starting from an empty project folder means writing C#
against setup files you have never seen and finding out what they actually contain at your first
upload — a fortnight's work discovered in the wrong order.

---

## Step 1 — Confirm the starter model exists

**Ask, before anything else: has Lonrix set up the starter model for this client, and has it been run
online at least once?**

The starter model is a small, complete, *working* custom domain model. It is not a stub and not a
placeholder — it forecasts something, and it has already been published and run in the client's own
project, which is what proves the input files, the budget columns and the configurations around it
are real and consistent with each other.

That one run is the point of the whole arrangement. It means every failure you meet from here on is
attributable to a change *you* made, because the pipeline demonstrably worked before you touched it.

**If there is no starter model, stop here.** That is not a licence to scaffold one — the model has to
exist in the client's project as a registered custom domain model before any of this works, and
creating it is a Lonrix action. Email `support@lonrix.com` and say which client you are working on.
[`../conventions/when-to-stop.md`](../conventions/when-to-stop.md).

## Step 2 — Get the model source from Lonrix

**Lonrix hands over the starter model as a complete C# project folder**, ready to build: a `.csproj`
at the top, an `Objects\` folder, a `domain_model_setup.xlsx` bundle, and a `refs\` folder of
framework reference assemblies.

**Where you put it is up to you, with one hard rule and one strong default:**

- **Hard rule: not inside the Assistant folder.** Everything in `JCassDomainModelAssistant\` is
  replaced when you take a newer version, and an upload zip must contain your model and nothing else.
  [`../conventions/naming-and-folders.md`](../conventions/naming-and-folders.md).
- **Strong default: beside it.** `C:\Work\JCassDomainModelAssistant\` and `C:\Work\NelsonRoads\` next
  to each other is what makes every command on these pages — `..\NelsonRoads` — mean what it says. If
  you put the model somewhere else that is fine and everything still works; you will be typing full
  paths instead of `..\`, and your assistant has to be able to read the folder.

> **Assistants: resolve the path out loud and get a yes before writing anything into it.**
> [`../00-start-here.md`](../00-start-here.md) § 1.

## Step 3 — Download a project snapshot

**In the web app: Postprocessing → Project snapshot → "Snapshot / Archive Setup and Outputs".** Add a
short description if you like — it is written into a `README.md` inside the zip — and download.

**Unzip it to a short folder such as `C:\work\`, or use 7-Zip.** Windows Explorer's own extractor
still enforces the old 260-character path limit and offers you a **Skip** button when it hits one;
skipped files then go missing from your copy with no record of which ones. A deep OneDrive,
SharePoint or Dropbox path can spend 130 characters before the archive's own contents are counted.

You do not need the project lock to take a snapshot, unless somebody else is holding it.

**What is in it, and what it is for:**

| In the zip | Why you want it |
|---|---|
| `inputs\` | The real setup: `lookups.xlsx`, `budgets.xlsx`, `configurations.xlsx`, the MCDA and KPI setups, `styles.xlsx` if the client has one, and the network data CSV |
| `supporting\` | Side-car CSVs the model loads at setup — fitted coefficients and the like |
| `domain_model\` | The bundle and DLL as last staged for a run |
| `outputs\`, `logs\`, `r-outputs\` | The last run's results. Large, and nothing on this path needs them |
| `domain_model_source\` | The debug workspace's source, **minus** `refs\`, `.vscode\`, `bin\` and `obj\` |

> **The snapshot is evidence, not a build source, and this catches people.** `refs\` and `.vscode\`
> are deliberately stripped, so `domain_model_source\` inside the zip **does not compile** and is not
> the folder you work in. The folder you work in is the one from step 2.

## Step 4 — Point your assistant at both folders

Tell it where they are, in full:

> My model is at `C:\Work\NelsonRoads`. The project snapshot is unzipped at
> `C:\work\cassandra_NelsonCityCouncil_20260903`.

What that buys you is the whole reason step 3 exists: your assistant can check the C# it is helping
you write **against the setup files the model will actually meet** — the lookup sets that exist, the
budget columns that exist, the input columns that exist — rather than against a plausible guess. What
it must *not* do with them has a rule of its own:
[`../conventions/input-files-in-scope.md`](../conventions/input-files-in-scope.md).

## Step 5 — Diagnose it before you change it

**`jcass-dm check` is the first thing you run, and it comes before reading the code.**

```powershell
.\tools\jcass-dm.exe check --project ..\NelsonRoads --lookups C:\work\cassandra_NelsonCityCouncil_20260903\inputs\lookups.xlsx
```

Then work through [`05-adopt-an-existing-model.md`](05-adopt-an-existing-model.md), which is written
for exactly this: a model you did not write, diagnosed before it is touched. The starter model *is* an
existing model, and everything that page says about inheriting one applies — including its warning
about publishing, because the client already has a live custom model and it is this one.

## Step 6 — Make it theirs

Once it builds and checks clean, rename it from whatever Lonrix called it to the client's own model
name:

```powershell
.\tools\jcass-dm.exe rename NelsonRoads --project ..\NelsonRoads
```

**Never rename by hand.** Four names have to change together — the `.csproj` filename, the assembly,
the entry class, and two settings inside the bundle — and doing it by hand gets three of them. The
failure surfaces much later as *"class not found in the specified .dll"*.
[`../conventions/four-names.md`](../conventions/four-names.md).

Renaming the project folder itself, and the C# namespace, are cosmetic by comparison and can follow.

---

## Done when

- [ ] The starter model exists in the client's project and has been run online at least once.
- [ ] Its source is on your machine, outside the Assistant folder, and it **builds**.
- [ ] A project snapshot is unzipped somewhere short, and your assistant knows the path.
- [ ] `jcass-dm check` runs clean, or every finding is understood.
- [ ] The four names read the client's model name, set by `rename` rather than by hand.

Next: [`10-scaffold-and-build.md`](10-scaffold-and-build.md) § step 3 for the build loop, then
[`20-upload-and-debug.md`](20-upload-and-debug.md).

---

## For the assistant

**Do not offer to create a model from nothing.** *"I want to start a new domain model"* is answered by
this page, not by `jcass-dm scaffold`. Ask whether Lonrix has set the starter model up; if the answer
is no, that is a stop and an email, not a scaffold.

**`scaffold --from-sample` still exists and is still correct — for Lonrix, producing a starter
model.** [`10-scaffold-and-build.md`](10-scaffold-and-build.md) documents it, and it is also what
somebody working entirely outside a Juno Cassandra client project would use. It is not the first thing
you suggest to an engineer who has a client.

**The engineering questions have not gone away.**
[`01-plan-your-model.md`](01-plan-your-model.md) is still the half hour that decides the fortnight —
read against a starter model it becomes a set of questions about what is already there, which is how
that page already tells you to read it for an inherited model. Walk its four guidelines one at a
time, as always.
