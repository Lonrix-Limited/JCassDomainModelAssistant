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

Reads and writes CSV files as `JCass_Data.Objects.jcDataSet` tables. This is how a domain model loads side-car setup data - fitted coefficients, distribution definitions, per-cohort parameter tables - at setup time.

**Remarks.** Guard every read and name the path in the exception message. None of these methods checks that the file exists before opening it, so a missing or misspelt setup file produces a framework-level file exception rather than one naming what the model was trying to load. Every working model in the corpus wraps its reads:

```csharp string path = Path.Combine(model.Configuration.WorkFolder, "supporting/rut_coefficients.csv"); if (!File.Exists(path)) throw new Exception($"Rut coefficient file not found at: {path}"); jcDataSet coefficients = CSVHelper.ReadDataFromCsvFile(path, keyColumn: "parameter");

```

Build paths from `model.Configuration.WorkFolder` and the `supporting/` folder. That resolves to the same file under an ordinary run and under an in-browser debug run, where a bundle-relative path does not.

Every value read is a string. Nothing is typed on the way in - convert with `jcDataSet.GetNumber`, or call `GuessColumnInfos` to infer types from the data.

## Methods

### ExportEmptySetToCsv

```csharp
public static void ExportEmptySetToCsv(List<string> headers, string filePath)
```

Writes a CSV containing only a header row - a template for a setup file somebody is about to fill in.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `headers` | `List<string>` | Column names, in order. |
| 2 | `filePath` | `string` | Destination file path. |

**Throws.**

- `System.IO.IOException` — Thrown if the file cannot be written.

### ExportToCsv

```csharp
public static void ExportToCsv(List<Dictionary<string, object>> dataList, string filePath)
```

Writes rows to a CSV file, taking the column names and their order from the first row.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `dataList` | `List<Dictionary<string, object>>` | Rows to write. An empty list writes an empty file. |
| 2 | `filePath` | `string` | Destination file path. Overwritten if it exists. |

**Throws.**

- `System.IO.IOException` — Thrown if the file cannot be written.

**Remarks.** Only the first row decides the columns. A later row with an extra key has that value dropped, and one missing a key writes a blank - silently in both cases.

### ExportToCsv_old

```csharp
public static void ExportToCsv_old(List<Dictionary<string, object>> dataList, string filePath)
```

Superseded CSV writer, kept for compatibility. Use `JCass_Data.Utils.CSVHelper.ExportToCsv(System.Collections.Generic.List{System.Collections.Generic.Dictionary{System.String,System.Object}},System.String)`.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `dataList` | `List<Dictionary<string, object>>` | Rows to write. |
| 2 | `filePath` | `string` | Destination file path. |

### GetCSVDataAsync

```csharp
public static Tuple<string[], List<string[]>> GetCSVDataAsync(string filePath)
```

Reads a CSV file as raw headers and raw rows, doing no conversion at all.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `filePath` | `string` | Full path to the CSV file. |

**Returns.** The header row first, then the data rows as string arrays.

**Throws.**

- `System.Exception` — Thrown if the file cannot be read or parsed.

**Remarks.** The name is misleading - this is synchronous and returns a value, not a task. For low-level work where the column-name mapping is not wanted; most callers want `JCass_Data.Utils.CSVHelper.ReadDataFromCsvFile(System.String,System.String)`.

### ReadDataFromCsvFile — overload 1 of 2

```csharp
public static jcDataSet ReadDataFromCsvFile(System.IO.Stream csvStream, string keyColumn = null)
```

Reads CSV content from a stream into a `JCass_Data.Objects.jcDataSet`, for data that does not come from a file on disk.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `csvStream` | `System.IO.Stream` | Stream positioned at the start of the CSV content. |
| 2 | `keyColumn` | `string` | Ignored. See the remarks. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** The loaded data.

**Remarks.** `keyColumn` is accepted and then discarded - unlike the file overload, this one never builds the row key index. Passing a key column here looks like it worked and then every `jcDataSet.Row(string)` lookup throws, whatever the key. Call `SetupRowKeys` on the result yourself.

### ReadDataFromCsvFile — overload 2 of 2

```csharp
public static jcDataSet ReadDataFromCsvFile(string filePath, string keyColumn = null)
```

Reads a CSV file into a `JCass_Data.Objects.jcDataSet`, optionally indexing the rows by a key column so they can be fetched by name. This is the method a domain model normally uses.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `filePath` | `string` | Full path to the CSV file. Check it exists first - see the type's remarks. |
| 2 | `keyColumn` | `string` | Column whose values become row keys, enabling `jcDataSet.Row(string)`. Omit for positional access only. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** The loaded data, or an empty data set if the file has no rows.

**Throws.**

- `System.IO.FileNotFoundException` — Thrown if the file does not exist.
- `System.Exception` — Thrown, naming the column, if `keyColumn` is not among the file's columns.
- `System.ArgumentException` — Thrown if the key column repeats a value - keys must be unique.

**Remarks.** Every value is read as a string. The duplicate-key exception is worth having: a coefficients file listing the same parameter twice is a mistake, and quietly keeping one of the two rows would be worse than stopping.

### ReadListFromCsvFile — overload 1 of 2

```csharp
public static List<Dictionary<string, object>> ReadListFromCsvFile(System.IO.Stream csvStream)
```

Reads CSV content from a stream into a plain list of rows.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `csvStream` | `System.IO.Stream` | Stream positioned at the start of the CSV content. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** One dictionary per data row, keyed by column name. Every value is a string.

### ReadListFromCsvFile — overload 2 of 2

```csharp
public static List<Dictionary<string, object>> ReadListFromCsvFile(string filePath)
```

Reads a CSV file into a plain list of rows, without wrapping it in a `JCass_Data.Objects.jcDataSet`.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `filePath` | `string` | Full path to the CSV file. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** One dictionary per data row, keyed by column name. Every value is a string.

**Throws.**

- `System.IO.FileNotFoundException` — Thrown if the file does not exist.

**Remarks.** Prefer `JCass_Data.Utils.CSVHelper.ReadDataFromCsvFile(System.String,System.String)` unless you specifically want the raw list.
