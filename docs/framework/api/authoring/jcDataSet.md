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

# jcDataSet

**Namespace:** `JCass_Data.Objects`  
**Assembly:** `JCass_Data`  
**Kind:** class

> **Should a domain model use this?**  
> **Yes** — it is what a side-car CSV becomes when you read it at setup.
>  
> The universal shape for tabular setup data. `SetupRowKeys` turns a column into a lookup key so rows can be fetched by name.

A table of data held as a list of rows, where each row is a dictionary of column name to value. This is the framework's universal shape for tabular data.

**Remarks.** A domain model meets this most often at setup: a coefficients CSV read from the project's `supporting/` folder arrives as a `jcDataSet`, and multi-column lookups are held as these too.

Values are stored as `object` and are not typed on the way in. A CSV read gives you strings regardless of what the column looks like; use `JCass_Data.Objects.jcDataSet.GetNumber(System.Int32,System.String)` to convert, or call `JCass_Data.Objects.jcDataSet.GuessColumnInfos` to infer types from the data. Nothing checks that a column holds what its name suggests.

Rows can be addressed by position or by key, but only after `JCass_Data.Objects.jcDataSet.SetupRowKeys(System.String)` has built the key index. That is the usual way to fetch a named row of coefficients - see `JCass_Data.Objects.jcDataSet.Row(System.String)`.

## Constructors

### jcDataSet — overload 1 of 4

```csharp
public jcDataSet()
```

Creates an empty data set. Add columns with `JCass_Data.Objects.jcDataSet.AddColumn(System.String,System.String)`, then rows with `JCass_Data.Objects.jcDataSet.AddRow(System.Collections.Generic.Dictionary{System.String,System.Object})`.

### jcDataSet — overload 2 of 4

```csharp
public jcDataSet(List<Dictionary<string, object>> data)
```

Wraps an existing list of rows. Column names are taken from the first row.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `data` | `List<Dictionary<string, object>>` | Rows, each a dictionary of column name to value. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Remarks.** The list is taken by reference, not copied, and only the first row is inspected for column names. Rows further down with different keys are accepted and will not appear in `JCass_Data.Objects.jcDataSet.Columns`.

### jcDataSet — overload 3 of 4

```csharp
public jcDataSet(System.Data.DataTable data, string key_column = null)
```

Creates a data set from a `System.Data.DataTable`, carrying the column types across and optionally building the row key index.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `data` | `System.Data.DataTable` | Source table. |
| 2 | `key_column` | `string` | Column whose values become row keys. Omit for no key index. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Throws.**

- `System.Exception` — Thrown if a source column is of a type other than double, string or DateTime.

### jcDataSet — overload 4 of 4

```csharp
public jcDataSet(string jsonString, string rowKey = "none")
```

Creates a data set from a JSON array of objects.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `jsonString` | `string` | JSON text: an array of objects with consistent keys. |
| 2 | `rowKey` | `string` | Column to build the row key index from, or "none". |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Remarks.** Every value arrives as a string, whatever its JSON type, and an empty JSON object becomes an empty string. Convert with `JCass_Data.Objects.jcDataSet.GetNumber(System.Int32,System.String)` where you need a number. An empty or null `jsonString` yields an empty set rather than an error.

## Properties

### Count

```csharp
public int Count { get; }
```

Number of Rows in the Set

### SourceFile

```csharp
public string SourceFile { get; set; }
```

Source file (Excel file) from which this dataset was loaded

### SourceSheet

```csharp
public string SourceSheet { get; set; }
```

Source sheet (Excel sheet name) from which this dataset was loaded

## Fields

### ColumnInfo

```csharp
public List<ColumnInfo> ColumnInfo;
```

Optional information about Columns in the set. Key is column name; Value is ColumnInfo for the object. If supplied, this information can be used to convert to a DataTable

### Columns

```csharp
public Dictionary<string, int> Columns;
```

Columns in the set. Key is column name; Value is zero-based index of the column

### Index

```csharp
public Dictionary<string, int> Index;
```

Lookup index, if data was read with a lookup key column provided. Key is the values of the key column, values are the associated row indexes. Ensure that keys are valid strings and are all unique

## Methods

### AddColumn

```csharp
public void AddColumn(string columnName, string dataType = "none")
```

