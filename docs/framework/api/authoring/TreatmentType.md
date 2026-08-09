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

The definition of a treatment: its name, what family it belongs to, and which budget category pays for it. One of these per row of the model setup's treatments sheet.

**Remarks.** A domain model reads these, through `model.TreatmentTypes`, keyed by treatment name. It never creates one - they come from the setup data.

Treatment types carry no unit rate. That was removed in May 2026: a rate stored on the type could not vary by element or by condition, and could not be changed without a rebuild. Supply the rate per `TreatmentInstance` instead, read from the project's lookups.

**Inherits:** `DataObject`  

## Constructors

### TreatmentType — overload 1 of 3

```csharp
public TreatmentType()
```

Parameterless constructor, required so the type can be deserialised from JSON. Leaves every property empty.

### TreatmentType — overload 2 of 3

```csharp
public TreatmentType(Dictionary<string, object> row)
```

Creates a treatment type from a row of the setup's treatments sheet. This is how the framework builds them.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `row` | `Dictionary<string, object>` | The row. Must contain `treatment_name`, `description`, `category` and `budget_category`. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Throws.**

- `System.Collections.Generic.KeyNotFoundException` — Thrown if any of those four columns is absent.

### TreatmentType — overload 3 of 3

```csharp
public TreatmentType(
    string name,
    string category,
    double unitRate,
    string description = "none")
```

Creates a treatment type from explicit values.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `name` | `string` | Treatment name. |
| 2 | `category` | `string` | Treatment family. |
| 3 | `unitRate` | `double` | Ignored. See the remarks. |
| 4 | `description` | `string` | Description of the treatment. |

> Positional order matters and 4 arguments is more than anyone reliably remembers.
> Call this with **named arguments** — `quantity: q, unitRate: r` — so a wrong order
> is a compile error rather than a silently wrong model.

**Remarks.** Deprecated, and nothing in the framework calls it. Two reasons not to use it: `unitRate` is accepted and then discarded, because treatment types stopped carrying unit rates in May 2026; and it leaves `JCass_ModelCore.Treatments.TreatmentType.BudgetCategory` empty, which fails the setup check. Treatment types come from the setup data, through the constructor that takes a data row.

## Properties

### BudgetCategory

```csharp
public string BudgetCategory { get; set; }
```

Which budget pays for this treatment by default.

**Remarks.** Must match a column of the run's budget sheet. The framework checks this at setup and names any mismatch, so getting it wrong here fails early and clearly. A treatment can override the split at runtime with `TreatmentInstance.AssignBudgetCategoryFractions` - and those category names are not checked at setup.

### Category

```csharp
public string Category { get; set; }
```

The treatment's family - for example resurfacing, rehabilitation, maintenance. Used for grouping in exports and for rules that apply to a whole class of treatment.

### Description

```csharp
public string Description { get; set; }
```

Free-text description of the treatment, for reports and for whoever reads the setup file.

### Name

```csharp
public string Name { get; set; }
```

Name of the treatment. This is the key domain models use when creating a `TreatmentInstance`, and it must match exactly.
