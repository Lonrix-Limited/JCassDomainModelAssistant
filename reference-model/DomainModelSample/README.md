# DomainModelSample

A deliberately small, working Juno Cassandra **domain model**. Clone this folder, rename it, and
refactor it into your own model. It is not a library to reference — it is a starting point to
edit.

---

## 1. What the framework does, and what you do

The **framework model** owns the loop. It reads your network's input data, steps through
modelling periods, asks an optimiser which treatments to fund within the budget, and writes the
outputs. You never write any of that.

The **domain model** — this project — owns the engineering. It answers four questions, once per
element per period:

| Question | Method |
|---|---|
| What condition does this element start in? | `Initialise` |
| What work could be done to it this period? | `GetTreatmentCandidates` |
| How does it decay if nothing is done? | `Increment` |
| How does it improve if something is done? | `Reset` |

Plus two smaller ones: `GetTriggeredMaintenance` (work that happens regardless of budget) and
`DoEndOfPeriodCalculations` (network-wide sums, once per period).

The framework loads your compiled DLL by reflection at run time, finds the one class that
implements its interface, and calls those methods. It has no compile-time knowledge of your code.

---

## 2. What this particular model does

A network of generic assets, each with an age, a condition rating (0 = good, 100 = poor), a
material and an area.

- Elements deteriorate by a fixed number of condition points per year, set by material —
  plastic decays ten times faster than titanium.
- Once an element is old enough and bad enough, it triggers a **repair**. Once it is worse than
  a repair can fix, it triggers a **replace**.
- An element that triggers a repair is *also* offered a replace, so the optimiser has a genuine
  choice between the cheap holding action and the permanent fix.
- Anything worse than condition 50 also picks up **routine maintenance** every period, outside
  the budget competition.
- Cost is `quantity × unit rate` — area in square metres, times a per-square-metre rate that
  varies by material.

None of that is meant to be right. It is meant to be small enough to read in ten minutes and
structured the way a real model should be structured.

---

## 3. The four-name rule — read this before you rename anything

**Four strings must be identical. In this kit they all read `DomainModelSample`.**

