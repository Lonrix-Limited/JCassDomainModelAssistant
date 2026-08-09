# Naming and folders — what lives where

Two separate questions that get confused with each other: **where a file belongs on the client's
side**, and **what belongs inside the model project**.

---

## Part 1 — the client's folders

A Juno Cassandra client has a folder on the server. The parts a domain model interacts with:

| Folder | What is in it | Who changes it, and how |
|---|---|---|
| `inputs\` | `lookups.xlsx`, `budgets.xlsx`, `configurations.xlsx`, the network data CSV | The modeller, through the web app's **Files** and **Tuning** pages. No rebuild. |
| `supporting\` | Side-car CSVs — fitted coefficients, per-cohort tables, anything at table scale | The modeller, through the **Files** page. No rebuild. |
| `outputs\` | What a run produces | Written by the run. |
| the model bundle | `domain_model_setup.xlsx` and the compiled DLL | Replaced by a **publish**, which needs a rebuild. |

**The rule that follows from that table:** the further left a number sits, the cheaper it is to
change. Put things as far left as they will go. Which number belongs where is
[`where-numbers-live.md`](where-numbers-live.md).

### `supporting\` versus the bundle, for a side-car CSV

| | Bundle | `supporting\` |
|---|---|---|
| Travels with a publish | Yes | No — uploaded per client on the Files page |
| Same path under a debug F5 run and a normal run | **No** | **Yes** |
| Changeable without a rebuild and republish | No | Yes |
| Visible on the Analyse Input page | No | Yes |

**Use `supporting\`.** Resolve it from the framework's `WorkFolder`, which is the client root under
both kinds of run:

```csharp
string path = Path.Combine(this.model.Configuration.WorkFolder, "supporting", "coefficients.csv");
```

A bundle-relative path reads a different folder under F5 than under a normal run, which produces a
model that debugs and does not run, or the reverse. `WorkFolder` is on
[`../framework/api/authoring/ModelConfiguration.md`](../framework/api/authoring/ModelConfiguration.md).

Existing production models resolve side-car CSVs against the bundle. That is what they happen to do,
not what to copy.

---

## Part 2 — the model project

**The engineer's model lives in its own folder, beside this repository — never inside it.**

```
C:\...\somewhere\
    JCassDomainModelAssistant\     <- this repository. Replaced wholesale on every update.
    MyRoadModel\                   <- their model. Never touched by an update.
```

Two invariants rest on that separation, and everything else follows from them: **the Assistant is
never uploaded**, and **the model folder is always uploaded whole**.

### Inside the project

```
MyRoadModel.csproj          Exactly one, at the top. The filename is load-bearing — four-names.md.
domain_model_setup.xlsx     The bundle. Five sheets, all required, spelled exactly.
refs\                       Framework reference assemblies. Never edited, never uploaded.
Objects\                    All the C#.
bin\  obj\                  Build output. Git-ignored, regenerated, never uploaded.
```

The five bundle sheets are `meta`, `input_headers`, `parameters`, `treatments` and
`network_functions` — the last one header-row-only in most models, and still required. Inspect any
bundle as text without opening Excel:

```powershell
.\tools\jcass-dm.exe dump ..\MyRoadModel\domain_model_setup.xlsx
```

### Naming inside the project

| Thing | Convention | Enforced? |
|---|---|---|
| Numeric model parameter | starts `par_` | No — convention only. The framework requires only that it starts with `p` and is all lowercase. |
| Lookup sheet | starts `lkp_` | **Yes** — any sheet in `lookups.xlsx` whose name starts `lkp_` is read; others are ignored. |
| Lookup value address | the pair (set name, key) | Yes. Which `lkp_` sheet a row sits in is organisational only, so sheets can be regrouped without touching code. |
| Treatment name | a `const` on `TreatmentNames`, used verbatim in the bundle | No, and this is why `jcass-dm check` cross-checks the two. Never type the same treatment string twice. |
| Element class | anything — `RoadSegment`, `Bridge` | No. It is not one of the four names. |

---

## Part 3 — the two zips

**These get confused, and getting it wrong costs an afternoon.**

| Zip | Contents | Why |
|---|---|---|
| **Sending the kit to a colleague** for local development | Everything, **including `refs\`** | They need the assemblies to compile and to get IntelliSense |
| **Uploading to the web Debug Model page** | Source only, **no `refs\`** | That workspace stages its own larger, runnable set |

**Build the upload zip with the tool, not by hand:**

```powershell
.\tools\jcass-dm.exe package --project ..\MyRoadModel
```

It excludes `refs\`, `bin\`, `obj\`, `.git\` and `.vs\`, and it produces a zip that opens **straight
to the `.csproj`** rather than to a folder containing it.

Both halves of that matter. Zipping the folder rather than its contents puts everything one level
too deep and F5 fails with *"No .csproj file found at workspace root"*. Including `refs\` overwrites
runnable framework assemblies with ones that cannot be executed —
[`silent-failures.md` § 8](silent-failures.md#8-a-refs-folder-inside-the-upload-zip).

---

## Part 4 — what not to touch

The list of settings inside a model project that look adjustable and are not — `<AssemblyName>`,
`Private=false`, the `refs\` contents, the five sheet names, the entry class's method signatures —
is written once, with the reason for each, in
[`DomainModelSample/README.md` § 8](../../reference-model/DomainModelSample/README.md#8-what-not-to-touch).

Two more that belong to this page rather than that one:

- **Anything inside this repository**, when the task is to change the engineer's model. The
  Assistant is replaced wholesale at every update, so an edit here is lost; theirs is not.
- **Files in `inputs\` or `supporting\`, from code.** A domain model reads the client's data; it
  never writes to it. Those folders belong to the modeller and to the web app.