Adds a column to the columns collection, and optionally also a column type definition to the ColumnInfo collection if dataType is not 'none'

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnName` | `string` | Name of the column to add. |
| 2 | `dataType` | `string` | Optional type: "number" / "numeric" / "num", or "text" / "string". Anything else, including the default "none", adds no type information. |

**Throws.**

- `System.ArgumentException` — Thrown if a column of that name already exists.

**Remarks.** An unrecognised `dataType` is not an error - it silently adds no `JCass_Data.Objects.jcDataSet.ColumnInfo` entry, exactly as "none" does. A misspelt type is therefore indistinguishable from omitting it.

### AddRow

```csharp
public void AddRow(Dictionary<string, object> row)
```

Appends a row.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `row` | `Dictionary<string, object>` | The row: column names to values. |

**Remarks.** The row is not validated against `JCass_Data.Objects.jcDataSet.Columns` and it is stored by reference. A row missing a column, or carrying an extra one, is accepted without complaint - and the failure surfaces much later, wherever something reads the column that is not there. Build rows with `JCass_Data.Objects.jcDataSet.GetNewRow` and fill every column.

### CheckRequiredColumns

```csharp
public List<string> CheckRequiredColumns(List<string> requiredColumns, bool throwErrorIfNotFound)
```

Checks that the set contains the columns you expect. Worth calling immediately after loading a setup file, so a renamed or misspelt column is caught at setup rather than mid-run.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `requiredColumns` | `List<string>` | Column names that must be present. |
| 2 | `throwErrorIfNotFound` | `bool` | True to throw on the first missing column; false to return the list of missing ones instead. |

**Returns.** The missing column names, or an empty list if all are present.

**Throws.**

- `System.Exception` — Thrown, naming the column, if `throwErrorIfNotFound` is true and one is missing.

**Remarks.** With `throwErrorIfNotFound` false, the return value is the whole result - ignoring it means the check did nothing.

### GetAsDataTable — overload 1 of 2

```csharp
public System.Data.DataTable GetAsDataTable()
```

Converts the set to a typed `System.Data.DataTable` using this set's own `JCass_Data.Objects.jcDataSet.ColumnInfo`.

**Returns.** The typed table.

**Remarks.** Returns an empty table if `JCass_Data.Objects.jcDataSet.ColumnInfo` was never populated - which is the case for anything loaded from CSV, and for anything produced by `JCass_Data.Objects.jcDataSet.GetClone`. Call `JCass_Data.Objects.jcDataSet.GuessColumnInfos` first, or use the overload that takes the column definitions explicitly.

### GetAsDataTable — overload 2 of 2

```csharp
public System.Data.DataTable GetAsDataTable(List<ColumnInfo> columnInfo)
```

Converts the set to a typed `System.Data.DataTable`, using the column definitions given.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnInfo` | `List<ColumnInfo>` | Column names and their types. Only these columns appear in the result. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** The typed table. Empty text and date values become `DBNull`.

**Throws.**

- `System.Exception` — Thrown if a value cannot be converted, or a column type is not handled.

### GetAsDelimitedString

```csharp
public string GetAsDelimitedString(string rowDelimiter = "~!!~", string columnDelimiter = "~##~")
```

Flattens the whole set, header row included, into a single delimited string for transport or storage.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `rowDelimiter` | `string` | Separator between rows. The default is deliberately unlikely to occur in data. |
| 2 | `columnDelimiter` | `string` | Separator between values within a row. |

**Returns.** The flattened text.

**Throws.**

- `System.Exception` — Thrown if the set has no columns or no rows to flatten.

**Remarks.** Values are written with no escaping, so a delimiter appearing inside a value corrupts the result. Keep the unusual default delimiters unless you know the data cannot contain them.

### GetAsJsonString

```csharp
public string GetAsJsonString()
```

Serialises the whole set to a JSON array of objects.

**Returns.** JSON text.

### GetAsListOfDictionaries

```csharp
public List<Dictionary<string, object>> GetAsListOfDictionaries()
```

Returns the rows as a plain list.

**Returns.** The rows, by reference - changing one changes this data set too.

### GetAsSmartFormattedText

```csharp
public jcDataSet GetAsSmartFormattedText()
```

Returns a copy in which every numeric value has been formatted as display text, choosing a sensible number of decimal places for each. Text values pass through unchanged.

**Returns.** A new data set of formatted strings.

**Remarks.** For display and export. Do not calculate with the result.

### GetClone — overload 1 of 2

