<!-- ------------------------------------------------------------------
     GENERATED FILE - DO NOT EDIT BY HAND.

     Generated from the framework reference assemblies and their XML
     documentation in refs\, by:

       cassandra_main\scripts\assistant\generate-api-reference.ps1

     The sync is ONE-WAY. Any edit made here is lost the next time that
     script runs, without warning and without a merge conflict. To change
     what this page says, change the /// documentation comments in the
     framework source, or the scoped surface in
     cassandra_main\scripts\assistant\api-surface.json, and regenerate.
     ------------------------------------------------------------------ -->

# LogisticModel

**Namespace:** `JCass_Core.Statistics`  
**Assembly:** `JCass_Core`  
**Kind:** sealed class

> **Should a domain model use this?**  
> Yes, where deterioration or a probability is expressed as a logistic curve. Coefficients come from a CSV, never from C#.

Represents a logistic regression model using coefficients exported from an R `glm(..., family = binomial(link = "logit"))` fit.

**Remarks.** This class implements the standard logistic regression prediction equations:

`η = β0 + β1x1 + β2x2 + ... + βkxk`

`p = 1 / (1 + exp(-η))`

where:

- `η` is the linear predictor (logit)
- `p` is the predicted probability
- `β0` is the intercept
- `βi` are the predictor coefficients

Constraints and assumptions:

- The coefficient dictionary must contain an intercept term named exactly `(Intercept)`.
- This class assumes there are no interaction terms such as `a:b`.
- This class assumes there are no transformed terms such as `log(x)`, `poly(...)`, splines, factor expansions, or offsets.
- Each non-intercept coefficient name must exactly match a key in the predictor-value dictionary passed to prediction methods.
- If any required predictor is missing during prediction, an exception is thrown.
- Extra entries in the predictor-value dictionary are ignored.
- This class is intended for models fitted with the logit link. It is not suitable for probit, cloglog, multinomial, ordinal, or other model forms.

The probability calculation uses a numerically stable implementation to reduce overflow risk for large positive or negative logits.

## Constructors

### LogisticModel

```csharp
public LogisticModel(Dictionary<string, double> coefficients)
```

Initializes a new instance of the `JCass_Core.Statistics.LogisticModel` class.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `coefficients` | `Dictionary<string, double>` | Dictionary of model coefficients keyed by term name. Must include an entry with key `(Intercept)`. |

**Throws.**

- `System.ArgumentNullException` — Thrown when `coefficients` is `null`.
- `System.ArgumentException` — Thrown when `coefficients` does not contain the required `(Intercept)` term.

## Properties

### Coefficients

```csharp
public IReadOnlyDictionary<string, double> Coefficients { get; }
```

Gets a copy of the model coefficients.

**Remarks.** The returned dictionary is a copy and can be safely inspected by callers without modifying the model's internal state.

## Methods

### PredictLogit

```csharp
public double PredictLogit(Dictionary<string, double> values)
```

Calculates the linear predictor (logit) for the supplied predictor values.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `values` | `Dictionary<string, double>` | Dictionary of predictor values keyed by predictor name. Each key must exactly match the corresponding non-intercept coefficient name. |

**Returns.** The linear predictor value `η`.

**Throws.**

- `System.ArgumentNullException` — Thrown when `values` is `null`.
- `System.ArgumentException` — Thrown when a required predictor term is missing from `values`.

### PredictProbability

```csharp
public double PredictProbability(Dictionary<string, double> values)
```

Calculates the predicted probability for the supplied predictor values.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `values` | `Dictionary<string, double>` | Dictionary of predictor values keyed by predictor name. Each key must exactly match the corresponding non-intercept coefficient name. |

**Returns.** Predicted probability in the range [0, 1].

**Throws.**

- `System.ArgumentNullException` — Thrown when `values` is `null`.
- `System.ArgumentException` — Thrown when a required predictor term is missing from `values`.

**Remarks.** This method first computes the linear predictor using `JCass_Core.Statistics.LogisticModel.PredictLogit(System.Collections.Generic.Dictionary{System.String,System.Double})`, then applies the inverse-logit transformation:

```csharp p = 1 / (1 + exp(-η))

```