| # | Where | Current value |
|---|---|---|
| 1 | The `.csproj` filename | `DomainModelSample.csproj` |
| 2 | The assembly name | `DomainModelSample` (inherited from #1 — **`<AssemblyName>` is deliberately not set**) |
| 3 | The entry class | `public class DomainModelSample : DomainModelBase` |
| 4 | `meta.main_dll` / `meta.main_class` in `domain_model_setup.xlsx` | `DomainModelSample.dll` / `DomainModelSample` |

This is not tidiness. Your model gets loaded by **two different routes**, and they disagree about
where the name comes from:

- A **normal model run** reads the `meta` sheet of your bundle. It loads whatever `main_dll` says,
  and looks inside it for whatever class `main_class` says.
- A **debug run** (pressing F5 in the web Debug Model page) **ignores the `meta` sheet entirely**
  and derives both the DLL name and the class name from your `.csproj` filename. It has to: when
  you are mid-edit, your source has usually drifted away from whatever identity was last shipped,
  and the meta sheet still describes the shipped version.

So the two routes only ever agree when all four strings match. When they don't, everything looks
fine until you press F5 and get:

```
Domain Model class 'Whatever' was not found in the specified .dll
```

The framework's unit-test domain model gets this wrong — its csproj is `JCassUnitTestDomainModel`
but its class is `UnitTestDomainModel` — and F5 against it fails for exactly this reason. That
mistake is the reason this kit exists.

### Renaming this kit to your own model

Do all four in one sitting, then build:

```powershell
# 1. Rename the project file
git mv DomainModelSample.csproj MyRoadModel.csproj      # or: Rename-Item, if not yet in git

# 2. Rename the entry class file
git mv Objects\DomainModelSample.cs Objects\MyRoadModel.cs
```

3. Open `Objects\MyRoadModel.cs` and rename the class `DomainModelSample` → `MyRoadModel`.
   Also update the namespace across all files if you want (`DomainModelSample.Objects` →
   `MyRoadModel.Objects`) — the namespace is *not* part of the rule, but leaving a stale one is
   confusing.
4. Open `domain_model_setup.xlsx`, sheet `meta`, and set `main_dll` to `MyRoadModel.dll` and
   `main_class` to `MyRoadModel`.

Then rebuild and confirm `bin\Debug\net9.0\MyRoadModel.dll` exists.

**Do not add `<AssemblyName>` to the csproj.** It is the one setting that can break the rule
silently: the file would still say `MyRoadModel.csproj` while the DLL came out under a different
name, and only the debug path would notice.

---

## 4. Build it

Nothing to fetch. The framework assemblies are committed in `refs\`, so from the project root:

```powershell
dotnet build DomainModelSample.csproj -c Debug --no-incremental
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`, and `bin\Debug\net9.0\DomainModelSample.dll`.
Open the folder in VS Code and you get IntelliSense on the framework types straight away.

### You can build it here; you cannot run it here

The assemblies in `refs\` are **reference assemblies** — the framework's full public API with no
method bodies. That is enough for the compiler and enough for IntelliSense, and it is deliberately
not enough for the runtime. Anything that tries to execute the framework on your machine compiles
cleanly and then fails at load with:

```
System.BadImageFormatException: ... Reference assemblies cannot be loaded for execution.
```

That is expected. You **author** here and you **run and debug** on the web app's Debug Model page —
see §9, and [`refs\README.md`](refs/README.md) for the detail.

`refs\FRAMEWORK-VERSION.txt` records which framework build these came from, down to its git commit
SHA. You do not refresh them yourself: a newer framework arrives with a newer release of the
Assistant. From the Assistant's root:

```powershell
.\scripts\check-framework-version.ps1
```

**Build Debug while you work; ship Release.** The DLL that goes into the domain-model registry is
what *regular* model runs load, so it should be optimised:

```powershell
dotnet build DomainModelSample.csproj -c Release
```

Debug buys you nothing there — a web **Debug Model** F5 run never loads the registry DLL at all, it
builds from your source and loads `bin\Debug\net9.0\` out of the workspace. Shipping the PDB
alongside the Release DLL is optional but worth it: it puts line numbers in the stack trace when a
regular run throws.

### Packaging it up — two zips, different contents

These get confused, and getting it wrong wastes an afternoon:

| Zip | Contents | Why |
|---|---|---|
| **Sending the kit to someone**, for local development | Everything, **including `refs\`** | They need the DLLs to compile and to get IntelliSense |
| **Uploading to the web Debug Model page** | Source only, **no `refs\`** | That workspace stages its own larger set; overwriting part of it is how local and server drift apart |

For the upload zip, select the items **inside** the project folder — `Objects`,
`DomainModelSample.csproj`, `domain_model_setup.xlsx`, `README.md` — right-click, **Send to →
Compressed (zipped) folder**. Do not zip the folder itself: if the zip opens to a
`DomainModelSample\` folder rather than straight to `Objects` and the `.csproj`, everything lands
one level too deep and F5 fails with *"No .csproj file found at workspace root"*.

`bin` and `obj` are filtered out by the upload, so they do no harm if they slip in. **`refs\` is
not — leave it out yourself.** It matters more than it used to: the assemblies here cannot be
executed, so overwriting part of the workspace's staged set with them replaces working framework
assemblies with ones that will not load, and the failure arrives at F5 looking like nothing to do
with a zip.

### Why `refs\` and not a project reference

The project references framework assemblies out of `refs\` with a wildcard, not via
`<ProjectReference>`. Two reasons:

- You do not have the framework source, so a project reference is not available to you.
- The web Debug Model workspace stages framework DLLs into `refs\` for you when you initialise
  the workspace — and it stages far more of them than a local copy does, because it includes
  NuGet transitives. A wildcard picks up whatever is actually there, so this same `.csproj`
  builds unchanged on your machine and on the debug sidecar. A hard-coded list would need editing
  every time you moved between the two.

The contents of `refs\` **are committed** — the assemblies and their `.xml` documentation files
both, so that the project compiles and gives IntelliSense the moment it is cloned or unzipped.
Symbols are the one thing not carried. See [`refs\README.md`](refs/README.md).

---

## 5. The file map

```
DomainModelSample.csproj        Build config. The filename is load-bearing — see §3.
domain_model_setup.xlsx         The bundle: what the framework needs to know before it loads you.
refs\                           Framework reference assemblies + their docs. See refs\README.md.
Objects\
  DomainModelSample.cs          Entry class. A switchboard — keep it thin.
  Constants.cs                  Tunable numbers, read from lookups.xlsx. See §7 — copy this shape.
  SampleElement.cs              What an asset is, and how it decays and recovers.
  ElementFactory.cs             Framework dictionaries -> SampleElement. All column names live here.
  TreatmentTrigger.cs           When work is due, and what it costs. The engineering judgement.
  StrategyGenerator.cs          What options the optimiser gets to choose between.
  TreatmentNames.cs             Treatment name constants, shared with the bundle.
```

Beyond the bundle, this model reads the client's `inputs\lookups.xlsx` for every threshold and
rate — see §7. It needs the lookup sets `repair_thresholds`, `replace_thresholds` and
`unit_rates` to be present, and fails at setup with a message naming the missing one if they
are not.

`domain_model_setup.xlsx` has five sheets, all required:

| Sheet | What it declares |
|---|---|
| `meta` | Which DLL and which class to load, plus a display name. |
| `input_headers` | The columns your model expects in the client's input CSV. |
| `parameters` | The per-element state your model carries between periods. |
| `treatments` | The treatments your model can produce, and which budget each is charged to. |
| `network_functions` | Framework-computed network statistics. Empty here — header row only. |

---

## 6. How to make the four common changes

### Add an input column

The client's input CSV gains a column, say `traffic_count`.

1. `domain_model_setup.xlsx` → `input_headers` → add a row: category `general`, column_name
   `traffic_count`, data_type `number`, an example value, a comment.
2. `Objects\ElementFactory.cs` → add `TrafficCount = numInputs["traffic_count"],` to **both**
   factory methods. Missing the second one is the classic bug: the model behaves correctly in
   period 0 and wrongly from period 1.
3. `Objects\SampleElement.cs` → add the matching property.

Text columns come from `textInputs`, numeric from `numInputs`. The framework rejects nulls in
numeric columns, so a client CSV with blanks must use a sentinel value instead.

### Add a model parameter

Parameters are the state that survives from one period to the next. If a value must be remembered,
it is a parameter; if it can be recomputed from inputs and other parameters, it need not be.

1. `domain_model_setup.xlsx` → `parameters` → add a row. `minimum`/`maximum` are validation
   bounds, not clamps — the run fails if a value lands outside them, which is usually what you
   want.
2. `Objects\SampleElement.cs` → write it in `SetParameterValues`. **Every parameter in the sheet
   must be written there**, or setup fails.
3. `Objects\ElementFactory.cs` → read it back in `GetElementFromModelData`.

Numeric parameter names conventionally start with `par_`.

### Add a treatment

1. `Objects\TreatmentNames.cs` → add the constant.
2. `domain_model_setup.xlsx` → `treatments` → add a row using exactly that string as
   `treatment_name`. Set `budget_category` to a column that exists in the client's
   `inputs\budgets.xlsx` — a category with no budget column never gets funded, silently.
3. `Objects\TreatmentTrigger.cs` → decide when it fires and what it costs.
4. `Objects\StrategyGenerator.cs` → decide whether it competes with the others as an alternative.
5. `Objects\SampleElement.cs` → handle it in `Reset`. The `default:` branch throws, so a treatment
   you forget here fails loudly rather than doing nothing.

### Change a threshold or a rate

**Do not do this in C#.** Edit the client's `inputs\lookups.xlsx` — or use the web app's Tuning
page, which writes to it for you — and re-run. No rebuild. If the number you want is not there
yet, add it to `lookups.xlsx` and to `Objects\Constants.cs`; §7 has the three-step worked example.

---

## 7. Where the numbers live — read this before you write a threshold

**Thresholds and rates belong in `inputs\lookups.xlsx`, not in C#.**

This is not a style preference. A modeller recalibrating a model — moving a repair trigger from
age 15 to age 12, escalating replacement costs by 10% — must be able to do it themselves, look at
the result, and try again. The web app's **Tuning** page exists for exactly this and writes back
to `lookups.xlsx`. Every number you hard-code in C# is a number they have to ask a developer for,
wait on a rebuild for, and wait on a deploy for. That is the difference between a model someone
can calibrate and a model they can only file tickets against.

`Objects\Constants.cs` is the worked example. Copy its shape.

### How lookups.xlsx is structured

Any sheet whose name starts with `lkp_` is read, and all of them are merged into one flat table.
Three columns matter:

| lookup_set_name | setting_key | setting_value |
|---|---|---|
| `repair_thresholds` | `age_gt` | 15 |
| `repair_thresholds` | `cond_gt` | 50 |
| `replace_thresholds` | `age_gt` | 5 |

A value is addressed by the **pair** (set name, key). Which sheet a row sits in is only an
organisational convenience — it is not part of the address, so you can regroup sheets freely
without touching code. This kit reads two sets of thresholds from `lkp_project` and the treatment
rate adjustments from `lkp_unit_rates`.

### Reading them

`Constants` takes `this.model.Lookups` — a `Dictionary<string, Dictionary<string, object>>` keyed
by set name then setting key — and reads out of it directly:

```csharp
var unitRateSet = lookupSets["unit_rates"];
if (!unitRateSet.ContainsKey(treatmentName))
    throw new Exception($"Unit rate for Treatment '{treatmentName}' not found in lookup set 'unit_rates'.");
double unitRate = Convert.ToDouble(unitRateSet[treatmentName]);
```

Guard before you index, and name the set and key in the message. A typo in the spreadsheet then
fails immediately at setup with something you can act on, rather than a bare `KeyNotFoundException`
or — worse — a silent default that skews an entire run.

`Convert.ToDouble` is not optional: `setting_value` arrives as **text** regardless of how the cell
looks in Excel, so a cast to `double` throws an unhelpful `InvalidCastException`.

`DomainModelBase` also offers `GetLookupValueNumber(set, key)` and `GetLookupValueText(set, key)`
if you would rather not hold the dictionary yourself. They do the same guarding.

### Rates are looked up by treatment name

Note the shape of `Constants.GetUnitRate` — it keeps the whole `unit_rates` set and resolves a rate
by treatment name at the point of use, rather than unpacking each one into a property. That means
adding a treatment costs a row in `lookups.xlsx` and a constant on `TreatmentNames`, and **nothing
at all in `Constants`**. Thresholds are unpacked into properties because each one is used in a
different comparison; rates are not, because they are all used the same way.

**Read them in `SetupInstance()`, never earlier.** The framework populates lookups immediately
before calling it. A lookup read from a constructor or a static initialiser gets an empty
dictionary — and because that reads as "key not found" rather than "too early", it is a
genuinely confusing hour to lose.

### The one number deliberately left hard-coded

`TreatmentTrigger.RoutineMaintenanceConditionGreaterThan` is still a `const`, as the
counter-example. Compare it against anything on `Constants` and the difference in who can change
it is the whole point. Moving it is a good first exercise:

1. Add a row to `lkp_project`: set `maintenance_thresholds`, key `cond_gt`, value `50`.
2. Add a property to `Constants` reading it, alongside the others.
3. Replace the `const` reference in `TreatmentTrigger.GetTriggeredMaintenance`.

The per-material deterioration and cost rates in `SampleElement` are hard-coded for the same
reason — they are the second exercise, and in a real model they belong in lookups too, since
deterioration rates are exactly what gets recalibrated against observed condition data.

---

## 8. What not to touch

- **`<AssemblyName>`** — leave it unset. See §3.
- **The method signatures on `DomainModelSample`.** They are `override`s of an abstract base the
  framework owns. Change one and it stops compiling; work around it and the framework stops
  finding your class.
- **The five sheet names in `domain_model_setup.xlsx`.** All five must exist, spelled exactly, even
  `network_functions` with no rows in it.
- **The `refs\` folder contents.** They arrive with the Assistant and are replaced wholesale when
  you download a newer one. Never edit them, and never mix assemblies from two framework releases
  in one folder — the reference is a wildcard, so a leftover gets compiled against rather than
  ignored.
- **`Private=false` on the `<Reference>` item in the csproj.** It stops framework DLLs being copied
  next to your own. At run time the host has already loaded its own copies, and a duplicate beside
  yours only creates version confusion. Your `bin\` should hold your DLL and its PDB, nothing else.
- **`bin\` and `obj\`.** Git-ignored, and regenerated by every build.

---

## 9. Debugging it in the browser

The web app's **Debug Model** page runs this project on the server with a real debugger attached,
so breakpoints in your source fire mid-run. Upload this folder as a zip, initialise the workspace,
press F5.

Two things about this kit are what make that work, and both are easy to undo by accident:

- The four names agree (§3), so the debug run resolves your class.
- There is exactly **one `.csproj` at the top level**. The debug run refuses to guess between two.

If your breakpoints show as hollow rather than solid, the debugger did not find the PDB for the DLL
it loaded — usually a stale DLL somewhere. Rebuild, and check the DLL and PDB timestamps match.
