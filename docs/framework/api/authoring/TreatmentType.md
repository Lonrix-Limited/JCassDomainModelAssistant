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

# TreatmentType

**Namespace:** `JCass_ModelCore.Treatments`  
**Assembly:** `JCass_ModelCore`  
**Kind:** class

> **Should a domain model use this?**  
> You read these; you do not create them. They come from the model setup.
>  
> Unit rates were deprecated on `TreatmentType` in May 2026. Supply the unit rate on the `TreatmentInstance` instead, read from `lookups.xlsx`.

*The framework carries no `<summary>` for this type. The signatures below come
from the assembly metadata and are authoritative; the description is not available.*

**Inherits:** `DataObject`  

## Constructors

### TreatmentType — overload 1 of 3

```csharp
public TreatmentType()
```

Parameterless constructor needed to construct object from json

### TreatmentType — overload 2 of 3

```csharp
public TreatmentType(Dictionary<string, object> row)
```

Construct a TreatmentType object from a dictionary row

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `row` | `Dictionary<string, object>` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### TreatmentType — overload 3 of 3

```csharp
public TreatmentType(
    string name,
    string category,
    double unitRate,
    string description = "none")
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `name` | `string` | — |
| 2 | `category` | `string` | — |
| 3 | `unitRate` | `double` | — |
| 4 | `description` | `string` | — |

> Positional order matters and 4 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

## Properties

### BudgetCategory

```csharp
public string BudgetCategory { get; set; }
```

*No framework documentation for this member.*

### Category

```csharp
public string Category { get; set; }
```

*No framework documentation for this member.*

### Description

```csharp
public string Description { get; set; }
```

*No framework documentation for this member.*

### Name

```csharp
public string Name { get; set; }
```

*No framework documentation for this member.*
