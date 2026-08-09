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

# PieceWiseLinearModelGeneric

**Namespace:** `JCass_Core.JFunctions`  
**Assembly:** `JCass_Core`  
**Kind:** class

> **Should a domain model use this?**  
> Yes — the same idea, built from explicit x/y lists rather than a setup code.

General form of a simple piece-wise linear model. You provide the model with a set of (X,Y) points, and then you call GetValue(X) to get the value of Y for any X. The model does linear interpolation between the closest (X,Y) points. If X is less than the minimim X it returns the Y point associated with the minimum Y. If X is greater than the maximum X, it returns the Y point associated with the maximum X

## Constructors

### PieceWiseLinearModelGeneric — overload 1 of 3

```csharp
public PieceWiseLinearModelGeneric()
```

*No framework documentation for this member.*

### PieceWiseLinearModelGeneric — overload 2 of 3

```csharp
public PieceWiseLinearModelGeneric(string setupString, bool canExtrapolate)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `setupString` | `string` | Specifies X and Y points in format x1,y1\|x2,y2\|x3,y3\|etc |
| 2 | `canExtrapolate` | `bool` | Flag to indicate if we can extrapolate outside the points or not |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### PieceWiseLinearModelGeneric — overload 3 of 3

```csharp
public PieceWiseLinearModelGeneric(List<double> tXValues, List<double> tYValues, bool canExtrapolate)
```

Constructor for PieceWiseLinear model. It is assumed that Xvalues are sorted in increasing values

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `tXValues` | `List<double>` | XValues, assumed to be in increasing order |
| 2 | `tYValues` | `List<double>` | YValues corresponding to the XValues |
| 3 | `canExtrapolate` | `bool` | If true, allow extrapolation outside the supplied X-range |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

## Properties

### CanExtrapolate

```csharp
public bool CanExtrapolate { get; }
```

*No framework documentation for this member.*

## Fields

### maxXIndex

```csharp
public int maxXIndex;
```

*No framework documentation for this member.*

### minXIndex

```csharp
public int minXIndex;
```

*No framework documentation for this member.*

## Methods

### GetMaximumValue

```csharp
public double GetMaximumValue()
```

*No framework documentation for this member.*

### GetMaximumX

```csharp
public double GetMaximumX()
```

*No framework documentation for this member.*

### GetMinimumValue

```csharp
public double GetMinimumValue()
```

*No framework documentation for this member.*

### GetMinimumX

```csharp
public double GetMinimumX()
```

*No framework documentation for this member.*

### GetValue

```csharp
public double GetValue(double X)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `X` | `double` | — |

### SetupFromXYPairs

```csharp
public void SetupFromXYPairs(string setupString, bool canExtrapolate)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `setupString` | `string` | — |
| 2 | `canExtrapolate` | `bool` | — |
