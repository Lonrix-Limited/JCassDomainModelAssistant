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

# CSVHelper

**Namespace:** `JCass_Data.Utils`  
**Assembly:** `JCass_Data`  
**Kind:** static class

> **Should a domain model use this?**  
> Yes — this is how you read a `supporting/` CSV in `SetupInstance`.
>  
> Always guard the read and throw with the file path in the message. A missing setup file that is not guarded surfaces later as a wrong number rather than as a missing file.

*The framework carries no `<summary>` for this type. The signatures below come
from the assembly metadata and are authoritative; the description is not available.*

## Methods

### ExportEmptySetToCsv

```csharp
public static void ExportEmptySetToCsv(List<string> headers, string filePath)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `headers` | `List<string>` | — |
| 2 | `filePath` | `string` | — |

### ExportToCsv

```csharp
public static void ExportToCsv(List<Dictionary<string, object>> dataList, string filePath)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `dataList` | `List<Dictionary<string, object>>` | — |
| 2 | `filePath` | `string` | — |

### ExportToCsv_old

```csharp
public static void ExportToCsv_old(List<Dictionary<string, object>> dataList, string filePath)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `dataList` | `List<Dictionary<string, object>>` | — |
| 2 | `filePath` | `string` | — |

### GetCSVDataAsync

```csharp
public static Tuple<string[], List<string[]>> GetCSVDataAsync(string filePath)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `filePath` | `string` | — |

### ReadDataFromCsvFile — overload 1 of 2

```csharp
public static jcDataSet ReadDataFromCsvFile(System.IO.Stream csvStream, string keyColumn = null)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `csvStream` | `System.IO.Stream` | — |
| 2 | `keyColumn` | `string` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### ReadDataFromCsvFile — overload 2 of 2

```csharp
public static jcDataSet ReadDataFromCsvFile(string filePath, string keyColumn = null)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `filePath` | `string` | — |
| 2 | `keyColumn` | `string` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### ReadListFromCsvFile — overload 1 of 2

```csharp
public static List<Dictionary<string, object>> ReadListFromCsvFile(System.IO.Stream csvStream)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `csvStream` | `System.IO.Stream` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### ReadListFromCsvFile — overload 2 of 2

```csharp
public static List<Dictionary<string, object>> ReadListFromCsvFile(string filePath)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `filePath` | `string` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.
