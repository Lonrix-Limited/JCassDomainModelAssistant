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

# StrategySetupInfo

**Namespace:** `JCass_ModelCore.Treatments`  
**Assembly:** `JCass_ModelCore`  
**Kind:** class

> **Should a domain model use this?**  
> Only in models that generate multi-treatment strategies for benefit-cost optimisation.

Class to hold information setup data for a treatment strategy

## Constructors

### StrategySetupInfo

```csharp
public StrategySetupInfo(Dictionary<string, object> setupRow)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `setupRow` | `Dictionary<string, object>` | — |

## Properties

### FirstTreatment

```csharp
public string FirstTreatment { get; set; }
```

*No framework documentation for this member.*

### ForceFirstTreatment

```csharp
public bool ForceFirstTreatment { get; set; }
```

*No framework documentation for this member.*

### StrategyName

```csharp
public string StrategyName { get; set; }
```

*No framework documentation for this member.*

### Treat2Force

```csharp
public bool Treat2Force { get; set; }
```

*No framework documentation for this member.*

### Treat2Name

```csharp
public string Treat2Name { get; set; }
```

*No framework documentation for this member.*

### Treat2WaitPeriod

```csharp
public int Treat2WaitPeriod { get; set; }
```

*No framework documentation for this member.*

### Treat3Force

```csharp
public bool Treat3Force { get; set; }
```

*No framework documentation for this member.*

### Treat3Name

```csharp
public string Treat3Name { get; set; }
```

*No framework documentation for this member.*

### Treat3WaitPeriod

```csharp
public int Treat3WaitPeriod { get; set; }
```

*No framework documentation for this member.*

### Treat4Force

```csharp
public bool Treat4Force { get; set; }
```

*No framework documentation for this member.*

### Treat4Name

```csharp
public string Treat4Name { get; set; }
```

*No framework documentation for this member.*

### Treat4WaitPeriod

```csharp
public int Treat4WaitPeriod { get; set; }
```

*No framework documentation for this member.*