```csharp
public jcDataSet GetClone()
```

Returns a copy with its own rows, so changing the copy does not change this set.

**Returns.** The copy.

**Remarks.** `JCass_Data.Objects.jcDataSet.ColumnInfo` is not carried across - the clone has the same columns but no type information. Call `JCass_Data.Objects.jcDataSet.GuessColumnInfos` on the clone if you need it, for instance before `JCass_Data.Objects.jcDataSet.GetAsDataTable`.

### GetClone — overload 2 of 2

```csharp
public jcDataSet GetClone(int nRows)
```

Returns a copy containing only the first `nRows` rows - useful for previewing a large setup file.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `nRows` | `int` | How many rows to keep. More than the set holds keeps all of them. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** The copy.

**Remarks.** `JCass_Data.Objects.jcDataSet.ColumnInfo` is not carried across, as with `JCass_Data.Objects.jcDataSet.GetClone`.

### GetDateColumnValueCounts

```csharp
public List<KeyValuePair<DateTime, int>> GetDateColumnValueCounts(string columnName, out int nullCounts)
```

Counts occurrences of each date in a column, and reports how many values were null.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnName` | `string` | Column to summarise. |
| 2 | `nullCounts` | `int` | Set to the number of null values found. |

**Returns.** Date to occurrence count, most frequent first.

**Throws.**

- `System.Exception` — Thrown, naming the row, if a value cannot be parsed as a date. Dates must be formatted 'dd-MMM-yyyy', for example '13-Apr-2007'.

### GetDefaultNoDataList

```csharp
public static List<string> GetDefaultNoDataList()
```

The values treated as 'no data' and converted to nulls when reading a file: "No Data", "no data", "-999", and the empty, one-space and two-space strings.

**Returns.** The default no-data values.

**Remarks.** `-999` is the framework's invalid-value sentinel and appears here as text. A column that legitimately holds -999 as a measurement will have it read as missing.

### GetFilteredSetWithReference

```csharp
public jcDataSet GetFilteredSetWithReference(Dictionary<string, List<string>> filterColumnsAndExcludeValueLists, bool caseInsensitive = true)
```

Returns a subset in which all rows are omitted for which certain columns have values that are in the lists passed as parameters. Only valid for Text columns and values.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `filterColumnsAndExcludeValueLists` | `Dictionary<string, List<string>>` | Keys are column names, value is a list of values that are NOT allowed (to be ommitted) |
| 2 | `caseInsensitive` | `bool` | If true, the comparison ignores case. |

**Returns.** A data set holding the surviving rows by reference to this instance.

**Remarks.** Two things this shares rather than copies, both implied by "WithReference" in the name and both easy to miss: the surviving rows are the same objects, so editing one edits this set too, and the `JCass_Data.Objects.jcDataSet.Columns` dictionary is the same object, so adding a column to the result adds it here. Use `JCass_Data.Objects.jcDataSet.GetClone` on the result if you intend to modify it.

With `caseInsensitive` true, the exclude lists you pass in are lower-cased in place - the caller's own lists are modified.

### GetKeysAndValuesFromColumn

```csharp
public Dictionary<string, string> GetKeysAndValuesFromColumn(string keyColumnName, string valueColumnName)
```

Turns two columns into a dictionary - a compact way to read a name-to-value mapping out of a setup file.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `keyColumnName` | `string` | Column supplying the keys. Values must be unique. |
| 2 | `valueColumnName` | `string` | Column supplying the values. |

**Returns.** Key to value, both as text.

**Throws.**

- `System.Exception` — Thrown, naming the duplicate, if the key column repeats a value.

### GetMostFrequentOnDateColumn

```csharp
public Dictionary<string, DateTime> GetMostFrequentOnDateColumn(string columnName, List<string> groupColumns, DateTime defaultValueIfNoData)
```

The date counterpart of `JCass_Data.Objects.jcDataSet.GetMostFrequentOnTextColumn(System.String,System.Collections.Generic.List{System.String})`: the most common date in a column, overall and within each grouping.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnName` | `string` | Column whose most frequent date is wanted. Values must already be DateTime. |
| 2 | `groupColumns` | `List<string>` | Columns to group by, cumulative and nested. |
| 3 | `defaultValueIfNoData` | `DateTime` | Returned for any group that had no usable value. |

**Returns.** Group key to the most frequent date, with "all" holding the ungrouped result.

