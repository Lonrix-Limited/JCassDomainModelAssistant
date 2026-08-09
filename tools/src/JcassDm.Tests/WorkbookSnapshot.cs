using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ClosedXML.Excel;
using Xunit;

namespace JcassDm.Tests;

/// <summary>
/// Every cell of every sheet, with its value and the formatting that shows on screen.
///
/// <para>This is how "preserve everything you did not touch" is actually checked. Comparing
/// the files byte for byte would not work and would not mean much either: any OpenXML
/// library rewrites the whole package on save, so attribute order shifts and shared-string
/// indexes renumber even when nothing changed. What matters is that no cell anybody can see
/// moved, changed value, or lost its formatting - and that is what this compares.</para>
/// </summary>
internal sealed class WorkbookSnapshot
{
    private readonly Dictionary<string, Dictionary<string, CellSnapshot>> _sheets;

    private WorkbookSnapshot(List<string> sheetNames, Dictionary<string, Dictionary<string, CellSnapshot>> sheets)
    {
        this.SheetNames = sheetNames;
        this._sheets = sheets;
    }

    /// <summary>Sheet names in workbook order.</summary>
    public IReadOnlyList<string> SheetNames { get; }

    public static WorkbookSnapshot Take(string path)
    {
        using var workbook = new XLWorkbook(path);

        var names = new List<string>();
        var sheets = new Dictionary<string, Dictionary<string, CellSnapshot>>(StringComparer.Ordinal);

        foreach (IXLWorksheet sheet in workbook.Worksheets)
        {
            names.Add(sheet.Name);
            var cells = new Dictionary<string, CellSnapshot>(StringComparer.Ordinal);

            IXLRange? used = sheet.RangeUsed();
            if (used is not null)
            {
                foreach (IXLCell cell in used.CellsUsed(XLCellsUsedOptions.All))
                {
                    cells[cell.Address.ToStringRelative()] = CellSnapshot.Of(cell);
                }
            }

            sheets[sheet.Name] = cells;
        }

        return new WorkbookSnapshot(names, sheets);
    }

    /// <summary>
    /// Fails unless at most one sheet differs between the two snapshots. A verb writes one
    /// sheet; everything else must come through untouched, formatting included.
    /// </summary>
    public void AssertOnlyDifferenceIsAppendedRows(WorkbookSnapshot after)
    {
        Assert.Equal(this.SheetNames, after.SheetNames);

        var changed = this.SheetNames.Where(name => !SheetsMatch(this._sheets[name], after._sheets[name])).ToList();

        Assert.True(
            changed.Count <= 1,
            $"Expected at most one sheet to change, but these did: {string.Join(", ", changed)}");
    }

    /// <summary>
    /// Fails unless every cell that existed before is still there, unchanged, on the named
    /// sheet. New cells are allowed - that is the appended row.
    /// </summary>
    public void AssertExistingCellsUnchanged(WorkbookSnapshot after, string sheetName)
    {
        Dictionary<string, CellSnapshot> before = this._sheets[sheetName];
        Dictionary<string, CellSnapshot> now = after._sheets[sheetName];

        foreach ((string address, CellSnapshot original) in before)
        {
            Assert.True(now.ContainsKey(address), $"{sheetName}!{address} disappeared.");
            Assert.True(
                original == now[address],
                $"{sheetName}!{address} changed: {original} -> {now[address]}");
        }
    }

    private static bool SheetsMatch(Dictionary<string, CellSnapshot> left, Dictionary<string, CellSnapshot> right)
    {
        if (left.Count != right.Count) return false;
        foreach ((string address, CellSnapshot cell) in left)
        {
            if (!right.TryGetValue(address, out CellSnapshot? other) || cell != other) return false;
        }
        return true;
    }
}

/// <summary>One cell's value and the formatting a reader would notice.</summary>
internal sealed record CellSnapshot(string Value, string Formatting)
{
    public static CellSnapshot Of(IXLCell cell)
        => new(cell.GetFormattedString(), DescribeFormatting(cell.Style));

    private static string DescribeFormatting(IXLStyle style)
    {
        string fill;
        try
        {
            fill = style.Fill.PatternType == XLFillPatternValues.None
                ? "none"
                : style.Fill.BackgroundColor.ToString();
        }
        catch (Exception)
        {
            // Theme and indexed colours can refuse to resolve. Not what this test is about.
            fill = "unresolved";
        }

        return string.Join('|',
            style.Font.Bold.ToString(),
            style.Font.FontName,
            style.Font.FontSize.ToString(CultureInfo.InvariantCulture),
            fill,
            style.Border.TopBorder.ToString(),
            style.Border.LeftBorder.ToString(),
            style.NumberFormat.Format,
            style.NumberFormat.NumberFormatId.ToString(CultureInfo.InvariantCulture),
            style.Alignment.Horizontal.ToString());
    }

    public override string ToString() => $"'{this.Value}' [{this.Formatting}]";
}
