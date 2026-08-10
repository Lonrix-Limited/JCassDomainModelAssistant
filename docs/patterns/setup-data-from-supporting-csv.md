# Setup data from a `supporting\` CSV

**Compiling example:** [`SetupDataFromSupportingCsv.cs`](../../examples/ExamplesLibrary/SetupDataFromSupportingCsv.cs)
**Rule it implements:** [`../conventions/where-numbers-live.md` § the third tier](../conventions/where-numbers-live.md#the-third-tier--a-set-of-coefficients-belongs-in-a-csv)

---

## When to reach for it

**When the numbers arrive as a set, from a fit, in a file.**

You are looking at this pattern if the engineer has, or is about to produce, any of these:

- regression coefficients from a fit;
- logistic or piecewise-linear model parameters;
- distribution definitions for a Monte Carlo model;
- a per-cohort, per-material or per-treatment parameter table with more than a handful of rows.

The test is **update granularity and provenance**, not a count:

> Does this change one value at a time, or as a whole set?
> Was it chosen by judgement, or produced by a fit?

Changed one at a time by a modeller exercising judgement → `lookups.xlsx`, through
[`constants-from-lookups.md`](constants-from-lookups.md). Regenerated as a set by a refit →
here.

**Raise it yourself when you see the shape.** By the time the eighth coefficient has been pasted
into `lookups.xlsx` the structure is set and nobody goes back and restructures it.

The full reasoning behind that boundary is
[`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md), and it is not
repeated here. This page is the mechanism.

---

## Where the file goes, and the one thing to get right

**The client's `supporting\` folder**, reached from `model.Configuration.WorkFolder`:

```csharp
string filePath = Path.Combine(workFolder, "supporting", fileName);
```

`WorkFolder` **is the client root**. That single fact is what makes this work:

| | Bundle (`domain_model\`) | `supporting\` |
|---|---|---|
| Travels with a publish | Yes | **No** — uploaded per client on the Files page |
| Same path under debug (F5) and a normal run | **No** | Yes |
| Changeable without a rebuild and republish | No | Yes |
| Visible on the Analyse Input page | No | Yes |

**Row 2 is the trap.** Under a debug run the bundle scratchpad is `debug_domain_model\`, not
`domain_model\`, and the framework exposes no bundle-folder property at all. So a bundle-relative
side-car path is correct in exactly one of the two run modes — and it fails as *file not found*,
which sends everyone looking for a missing file rather than a wrong folder.

Row 3 is the reason the trade is worth making anyway: a modeller who refits a regression should not
need a code release to use the result.

> **Verified on the server, both run modes, 2026-08-09.** The same `supporting\` CSV resolved to
> the same absolute path under a debug F5 run and under a regular run, and the bundle-relative path
> resolved in exactly one of them. This is measured, not inferred.

**Never resolve `debug_domain_model\` from domain model code.** A normal run has no read grant on
that folder at all, and `File.Exists` returns `false` for a path you are denied — so the code reports
"file not found" for a file that is physically on disk, with nothing anywhere pointing at
permissions.

Some existing models still load side-car CSVs from the bundle. That is what they happen to do, not
what to copy.

---

## The shape

Three moves, and each is load-bearing.

### 1. One helper that resolves and guards

```csharp
public static jcDataSet ReadSupportingCsv(string workFolder, string fileName)
{
    string filePath = Path.Combine(workFolder, "supporting", fileName);

    if (!File.Exists(filePath))
    {
        throw new Exception(
            $"Setup file '{fileName}' not found in the client's supporting folder. " +
            "Upload it on the Files page, under Inputs.");
    }

    return CSVHelper.ReadDataFromCsvFile(filePath);
}
```

**Write this once and call it, rather than repeating the two lines per file.** A model that loads
six side-car CSVs and writes the guard out six times is a model where one of them will drift — in
one real case, calling the reader *before* the existence check, which turns a clear message about
`supporting\` into whatever the CSV reader happens to throw.

**Name the file, not the absolute path.** The engineer uploads through the Files page and never sees
the server's folder layout; the file name is the part they can act on, and it is the part that is
safe to put in a log.

### 2. Read it once, at setup

Everything here is called from `SetupInstance` and the result lives on the model for the rest of the
run:

```csharp
public override void SetupInstance()
{
    this.Constants = new PipeConstants(this.model.Lookups);

    string workFolder = this.model.Configuration.WorkFolder;
    SetupDataFromSupportingCsv.LoadDeteriorationCurves(this.SubModels, workFolder);
}
```

Parsing a CSV and building an object per row is not work to repeat per element per period. Doing it
per element produces **identical numbers**, so nothing tells you it is happening except the clock.

### 3. Check the shape of the data, not just that the file exists

Two checks that earn their keep:

```csharp
// A missing column, named, rather than a KeyNotFoundException later.
setupData.CheckRequiredColumns(
    new List<string> { "cohort_label", "cohort_rule", "cohort_shape" },
    throwErrorIfNotFound: true);
```

```csharp
// An empty file parses perfectly well and produces a model that cannot deteriorate anything.
if (subModels.DeteriorationCurves.Count == 0)
{
    throw new Exception(
        $"'{DeteriorationCurvesFile}' in the supporting folder has a header row but no data rows.");
}
```

The second is the one people miss. A CSV with headers and no rows is a valid CSV.

---

## Cell-level guards

The example carries `GetText` and `GetNumber` helpers that name **the file, the column and the
row**:

> `Column 'material' is empty on row 14 of 'pipe_deterioration_curves.csv'.`

Compare that with `Object reference not set to an instance of an object`. The row number costs one
extra parameter and it is the difference between a fix in seconds and an afternoon.

As with lookups: **convert, never cast.** A CSV cell arrives as text.

---

## What the engineer has to do outside the code

They upload the CSV through the web app's **Files → Inputs** page. `supporting\` is listed there.

It is **not** visible inside the browser debug editor — the code-server workspace is rooted at the
model's source folder, so nothing outside it appears in the file explorer whatever its permissions
are. That is expected, not a fault, and it is worth telling them before they go looking.

**It does not travel with a publish.** Publishing a model to a client that has never had the CSV
uploaded gives a run that fails at setup naming the file — which is the good outcome, but it is a
round trip. Include the file list when handing a model over.

---

## Related

- [`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md) — which tier a number belongs in
- [`constants-from-lookups.md`](constants-from-lookups.md) — the other tier, for tunable scalars
- [`distribution-simulators.md`](distribution-simulators.md) and [`logistic-coefficients.md`](logistic-coefficients.md) — the two shapes engineers most often arrive with; both store their data here
- [`piecewise-linear-models.md`](piecewise-linear-models.md) — curves, from a setup code or from a CSV
- [`../framework/api/authoring/CSVHelper.md`](../framework/api/authoring/CSVHelper.md) and [`../framework/api/authoring/jcDataSet.md`](../framework/api/authoring/jcDataSet.md)
- [`../framework/api/authoring/ModelConfiguration.md`](../framework/api/authoring/ModelConfiguration.md) — `WorkFolder`
- [`../conventions/naming-and-folders.md`](../conventions/naming-and-folders.md) — the folder layout in full