### GetMostFrequentOnTextColumn

```csharp
public Dictionary<string, string> GetMostFrequentOnTextColumn(string columnName, List<string> groupColumns)
```

Finds the most common text value in a column, overall and within each grouping of the columns given.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnName` | `string` | Column whose most frequent value is wanted. |
| 2 | `groupColumns` | `List<string>` | Columns to group by. Groups are cumulative and nested: the first column, then the first two, and so on. |

**Returns.** Group key to the most frequent value, with "all" holding the ungrouped result. Group keys are the grouping column values joined with a pipe.

**Remarks.** Ties are broken by whichever value was seen first. Null values are skipped.

### GetNewRow

```csharp
public Dictionary<string, object> GetNewRow()
```

Returns an empty row ready to be filled and passed to `JCass_Data.Objects.jcDataSet.AddRow(System.Collections.Generic.Dictionary{System.String,System.Object})`.

**Returns.** An empty dictionary.

### GetNumber — overload 1 of 2

```csharp
public double GetNumber(Dictionary<string, object> row, string columnName)
```

Reads a value as a number from a row you already hold.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `row` | `Dictionary<string, object>` | The row. |
| 2 | `columnName` | `string` | Column name. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** The value as a double.

**Throws.**

- `System.Collections.Generic.KeyNotFoundException` — Thrown if the column is not in the row.
- `System.FormatException` — Thrown if the value cannot be read as a number.

### GetNumber — overload 2 of 2

```csharp
public double GetNumber(int iRow, string columnName)
```

Reads a value as a number, converting from however it is stored.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iRow` | `int` | Zero-based row index. |
| 2 | `columnName` | `string` | Column name. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** The value as a double.

**Throws.**

- `System.Collections.Generic.KeyNotFoundException` — Thrown if the column is not in the row.
- `System.FormatException` — Thrown if the value cannot be read as a number.

**Remarks.** Values loaded from CSV or JSON are stored as strings, so this conversion is normal rather than a sign something has gone wrong.

### GetRowClone — overload 1 of 2

```csharp
public Dictionary<string, object> GetRowClone(Dictionary<string, object> row)
```

Returns a copy of a row you already hold.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `row` | `Dictionary<string, object>` | The row to copy. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** A new dictionary holding the same values.

### GetRowClone — overload 2 of 2

```csharp
public Dictionary<string, object> GetRowClone(int iRow)
```

Returns a copy of one row, so it can be changed without affecting the set.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iRow` | `int` | Zero-based row index. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** A new dictionary holding the same values.

### GetSorted

```csharp
public jcDataSet GetSorted(string sortColumn, bool largestToSmallest = false)
```

Returns a clone (no reference to current object) that is sorted on a specific column's values. Rows in which the sorting column has non-numeric or empty values will be placed at the bottom of the resulting set

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `sortColumn` | `string` | Column to sort on. Its values are read as numbers. |
| 2 | `largestToSmallest` | `bool` | True to sort descending. |

**Returns.** A new, sorted data set.

**Throws.**

- `System.Exception` — Thrown, naming the column, if it is not in the set.

**Remarks.** This is a numeric sort only. Non-numeric and empty values are not compared as text - they are pushed to the bottom regardless of direction. Sorting a text column therefore succeeds and returns rows in their original order, which looks like a sort that did nothing rather than one that was never possible.

`JCass_Data.Objects.jcDataSet.ColumnInfo` is not carried across, as with `JCass_Data.Objects.jcDataSet.GetClone`.

### GetTextColumnValueCounts

```csharp
public List<KeyValuePair<string, int>> GetTextColumnValueCounts(string columnName, out int nullCounts)
```

Gets the number of occurrences of a text value from a specific Column, and also returns the number of Null values encountered

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnName` | `string` | Name of the column |
| 2 | `nullCounts` | `int` | Number of null values encountered. Note: null values are defined by the null values list passed in when the dataset is read from Excel |

**Returns.** A List of Key Value pairs in which the Key for each pair is the string value, and the Value is the number of occurrences. List is sorted by decreasing occurences

### GetTrainAndTestSplit

```csharp
public Tuple<jcDataSet, jcDataSet> GetTrainAndTestSplit(double testFraction = 0.2, int seed = 12345)
```

Splits the set into a training set and a test set, for fitting and validating a machine-learning sub-model.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `testFraction` | `double` | Fraction of rows to place in the test set. Must not exceed 0.5. |
| 2 | `seed` | `int` | Random seed, so the same split can be reproduced. |

