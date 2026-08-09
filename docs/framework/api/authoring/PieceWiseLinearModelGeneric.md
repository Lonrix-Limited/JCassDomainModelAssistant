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

A curve defined by a set of (X, Y) points, with straight lines between them. Give it the points, then call `JCass_Core.JFunctions.PieceWiseLinearModelGeneric.GetValue(System.Double)` to read Y for any X.

**Remarks.** The usual way to express a relationship a modeller calibrates by eye or by fitting - a deterioration rate against age, a treatment effect against condition. Keeping it as points in a setup file rather than as an equation in C# is what lets the modeller change the shape without a rebuild.

Outside the range of the supplied points, behaviour depends on `JCass_Core.JFunctions.PieceWiseLinearModelGeneric.CanExtrapolate`, and the default is to extrapolate. With extrapolation on, the first and last line segments are continued indefinitely, so an X far beyond the last point produces a Y far beyond the last point - for a deterioration curve, an unbounded one. With it off, the curve is flat outside the range, holding the first and last Y values.

Extrapolation is the more dangerous default and it is worth choosing deliberately. A condition curve fitted over ages 0 to 40, asked for age 90, will happily return a value no pavement ever had. Pass `canExtrapolate: false` unless the relationship genuinely continues.

X values must be supplied in increasing order; nothing checks that they are.

## Constructors

### PieceWiseLinearModelGeneric — overload 1 of 3

```csharp
public PieceWiseLinearModelGeneric()
```

Creates an empty model. Not usable until `JCass_Core.JFunctions.PieceWiseLinearModelGeneric.SetupFromXYPairs(System.String,System.Boolean)` has been called
- calling `JCass_Core.JFunctions.PieceWiseLinearModelGeneric.GetValue(System.Double)` before then fails.

**Remarks.** Exists so the type can be deserialised. Prefer one of the constructors that builds a usable model.

### PieceWiseLinearModelGeneric — overload 2 of 3

```csharp
public PieceWiseLinearModelGeneric(string setupString, bool canExtrapolate)
```

Builds the curve from a setup string, which is how a curve reaches a model from a lookup or a setup file rather than from code.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `setupString` | `string` | Specifies X and Y points in format x1,y1\|x2,y2\|x3,y3\|etc |
| 2 | `canExtrapolate` | `bool` | True to continue the end segments beyond the supplied range; false to hold the end Y values flat. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Throws.**

- `System.Exception` — Thrown if the string is malformed, has fewer than two pairs, or repeats an X value.

### PieceWiseLinearModelGeneric — overload 3 of 3

```csharp
public PieceWiseLinearModelGeneric(List<double> tXValues, List<double> tYValues, bool canExtrapolate)
```

Constructor for PieceWiseLinear model. It is assumed that Xvalues are sorted in increasing values

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `tXValues` | `List<double>` | XValues, assumed to be in increasing order |
| 2 | `tYValues` | `List<double>` | YValues corresponding to the XValues |
| 3 | `canExtrapolate` | `bool` | True to continue the end segments beyond the supplied range; false to hold the end Y values flat. See the type's remarks - this is worth choosing deliberately. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Throws.**

- `System.Exception` — Thrown if the two lists differ in length, if fewer than two points are supplied, or if an X value is repeated.

## Properties

### CanExtrapolate

```csharp
public bool CanExtrapolate { get; }
```

Whether `JCass_Core.JFunctions.PieceWiseLinearModelGeneric.GetValue(System.Double)` continues the end segments beyond the supplied points (true) or holds the end Y values flat (false).

## Fields

### maxXIndex

```csharp
public int maxXIndex;
```

Index of the point with the highest X value. Set when the model is built.

### minXIndex

```csharp
public int minXIndex;
```

Index of the point with the lowest X value. Set when the model is built.

## Methods

### GetMaximumValue

```csharp
public double GetMaximumValue()
```

The Y value at the highest X - the curve's right-hand end.

**Returns.** The last Y value supplied.

**Throws.**

- `System.Exception` — Thrown if the model has no points.

**Remarks.** Despite the name, this is not the largest Y. It is the Y at the largest X. On a decreasing curve - a condition index falling with age, for instance - it returns the smallest value on the curve. Use it to mean "the end of the curve", never "the peak".

### GetMaximumX

```csharp
public double GetMaximumX()
```

The highest X value the curve was defined over.

**Returns.** The last X value supplied.

**Throws.**

- `System.Exception` — Thrown if the model has no points.

**Remarks.** Useful for deciding whether a lookup is about to leave the fitted range, which matters when `JCass_Core.JFunctions.PieceWiseLinearModelGeneric.CanExtrapolate` is true and the curve will happily keep going.

### GetMinimumValue

```csharp
public double GetMinimumValue()
```

The Y value at the lowest X - the curve's left-hand end.

**Returns.** The first Y value supplied.

**Throws.**

- `System.Exception` — Thrown if the model has no points.

**Remarks.** Despite the name, this is not the smallest Y. It is the Y at the smallest X, which on a decreasing curve is the largest value on it. See `JCass_Core.JFunctions.PieceWiseLinearModelGeneric.GetMaximumValue`.

### GetMinimumX

```csharp
public double GetMinimumX()
```

The lowest X value the curve was defined over.

**Returns.** The first X value supplied.

**Throws.**

- `System.Exception` — Thrown if the model has no points.

### GetValue

```csharp
public double GetValue(double X)
```

Reads Y for a given X, interpolating linearly between the two nearest points.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `X` | `double` | The X value to look up. |

**Returns.** The interpolated Y value.

**Throws.**

- `System.Exception` — Thrown if the model has no points - which is the case if it was created with the parameterless constructor and never set up.

**Remarks.** Outside the supplied range this does not fail. With `JCass_Core.JFunctions.PieceWiseLinearModelGeneric.CanExtrapolate` true, the first and last segments are continued, so a far-out X gives a far-out Y - including negative values from a curve that only ever described positive ones. With it false, the value is held flat at the nearest end.

Neither behaviour is reported. If it matters that a lookup stayed inside the fitted range, test against `JCass_Core.JFunctions.PieceWiseLinearModelGeneric.GetMinimumX` and `JCass_Core.JFunctions.PieceWiseLinearModelGeneric.GetMaximumX` first.

### SetupFromXYPairs

```csharp
public void SetupFromXYPairs(string setupString, bool canExtrapolate)
```

Builds or replaces the curve from a setup string. This is what makes a model created with the parameterless constructor usable.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `setupString` | `string` | X and Y points in the format `x1,y1\|x2,y2\|x3,y3`. X values must increase and must not repeat. |
| 2 | `canExtrapolate` | `bool` | True to continue the end segments beyond the supplied range; false to hold the end Y values flat. |

**Throws.**

- `System.Exception` — Thrown if the string is malformed, has fewer than two pairs, or repeats an X value.

**Remarks.** Calling this on a model that already has points replaces them entirely.
