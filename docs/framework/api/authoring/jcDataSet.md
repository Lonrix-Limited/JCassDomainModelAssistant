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

*The framework carries no `<summary>` for this type. The signatures below come
from the assembly metadata and are authoritative; the description is not available.*

## Constructors

### jcDataSet — overload 1 of 4

```csharp
public jcDataSet()
```

*No framework documentation for this member.*

### jcDataSet — overload 2 of 4

```csharp
public jcDataSet(List<Dictionary<string, object>> data)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `data` | `List<Dictionary<string, object>>` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### jcDataSet — overload 3 of 4

```csharp
public jcDataSet(System.Data.DataTable data, string key_column = null)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `data` | `System.Data.DataTable` | — |
| 2 | `key_column` | `string` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### jcDataSet — overload 4 of 4

```csharp
public jcDataSet(string jsonString, string rowKey = "none")
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `jsonString` | `string` | — |
| 2 | `rowKey` | `string` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

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
| 1 | `columnName` | `string` | Name of the columns to add |
| 2 | `dataType` | `string` | If 'none' then no action is taken. If 'number' or 'text' then a columnInfo is added automatically |

### AddRow

```csharp
public void AddRow(Dictionary<string, object> row)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `row` | `Dictionary<string, object>` | — |

### CheckRequiredColumns

```csharp
public List<string> CheckRequiredColumns(List<string> requiredColumns, bool throwErrorIfNotFound)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `requiredColumns` | `List<string>` | — |
| 2 | `throwErrorIfNotFound` | `bool` | — |

### GetAsDataTable — overload 1 of 2

```csharp
public System.Data.DataTable GetAsDataTable()
```

*No framework documentation for this member.*

### GetAsDataTable — overload 2 of 2

```csharp
public System.Data.DataTable GetAsDataTable(List<ColumnInfo> columnInfo)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnInfo` | `List<ColumnInfo>` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### GetAsDelimitedString

```csharp
public string GetAsDelimitedString(string rowDelimiter = "~!!~", string columnDelimiter = "~##~")
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `rowDelimiter` | `string` | — |
| 2 | `columnDelimiter` | `string` | — |

### GetAsJsonString

```csharp
public string GetAsJsonString()
```

*No framework documentation for this member.*

### GetAsListOfDictionaries

```csharp
public List<Dictionary<string, object>> GetAsListOfDictionaries()
```

*No framework documentation for this member.*

### GetAsSmartFormattedText

```csharp
public jcDataSet GetAsSmartFormattedText()
```

*No framework documentation for this member.*

### GetClone — overload 1 of 2

```csharp
public jcDataSet GetClone()
```

*No framework documentation for this member.*

### GetClone — overload 2 of 2

```csharp
public jcDataSet GetClone(int nRows)
```

Gets a clone that includes only the top 'nRows' rows

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `nRows` | `int` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### GetDateColumnValueCounts

```csharp
public List<KeyValuePair<DateTime, int>> GetDateColumnValueCounts(string columnName, out int nullCounts)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnName` | `string` | — |
| 2 | `nullCounts` | `int` | — |

### GetDefaultNoDataList

```csharp
public static List<string> GetDefaultNoDataList()
```

Gets a list of 'No Data' values that will be converted to nulls. Detauls no data values assumed are:

"No Data", "-999", "", " ", " "

### GetFilteredSetWithReference

```csharp
public jcDataSet GetFilteredSetWithReference(Dictionary<string, List<string>> filterColumnsAndExcludeValueLists, bool caseInsensitive = true)
```

Returns a subset in which all rows are omitted for which certain columns have values that are in the lists passed as parameters. Only valid for Text columns and values.

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `filterColumnsAndExcludeValueLists` | `Dictionary<string, List<string>>` | Keys are column names, value is a list of values that are NOT allowed (to be ommitted) |
| 2 | `caseInsensitive` | `bool` | If true, then the comparison is NOT case-sensitive |

**Returns.** A DataSet that is referenced to this instance but with some rows removed where needed

### GetKeysAndValuesFromColumn

```csharp
public Dictionary<string, string> GetKeysAndValuesFromColumn(string keyColumnName, string valueColumnName)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `keyColumnName` | `string` | — |
| 2 | `valueColumnName` | `string` | — |

### GetMostFrequentOnDateColumn

```csharp
public Dictionary<string, DateTime> GetMostFrequentOnDateColumn(string columnName, List<string> groupColumns, DateTime defaultValueIfNoData)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnName` | `string` | — |
| 2 | `groupColumns` | `List<string>` | — |
| 3 | `defaultValueIfNoData` | `DateTime` | — |

### GetMostFrequentOnTextColumn

```csharp
public Dictionary<string, string> GetMostFrequentOnTextColumn(string columnName, List<string> groupColumns)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnName` | `string` | — |
| 2 | `groupColumns` | `List<string>` | — |

### GetNewRow

```csharp
public Dictionary<string, object> GetNewRow()
```

*No framework documentation for this member.*

### GetNumber — overload 1 of 2

```csharp
public double GetNumber(Dictionary<string, object> row, string columnName)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `row` | `Dictionary<string, object>` | — |
| 2 | `columnName` | `string` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### GetNumber — overload 2 of 2

```csharp
public double GetNumber(int iRow, string columnName)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iRow` | `int` | — |
| 2 | `columnName` | `string` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### GetRowClone — overload 1 of 2

```csharp
public Dictionary<string, object> GetRowClone(Dictionary<string, object> row)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `row` | `Dictionary<string, object>` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### GetRowClone — overload 2 of 2

```csharp
public Dictionary<string, object> GetRowClone(int iRow)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `iRow` | `int` | — |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

### GetSorted

```csharp
public jcDataSet GetSorted(string sortColumn, bool largestToSmallest = false)
```

Returns a clone (no reference to current object) that is sorted on a specific column's values. Rows in which the sorting column has non-numeric or empty values will be placed at the bottom of the resulting set

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `sortColumn` | `string` | Column on which to sort |
| 2 | `largestToSmallest` | `bool` | Flag to indicate if sort should be Descending |

**Throws.**

- `System.Exception`

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

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `testFraction` | `double` | — |
| 2 | `seed` | `int` | — |

### GetUniqueValuesInColumn

```csharp
public List<string> GetUniqueValuesInColumn(string columnName)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnName` | `string` | — |

### GuessColumnInfos

```csharp
public void GuessColumnInfos()
```

*No framework documentation for this member.*

### LoadFromCSVFile

```csharp
public static jcDataSet LoadFromCSVFile(string filePath)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `filePath` | `string` | — |

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

Gets a specific row by Key value in the Index (only valid if data was read with a Index Key column provided)

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `key` | `string` | Lookup key (index) that determines the row index |

> This member is overloaded. Use named arguments so it is unambiguous which
> overload you are calling.

**Returns.** Dictionary in which the Keys are column names, and Values are the values in each column for this row

### SetupRowKeys

```csharp
public void SetupRowKeys(string columnName)
```

*No framework documentation for this member.*

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `columnName` | `string` | — |

### Transpose

```csharp
public static jcDataSet Transpose(jcDataSet data, string columnThatHoldsHeaders, string rowHeaderName)
```

Transposes a jcDataSet

| # | Parameter | Type | Description |
|---|---|---|---|
| 1 | `data` | `jcDataSet` | Data Set to transpose |
| 2 | `columnThatHoldsHeaders` | `string` | Name of the column that holds the values that will become headers in the transposed set |
| 3 | `rowHeaderName` | `string` | Name to assign to the Row Column in the transposed set |
