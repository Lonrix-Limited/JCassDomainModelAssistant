# Logistic coefficients

**Compiling example:** [`LogisticCoefficients.cs`](../../examples/ExamplesLibrary/LogisticCoefficients.cs)
**API:** [`../framework/api/authoring/LogisticModel.md`](../framework/api/authoring/LogisticModel.md)
**Where the coefficients file lives:** [`setup-data-from-supporting-csv.md`](setup-data-from-supporting-csv.md)

---

## When to reach for it

**When something either happens or it does not, and the model needs the probability.**

Does this pipe fail this year. Does this segment need maintenance this period. Is this surface going
to be potholed by the next survey. A logistic regression is the standard answer, the engineer will
usually already have one fitted in R or Python, and it arrives as a two-column CSV.

Same pattern applies to [`LinearRegressionModel`](../framework/api/authoring/LinearRegressionModel.md)
for a continuous outcome — coefficients from a file, predictors by name, and the same trap in the
middle.

---

## The shape

### Load at setup, from `supporting\`

```csharp
private static LogisticModel BuildLogisticModel(string workFolder, string fileName)
{
    jcDataSet coefficientData = SetupDataFromSupportingCsv.ReadSupportingCsv(workFolder, fileName);

    Dictionary<string, double> coefficients = new Dictionary<string, double>();

    for (int iRow = 0; iRow < coefficientData.Count; iRow++)
    {
        Dictionary<string, object> row = coefficientData.Row(iRow);

        string term = SetupDataFromSupportingCsv.GetText(row, "term", fileName, iRow);
        double estimate = SetupDataFromSupportingCsv.GetNumber(row, "estimate", fileName, iRow);

        if (coefficients.ContainsKey(term))
        {
            throw new Exception($"Term '{term}' appears more than once in '{fileName}'.");
        }

        coefficients[term] = estimate;
    }

    if (coefficients.Count == 0)
    {
        throw new Exception($"'{fileName}' contains no coefficient rows.");
    }

    return new LogisticModel(coefficients);
}
```

`term` and `estimate` are R's `broom::tidy()` column names, which is why the working models use
them. Any two column names will do as long as the code and the file agree.

### Predict

```csharp
Dictionary<string, double> predictors = new Dictionary<string, double>
{
    { "age", segment.Age },
    { "cond_grade", segment.ConditionGrade },
    { "break_rate", segment.BreakRatePerKmYear },
    { "diameter_mm", segment.DiameterMm },
};

return model.PredictProbability(predictors);
```

`PredictProbability` returns the probability. `PredictLogit` returns the linear predictor before the
logistic transform, which is occasionally what you want if you are combining models.

---

## The trap: the term names are a contract, and nothing checks it

**The keys in the dictionary must match the `term` column of the CSV exactly.** Including:

- **The intercept's own name.** R writes `(Intercept)`. If your file says that, your code must too —
  and normally the intercept is *not* something you pass in, so you only meet this when a term is
  missing rather than extra.
- **Any transform the fit applied.** If the regression was fitted on `log(pressure)` then the term
  is literally the string `log(pressure)`, and the value you pass must be
  `Math.Log(segment.Pressure)` — not the pressure. A model handed the untransformed value returns a
  probability, and it is wrong by a factor nobody will notice from the outputs.
- **Renames from a refit.** A statistician who tidies `cond_grade` to `condition_grade` between
  fits has broken the model, and the break shows up at the first element of the first period rather
  than at build time.

There is no compile-time check available for any of this. Two things help:

1. **Keep the predictor dictionary in one method**, as the example does, so the whole contract is
   readable in one place rather than scattered across triggers.
2. **When a refit arrives, diff the `term` column against the previous file** before uploading it.
   That takes a minute and it is the only place the mismatch is cheap to catch.

---

## Two smaller decisions worth copying

### Reject a duplicated term rather than letting the later row win

R and Python both emit one row per term. A duplicate means the file was concatenated or hand-edited,
and silently taking the last one produces a model nobody fitted.

### Separate models rather than a branch inside one

The example loads two files — metallic and non-metallic — because the fit was done separately per
material family. That is a modelling decision, and keeping it as two files and two objects makes it
visible to whoever refits them. Folding it into one file with a `material_family` term would be a
different model, and a legitimate one, but it should be a decision rather than an accident of how
the code was written.

Which family a material belongs to **stays in C#**:

```csharp
private static bool IsMetallic(string materialType)
    => materialType is "cast_iron" or "ductile_iron" or "steel";
```

These are the values the client's input column actually contains. Changing one would not recalibrate
the model, it would stop it recognising the data — so they are structural, and they belong here,
named. A modeller whose data has a new material needs this list extended **and** a coefficient file
to go with it, which is a code change either way.

---

## Related

- [`setup-data-from-supporting-csv.md`](setup-data-from-supporting-csv.md) — where the coefficients file goes, and the guards around reading it
- [`constants-from-lookups.md`](constants-from-lookups.md) — a calibration factor applied to a predicted probability is a tunable scalar and belongs in `lookups.xlsx`
- [`distribution-simulators.md`](distribution-simulators.md) — the other shape an engineer arrives with; same storage answer
- [`../framework/api/authoring/LinearRegressionModel.md`](../framework/api/authoring/LinearRegressionModel.md) — the continuous-outcome equivalent, including fitted residual spread
- [`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md) — why coefficients are never in `lookups.xlsx`
