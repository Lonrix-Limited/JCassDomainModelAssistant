using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using JcassDm.Cli;

namespace JcassDm.Bundle;

/// <summary>
/// One sheet of the bundle, read the way the framework reads it.
///
/// <para><b>The reading rules are copied from the framework's Excel facade, deliberately,
/// and must not be "improved".</b> The framework takes row 1 as the header row, reads
/// header cells left to right until the first empty one, and then reads data rows until it
/// meets a row whose FIRST cell is empty. A tool that read further than the framework does
/// would happily report and edit rows the model will never see.</para>
///
/// <para>Rows and columns here are 1-based Excel coordinates, matching ClosedXML, because
/// every message this tool prints names a row the user can find in Excel.</para>
/// </summary>
internal sealed class SheetTable
{
    private const int HeaderRow = 1;
    private const int FirstDataRow = 2;

    private readonly IXLWorksheet _sheet;
    private readonly List<string> _columns = new();
    private readonly Dictionary<string, int> _columnNumbers = new(StringComparer.Ordinal);

    internal SheetTable(IXLWorksheet sheet)
    {
        this._sheet = sheet;
        this.Name = sheet.Name;

        this.ReadHeaders();
        this.RowCount = this.CountDataRows();
        this.StrayContentRow = this.FindStrayContentBelowData();
    }

    /// <summary>Sheet name as spelled in the workbook.</summary>
    public string Name { get; }

    /// <summary>Header names in sheet order.</summary>
    public IReadOnlyList<string> Columns => this._columns;

    /// <summary>Number of data rows the framework would read.</summary>
    public int RowCount { get; private set; }

    /// <summary>
    /// The first row holding content BELOW the blank row that ends the data, or null when
    /// there is none. Content down there is invisible to the framework: it stopped reading
    /// at the blank. Worth reporting, never worth silently deleting.
    /// </summary>
    public int? StrayContentRow { get; }

    /// <summary>True when the sheet has a column of that exact name.</summary>
    public bool HasColumn(string columnName) => this._columnNumbers.ContainsKey(columnName);

    /// <summary>Columns from <paramref name="required"/> that this sheet does not have, in the given order.</summary>
    public IReadOnlyList<string> MissingColumns(IEnumerable<string> required)
        => required.Where(c => !this.HasColumn(c)).ToList();

    /// <summary>The Excel row number of the given 0-based data row.</summary>
    public int ExcelRowOf(int dataRowIndex) => FirstDataRow + dataRowIndex;

    /// <summary>Reads a cell by data row index and column name. Returns null for a blank cell.</summary>
    public object? Value(int dataRowIndex, string columnName)
    {
        if (!this._columnNumbers.TryGetValue(columnName, out int column)) return null;
        return ReadCell(this._sheet.Cell(this.ExcelRowOf(dataRowIndex), column));
    }

    /// <summary>Reads a cell as rendered text. Blank cells come back as an empty string.</summary>
    public string Text(int dataRowIndex, string columnName) => CellValue.Render(this.Value(dataRowIndex, columnName));

    /// <summary>
    /// Finds the 0-based data row whose key column holds <paramref name="key"/>, or -1.
    /// Matching is ordinal and untrimmed, because that is how the framework's dictionaries
    /// match: a treatment stored as "repair " is not the treatment your C# calls "repair".
    /// </summary>
    public int FindRowByKey(string keyColumn, string key)
    {
        for (int i = 0; i < this.RowCount; i++)
        {
            if (string.Equals(this.Text(i, keyColumn), key, StringComparison.Ordinal)) return i;
        }
        return -1;
    }

    /// <summary>
    /// Writes values into an existing data row. Only the named columns are touched; every
    /// other cell on the row, and its formatting, is left exactly as it was.
    /// </summary>
    public void UpdateRow(int dataRowIndex, IReadOnlyDictionary<string, object> values)
    {
        int excelRow = this.ExcelRowOf(dataRowIndex);
        foreach ((string column, object value) in values)
        {
            this.WriteCell(excelRow, column, value);
        }
    }

