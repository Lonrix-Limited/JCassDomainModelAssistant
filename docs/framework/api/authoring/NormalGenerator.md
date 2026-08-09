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

# NormalGenerator

**Namespace:** `JCass_Core.Statistics`  
**Assembly:** `JCass_Core`  
**Kind:** class

> **Should a domain model use this?**  
> Yes, in Monte Carlo models needing normally-distributed draws.
>  
> Seed it from the framework's `Random` (`model.Random`) rather than a new one, or runs stop being reproducible.

Generates normally distributed random numbers using the Marsaglia polar method (a variant of the Box–Muller transform).

**Remarks.** This class produces:
- Standard normal variates Z ~ N(0,1)
- General normal variates X ~ N(mean, sd^2)

Implementation details:
- Uses Marsaglia polar method (no trigonometric functions required).
- Generates two independent normals per iteration and caches one for efficiency.
- Suitable for Monte Carlo simulation.

For reproducible simulations, provide a fixed seed.

## Constructors

### NormalGenerator — overload 1 of 2

```csharp
public NormalGenerator(Random random)
```

Initializes a new instance of the `JCass_Core.Statistics.NormalGenerator` class.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `random` | `Random` | Random source to draw from. If null, a default `System.Random` is used. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### NormalGenerator — overload 2 of 2

```csharp
public NormalGenerator(int? seed = null)
```

Initializes a new instance of the `JCass_Core.Statistics.NormalGenerator` class.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `seed` | `int?` | Optional seed for reproducibility. If null, system time is used. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

## Methods

### NextNormal

```csharp
public double NextNormal(double mean, double sd)
```

Returns a normally distributed random variate X ~ N(mean, sd²).

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `mean` | `double` | The desired mean of the distribution. |
| 2 | `sd` | `double` | The desired standard deviation. Must be non-negative. |

**Returns.** A double drawn from N(mean, sd²).

**Throws.**

- `System.ArgumentOutOfRangeException` — Thrown if `sd` is negative.

### NextStandardNormal

```csharp
public double NextStandardNormal()
```

Returns a standard normal random variate Z ~ N(0,1).

**Returns.** A double drawn from a standard normal distribution with mean 0 and standard deviation 1.

**Remarks.** Uses the Marsaglia polar method. One of each generated pair is cached to improve performance.
