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

# PieceWiseLinearModel

**Namespace:** `JCass_Core.Statistics`  
**Assembly:** `JCass_Core`  
**Kind:** class

> **Should a domain model use this?**  
> Yes — the usual way to express a relationship a modeller calibrates as a curve.
>  
> Built from a setup code string, so the curve lives in data and not in C#. Changing it should never need a rebuild.

General form of a simple piece-wise linear model. You provide the model with a set of (X,Y) points, and then you call GetValue(X) to get the value of Y for any X. The model does linear interpolation between the closest (X,Y) points. If X is less than the minimim X it returns the Y point associated with the minimum Y. If X is greater than the maximum X, it returns the Y point associated with the maximum X

**Remarks.** This is a refactored, near-duplicate of class 'PieceWiseLinearModelGeneric' in namespace JCass_Core.JFunctions (which was cloned from the now-removed JCass_Functions project so domain models that load the framework DLL still work). New code should use this class. 'PieceWiseLinearModelGeneric' in namespace JCass_Core.JFunctions will be deprecated over time as domain models are migrated to use PieceWiseLinearModel.

## Constructors

### PieceWiseLinearModel — overload 1 of 3

```csharp
public PieceWiseLinearModel()
```

*No framework documentation for this member.*

### PieceWiseLinearModel — overload 2 of 3

```csharp
public PieceWiseLinearModel(string setupString, bool canExtrapolate)
```

Constructs a PieceWiseLinear model from a formatted string containing X,Y coordinate pairs.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `setupString` | `string` | Specifies X and Y points in format x1,y1\|x2,y2\|x3,y3\|etc (e.g., "0,10\|5,20\|10,15") |
| 2 | `canExtrapolate` | `bool` | Flag to indicate if the model can extrapolate values outside the defined points |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### PieceWiseLinearModel — overload 3 of 3

```csharp
public PieceWiseLinearModel(List<double> tXValues, List<double> tYValues, bool canExtrapolate)
```

Constructs a PieceWiseLinear model from separate lists of X and Y values. X values must be sorted in strictly increasing order with no duplicates.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `tXValues` | `List<double>` | X values, must be sorted in increasing order with no duplicates |
| 2 | `tYValues` | `List<double>` | Y values corresponding to each X value |
| 3 | `canExtrapolate` | `bool` | Flag to indicate if the model can extrapolate values outside the defined points |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Throws.**

- `System.Exception` — Thrown when X values contain duplicates or are not properly sorted

## Properties

### CanExtrapolate

```csharp
public bool CanExtrapolate { get; }
```

*No framework documentation for this member.*

## Methods

### GetMaximumValue

```csharp
public double GetMaximumValue()
```

Returns the maximum Y value in the model (Y value corresponding to the maximum X).

**Returns.** The maximum Y value

### GetMaximumX

```csharp
public double GetMaximumX()
```

Returns the maximum X value defined in the model.

**Returns.** The maximum X value

### GetMinimumValue

```csharp
public double GetMinimumValue()
```

Returns the minimum Y value in the model (Y value corresponding to the minimum X).

**Returns.** The minimum Y value

### GetMinimumX

```csharp
public double GetMinimumX()
```

Returns the minimum X value defined in the model.

**Returns.** The minimum X value

### GetValue

```csharp
public double GetValue(double X)
```

Gets the interpolated Y value for a given X value using piecewise linear interpolation. If X is outside the defined range, behavior depends on the CanExtrapolate flag.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `X` | `double` | The X value to interpolate at |

**Returns.** The interpolated or extrapolated Y value

**Remarks.** When CanExtrapolate is false: returns min/max Y for out-of-range X values. When CanExtrapolate is true: extends the line segments beyond the defined range.

### SetupFromXYPairs

```csharp
public void SetupFromXYPairs(string setupString, bool canExtrapolate)
```

Initializes the model from a formatted string containing pipe-delimited X,Y coordinate pairs.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `setupString` | `string` | String in format "x1,y1\|x2,y2\|x3,y3" where each pair is separated by '\|' and coordinates by ',' |
| 2 | `canExtrapolate` | `bool` | Flag to indicate if the model can extrapolate values outside the defined points |

**Throws.**

- `System.Exception` — Thrown when setupString is malformed, contains fewer than 2 points, or X values are not properly sorted/unique