**Returns.** Training set first, test set second.

**Throws.**

- `System.Exception` — Thrown if `testFraction` is greater than 0.5.

**Remarks.** Both sets hold rows by reference and share this set's `JCass_Data.Objects.jcDataSet.Columns` dictionary, so changing a row in either changes it everywhere. Treat them as read-only.

The sampling was corrected on 2026-08-09 - the last row could previously never be drawn into the test set. A given seed therefore produces a different split from before that date, which matters only if you are reproducing an older fit.

### GetUniqueValuesInColumn

```csharp
public List<string> GetUniqueValuesInColumn(string columnName)
```

Returns the distinct values in a column, as text, in the order first encountered.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnName` | `string` | Column to read. |

**Returns.** The distinct values.

**Throws.**

- `System.Exception` — Thrown, naming the column, if it is not in the set.

### GuessColumnInfos

```csharp
public void GuessColumnInfos()
```

Infers each column's type from its values and rebuilds `JCass_Data.Objects.jcDataSet.ColumnInfo`. A column is numeric only if every non-null value in it parses as a number; otherwise it is text.

**Remarks.** Useful after loading a CSV, where nothing carries type information. It is a guess and it reads the whole set to make it: one stray non-numeric value - a stray unit, an "n/a" - makes the entire column text.

### LoadFromCSVFile

```csharp
public static jcDataSet LoadFromCSVFile(string filePath)
```

Reads a CSV file into a data set, taking column names from the header row.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `filePath` | `string` | Full path to the CSV file. |

**Returns.** The loaded data.

**Remarks.** Check the file exists first and throw with the path in the message if it does not. A setup file that is silently absent surfaces later as a wrong number rather than as a missing file, and by then it looks like a modelling problem.

Every value arrives as text. Convert with `JCass_Data.Objects.jcDataSet.GetNumber(System.Int32,System.String)`, and call `JCass_Data.Objects.jcDataSet.SetupRowKeys(System.String)` if you want to fetch rows by name.

### Row — overload 1 of 2

```csharp
public Dictionary<string, object> Row(int index)
```

Gets a specific row by Index

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `index` | `int` | Zero-based index of the row |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** Dictionary in which the Keys are column names, and Values are the values in each column for this row

### Row — overload 2 of 2

```csharp
public Dictionary<string, object> Row(string key)
```

Gets a row by its key value - the usual way to fetch a named row of setup data, such as the coefficients for one parameter or one material.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `key` | `string` | Key value, from the column passed to `JCass_Data.Objects.jcDataSet.SetupRowKeys(System.String)`. |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** The row: column names to values.

**Throws.**

- `System.Collections.Generic.KeyNotFoundException` — Thrown if the key is not in the index.

**Remarks.** Call `JCass_Data.Objects.jcDataSet.SetupRowKeys(System.String)` first. Without it the index is empty and every lookup throws, whatever the key - which reads as "my data is missing" rather than "I forgot to index it".

### SetupRowKeys

```csharp
public void SetupRowKeys(string columnName)
```

Builds the row key index from a column's values, so rows can be fetched by name with `JCass_Data.Objects.jcDataSet.Row(System.String)`. This is the normal second step after loading a setup CSV.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnName` | `string` | Column whose values become the keys. |

**Throws.**

- `System.Collections.Generic.KeyNotFoundException` — Thrown if the column does not exist.
- `System.ArgumentException` — Thrown if two rows share the same key value.

**Remarks.** Keys must be unique. A duplicate throws, which is the behaviour you want - a coefficients file with the same parameter listed twice is a mistake, and silently keeping one of the two rows would be worse than stopping.

Calling this again replaces the existing index rather than adding to it.

### Transpose

```csharp
public static jcDataSet Transpose(jcDataSet data, string columnThatHoldsHeaders, string rowHeaderName)
```

Transposes a jcDataSet

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `data` | `jcDataSet` | Data Set to transpose |
| 2 | `columnThatHoldsHeaders` | `string` | Name of the column that holds the values that will become headers in the transposed set |
| 3 | `rowHeaderName` | `string` | Name for the column that will hold the original column names. |

**Returns.** The transposed set.

**Throws.**

- `System.ArgumentException` — Thrown if the header column contains duplicate values, since they would become duplicate column names.