    /// <summary>
    /// Appends a data row and returns its 0-based index.
    ///
    /// <para>Formatting is copied from the row above so the new row does not stand out as
    /// the one the tool wrote - a bundle that looks hand-edited in places is a bundle
    /// people start "tidying".</para>
    /// </summary>
    public int AppendRow(IReadOnlyDictionary<string, object> values)
    {
        int newIndex = this.RowCount;
        int excelRow = this.ExcelRowOf(newIndex);

        if (newIndex > 0)
        {
            int templateRow = excelRow - 1;
            foreach (int column in this._columnNumbers.Values)
            {
                this._sheet.Cell(excelRow, column).Style = this._sheet.Cell(templateRow, column).Style;
            }
        }

        foreach ((string column, object value) in values)
        {
            this.WriteCell(excelRow, column, value);
        }

        this.RowCount = newIndex + 1;
        return newIndex;
    }

    /// <summary>
    /// Widens columns to fit their content. Called only on a sheet that was actually
    /// written, so an untouched sheet keeps whatever widths its author chose.
    /// </summary>
    public void AutoFitColumns()
    {
        if (this._columns.Count == 0) return;
        this._sheet.Columns(1, this._columns.Count).AdjustToContents();
    }

    // -----------------------------------------------------------------------------
    // Internals
    // -----------------------------------------------------------------------------

    private void WriteCell(int excelRow, string columnName, object value)
    {
        if (!this._columnNumbers.TryGetValue(columnName, out int column))
        {
            // Every caller checks the column exists first; reaching here is a defect.
            throw new InvalidOperationException(
                $"Sheet '{this.Name}' has no column '{columnName}'.");
        }

        IXLCell cell = this._sheet.Cell(excelRow, column);
        switch (value)
        {
            case string text: cell.Value = text; break;
            case double number: cell.Value = number; break;
            case int integer: cell.Value = integer; break;
            case bool flag: cell.Value = flag; break;
            case DateTime date: cell.Value = date; break;
            default: cell.Value = value.ToString() ?? string.Empty; break;
        }
    }

    private void ReadHeaders()
    {
        int column = 1;
        while (true)
        {
            string header = this._sheet.Cell(HeaderRow, column).GetFormattedString();
            if (string.IsNullOrEmpty(header)) break;

            if (this._columnNumbers.ContainsKey(header))
            {
                throw new BundleFailure(
                    $"Sheet '{this.Name}' has two columns called '{header}' " +
                    $"(columns {ColumnLetter(this._columnNumbers[header])} and {ColumnLetter(column)}). " +
                    "The framework reads columns by name and cannot tell them apart.");
            }

            this._columns.Add(header);
            this._columnNumbers[header] = column;
            column++;
        }
    }

    private int CountDataRows()
    {
        if (this._columns.Count == 0) return 0;

        int count = 0;
        while (!string.IsNullOrEmpty(this._sheet.Cell(FirstDataRow + count, 1).GetFormattedString()))
        {
            count++;
        }
        return count;
    }

    private int? FindStrayContentBelowData()
    {
        IXLRow? lastUsed = this._sheet.LastRowUsed();
        if (lastUsed is null) return null;

        int firstBlank = FirstDataRow + this.RowCount;
        for (int row = firstBlank + 1; row <= lastUsed.RowNumber(); row++)
        {
            if (!this._sheet.Row(row).IsEmpty()) return row;
        }
        return null;
    }

    private static object? ReadCell(IXLCell cell)
    {
        // Mirrors the framework's Excel facade: blank is null, and each Excel type comes
        // back as its .NET equivalent rather than as a formatted string.
        XLCellValue value = cell.Value;
        if (value.IsBlank) return null;
        if (value.IsNumber) return value.GetNumber();
        if (value.IsBoolean) return value.GetBoolean();
        if (value.IsDateTime) return value.GetDateTime();
        if (value.IsText) return value.GetText();
        return value.ToString();
    }

    private static string ColumnLetter(int columnNumber) => XLHelper.GetColumnLetterFromNumber(columnNumber);
}
