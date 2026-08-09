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

# LinearRegressionModel

**Namespace:** `JCass_Core.Statistics`  
**Assembly:** `JCass_Core`  
**Kind:** class

> **Should a domain model use this?**  
> Yes, for a fitted linear relationship. Coefficients come from a CSV, never from C#.

Provides functionality to represent a linear regression model with specified coefficients, and an optional piecewise linear model to calculate the standard deviation of the residuals as a function of a predictor variable. The Predict method calculates the predicted value based on the input variables and the coefficients, while the PredictResidualStdDev method calculates the standard deviation of the residuals based on the predictor variable using the piecewise linear model if it is defined.

## Constructors

### LinearRegressionModel

```csharp
public LinearRegressionModel(
    Dictionary<string, double> coefficients,
    string residualStdDevPLMsetup = "",
    bool canExtrapolatePLM = false,
    Random random = null)
```

Initializes a new instance of the LinearRegressionModel with the specified coefficients and optional residual standard deviation model.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `coefficients` | `Dictionary<string, double>` | Dictionary containing regression coefficients. Must include an "(Intercept)" key. |
| 2 | `residualStdDevPLMsetup` | `string` | Optional setup string for piecewise linear model to predict residual standard deviation (format: "x1,y1\|x2,y2\|..."). Empty string means no residual model. |
| 3 | `canExtrapolatePLM` | `bool` | Flag indicating whether the residual standard deviation model can extrapolate beyond defined points |
| 4 | `random` | `Random` | Optional random source for residual sampling. If null, a default instance is used |

> Positional order matters and 4 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

**Throws.**

- `System.ArgumentNullException` — Thrown when coefficients dictionary is null
- `System.ArgumentException` — Thrown when coefficients dictionary does not contain '(Intercept)' key

## Methods

### GetRandomResidual — overload 1 of 2

```csharp
public double GetRandomResidual(double predictorValue)
```

Gets a random residual value based on the predicted standard deviation of the residuals for a given predictor value. This method is useful for Monte Carlo simulations where we want to generate random residuals that reflect the heteroscedasticity captured by the piecewise linear model. Uses the Random instance provided in the constructor.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `predictorValue` | `double` | The predictor value for which to generate a random residual. For example, if residual spread is dependent on rut depth, this should be the rut depth value. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** A random residual value corresponding to the specified predictor value.

### GetRandomResidual — overload 2 of 2

```csharp
public double GetRandomResidual(double predictorValue, Random random)
```

Gets a random residual value based on the predicted standard deviation of the residuals for a given predictor value. This method is useful for Monte Carlo simulations where we want to generate random residuals that reflect the heteroscedasticity captured by the piecewise linear model. Uses the provided Random instance.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `predictorValue` | `double` | The predictor value for which to generate a random residual. For example, if residual spread is dependent on rut depth, this should be the rut depth value. |
| 2 | `random` | `Random` | Random number generator to use for generating the random residual |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** A random residual value corresponding to the specified predictor value.

### Predict

```csharp
public double Predict(Dictionary<string, double> inputVariables)
```

Gets a prediction from the linear regression model based on the provided input variables. The inputVariables dictionary should contain values for all the predictor variables used in the model, with keys matching the variable names in the coefficients dictionary (excluding the '(Intercept)' term). The method calculates the predicted value by summing the intercept and the products of each coefficient with its corresponding input variable value. If any required input variable is missing, an exception is thrown.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `inputVariables` | `Dictionary<string, double>` | Input variables for the prediction. Keys must match the coefficient names in the model (excluding the '(Intercept)' term). |

**Returns.** The predicted value based on the input variables and model coefficients.

**Throws.**

- `System.ArgumentNullException` — Thrown if the inputVariables dictionary is null.
- `System.ArgumentException` — Thrown if any required input variable is missing.

**Remarks.** Performance: O(n) where n is the number of coefficients in the model.

### PredictResidualStdDev

```csharp
public double PredictResidualStdDev(double predictorValue)
```

Predicts the standard deviation of the residuals for a given predictor value using the piecewise linear model. This allows the model to account for heteroscedasticity by providing different residual standard deviation estimates based on the level of the predictor variable. If the piecewise linear model is not defined, an exception is thrown.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `predictorValue` | `double` | The predictor value for which to estimate the residual standard deviation. |

**Returns.** The predicted standard deviation of the residuals corresponding to the specified predictor value.

**Throws.**

- `System.InvalidOperationException` — Thrown if the model does not have a piecewise linear model defined for residual standard deviation.

**Remarks.** Uses the PieceWiseLinearModel.GetValue() method which has O(log n) performance with binary search.

### PredictWithRandomError — overload 1 of 2

```csharp
public double PredictWithRandomError(Dictionary<string, double> inputVariables, double predictorValueForResiduals)
```

Gets a prediction from the linear regression model with an added random error term to simulate the variability in predictions. The random error is drawn from a normal distribution with mean 0 and standard deviation equal to the predicted residual standard deviation for the given predictor value. This method allows for generating more realistic predictions that reflect the uncertainty inherent in the model, especially when used in simulations or Monte Carlo analyses. Uses the Random instance provided in the constructor.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `inputVariables` | `Dictionary<string, double>` | Input variables for the prediction. Keys must match the coefficient names in the model (excluding the '(Intercept)' term). |
| 2 | `predictorValueForResiduals` | `double` | The predictor value for which to generate a random residual. For example, if residual spread is dependent on rut depth, this should be the rut depth value. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** The predicted value with added random error based on the residual standard deviation model.

### PredictWithRandomError — overload 2 of 2

```csharp
public double PredictWithRandomError(Dictionary<string, double> inputVariables, double predictorValueForResiduals, Random random)
```

Gets a prediction from the linear regression model with an added random error term to simulate the variability in predictions. The random error is drawn from a normal distribution with mean 0 and standard deviation equal to the predicted residual standard deviation for the given predictor value. This method allows for generating more realistic predictions that reflect the uncertainty inherent in the model, especially when used in simulations or Monte Carlo analyses. Uses the provided Random instance.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `inputVariables` | `Dictionary<string, double>` | Input variables for the prediction. Keys must match the coefficient names in the model (excluding the '(Intercept)' term). |
| 2 | `predictorValueForResiduals` | `double` | The predictor value for which to generate a random residual. For example, if residual spread is dependent on rut depth, this should be the rut depth value. |
| 3 | `random` | `Random` | Random number generator to use for generating the random error term |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** The predicted value with added random error based on the residual standard deviation model.
