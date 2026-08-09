using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using JcassDm.Cli;

namespace JcassDm.Bundle;

/// <summary>
/// An open <c>domain_model_setup.xlsx</c>.
///
/// <para>Two responsibilities: hand out <see cref="SheetTable"/>s that read the way the
/// framework reads, and refuse to work on a file that is not a bundle - naming what is
/// wrong with it, because "invalid bundle" sends somebody back to Excel to hunt.</para>
///
/// <para><b>Only the cells a verb writes are touched.</b> Nothing else on the sheet, and
/// nothing at all on the other four sheets, is read-modify-written. A tool that reformats
/// unrelated sheets loses its user's trust the first time they notice, and after that they
/// go back to editing the file by hand.</para>
/// </summary>
internal sealed class BundleFile : IDisposable
{
    private readonly XLWorkbook _workbook;
    private readonly Dictionary<string, SheetTable> _tables = new(StringComparer.Ordinal);
    private readonly List<string> _problems = new();
    private readonly List<string> _warnings = new();

    private BundleFile(string path, XLWorkbook workbook)
    {
        this.Path = path;
        this._workbook = workbook;

        this.SheetNames = workbook.Worksheets.Select(w => w.Name).ToList();
        this.Inspect();
    }

    /// <summary>Absolute path the bundle was opened from.</summary>
    public string Path { get; }

    /// <summary>Every sheet in the workbook, in workbook order.</summary>
    public IReadOnlyList<string> SheetNames { get; }

    /// <summary>
    /// Sheets beyond the five required ones. Not an error - a model author is free to keep
    /// notes in the workbook, and the framework ignores them - but worth showing, because
    /// a sheet called "treatments (old)" is usually a story.
    /// </summary>
    public IReadOnlyList<string> ExtraSheetNames =>
        this.SheetNames.Where(n => SheetSpec.Find(n) is null).ToList();

    /// <summary>Structural faults that make the bundle unusable. Empty means it is well-formed.</summary>
    public IReadOnlyList<string> Problems => this._problems;

    /// <summary>Things worth knowing that do not stop the model running.</summary>
    public IReadOnlyList<string> Warnings => this._warnings;

    /// <summary>
    /// Opens the bundle at <paramref name="path"/>. Fails with a message aimed at somebody
    /// who does not know what a workbook part is.
    /// </summary>
    public static BundleFile Open(string path)
    {
        string full = System.IO.Path.GetFullPath(path);

        if (Directory.Exists(full))
        {
            throw new BundleFailure(
                $"'{full}' is a folder. Give the path to the bundle file itself, " +
                "usually <your model>/domain_model_setup.xlsx.");
        }
        if (!File.Exists(full))
        {
            throw new BundleFailure($"No file at: {full}");
        }

        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(full);
        }
        catch (Exception ex)
        {
            throw new BundleFailure(
                $"Could not read '{full}' as an Excel workbook. " +
                "If it is open in Excel, close it and try again." + Environment.NewLine +
                $"Details: {ex.Message}");
        }

        return new BundleFile(full, workbook);
    }

    /// <summary>
    /// The table for one of the five sheets. Throws when the sheet is absent, so callers
    /// that have already run <see cref="RequireWellFormed"/> can use it without checking.
    /// </summary>
    public SheetTable Sheet(SheetSpec spec)
    {
        if (this._tables.TryGetValue(spec.Name, out SheetTable? cached)) return cached;

        IXLWorksheet? sheet = this._workbook.Worksheets.FirstOrDefault(
            w => string.Equals(w.Name, spec.Name, StringComparison.Ordinal));
        if (sheet is null)
        {
            throw new BundleFailure($"Required sheet '{spec.Name}' not found in {this.Path}");
        }

        var table = new SheetTable(sheet);
        this._tables[spec.Name] = table;
        return table;
    }

    /// <summary>Any sheet by name, for <c>dump --sheet</c>. Null when the workbook has no such sheet.</summary>
    public SheetTable? SheetByName(string sheetName)
    {
        IXLWorksheet? sheet = this._workbook.Worksheets.FirstOrDefault(
            w => string.Equals(w.Name, sheetName, StringComparison.Ordinal));
        if (sheet is null) return null;

        if (this._tables.TryGetValue(sheetName, out SheetTable? cached)) return cached;

        var table = new SheetTable(sheet);
        this._tables[sheetName] = table;
        return table;
    }

    /// <summary>
    /// Fails unless the bundle is structurally sound. Every write verb calls this before
    /// looking at its own arguments, so a broken bundle is reported once, plainly, rather
    /// than as whatever the verb tripped over first.
    /// </summary>
    public void RequireWellFormed()
    {
        if (this._problems.Count == 0) return;

        throw new BundleFailure(
            $"'{this.Path}' is not a usable domain model bundle:" + Environment.NewLine +
            string.Join(Environment.NewLine, this._problems.Select(p => "  - " + p)));
    }

    /// <summary>
    /// Fails when the named sheet cannot safely be appended to. Separate from
    /// <see cref="RequireWellFormed"/> because content stranded below the blank row that
    /// ends the data is invisible to the framework but very much visible to an append,
    /// which would land on top of it.
    /// </summary>
    public void RequireWritable(SheetSpec spec)
    {
        SheetTable table = this.Sheet(spec);
        if (table.StrayContentRow is not int strayRow) return;

        throw new BundleFailure(
            $"Sheet '{spec.Name}' has content at row {strayRow}, below the blank row that ends its data." +
            Environment.NewLine +
            "The framework stops reading at that blank, so those rows are already being ignored - " +
            "and writing a new row here would land on top of them." + Environment.NewLine +
            "Open the sheet in Excel, move the content up to close the gap or delete it, then re-run.");
    }

    /// <summary>
    /// Saves in place, via a temporary file in the same folder.
    ///
    /// <para>The two-step matters: ClosedXML rewrites the whole package on save, so a
    /// crash or a full disk part-way through a direct save would leave a truncated bundle
    /// where a working one used to be. <see cref="File.Replace(string, string, string?)"/>
    /// is the atomic swap.</para>
    /// </summary>
    public void Save()
    {
        string folder = System.IO.Path.GetDirectoryName(this.Path) ?? ".";
        // The temporary name has to end in .xlsx: ClosedXML picks the document type from the
        // extension and refuses to save to anything it does not recognise.
        string temporary = System.IO.Path.Combine(
            folder, $".{System.IO.Path.GetFileNameWithoutExtension(this.Path)}.jcass-dm-{Environment.ProcessId}.tmp.xlsx");

        try
        {
            this._workbook.SaveAs(temporary);
            File.Replace(temporary, this.Path, destinationBackupFileName: null);
        }
        catch (IOException ex)
        {
            throw new BundleFailure(
                $"Could not write '{this.Path}'. If it is open in Excel, close it and try again." +
                Environment.NewLine + $"Details: {ex.Message}");
        }
        finally
        {
            if (File.Exists(temporary))
            {
                try { File.Delete(temporary); } catch (IOException) { /* best effort */ }
            }
        }
    }

    public void Dispose() => this._workbook.Dispose();

    // -----------------------------------------------------------------------------
    // Inspection
    // -----------------------------------------------------------------------------

    private void Inspect()
    {
        foreach (SheetSpec spec in SheetSpec.All)
        {
            if (!this.SheetNames.Contains(spec.Name, StringComparer.Ordinal))
            {
                // Named individually rather than as a set, and with the near-miss called out,
                // because the usual cause is capitalisation or a trailing space in a tab name.
                string? nearMiss = this.SheetNames.FirstOrDefault(
                    n => string.Equals(n.Trim(), spec.Name, StringComparison.OrdinalIgnoreCase));
                this._problems.Add(nearMiss is null
                    ? $"Required sheet '{spec.Name}' is missing."
                    : $"Required sheet '{spec.Name}' is missing. The workbook has a sheet called '{nearMiss}' - " +
                      "sheet names are matched exactly, including case and spaces.");
                continue;
            }

            SheetTable table;
            try
            {
                table = this.Sheet(spec);
            }
            catch (BundleFailure failure)
            {
                this._problems.Add(failure.Message);
                continue;
            }

            IReadOnlyList<string> missing = table.MissingColumns(spec.RequiredColumns);
            if (missing.Count > 0)
            {
                this._problems.Add(
                    $"Sheet '{spec.Name}' is missing column{(missing.Count > 1 ? "s" : "")} " +
                    string.Join(", ", missing.Select(c => $"'{c}'")) + ". " +
                    $"It has: {string.Join(", ", table.Columns)}");
            }

            if (table.StrayContentRow is int strayRow)
            {
                this._warnings.Add(
                    $"Sheet '{spec.Name}': row {strayRow} has content below the blank row that ends the data. " +
                    "The framework stops reading at the blank, so it is being ignored.");
            }

            this.AddWhitespaceWarnings(spec, table);
        }

        if (this.SheetNames.Contains(SheetSpec.Meta.Name, StringComparer.Ordinal)
            && !this._problems.Any(p => p.Contains($"'{SheetSpec.Meta.Name}'", StringComparison.Ordinal)))
        {
            this.AddMetaWarnings();
        }
    }

    private void AddWhitespaceWarnings(SheetSpec spec, SheetTable table)
    {
        if (spec.KeyColumn is not string keyColumn || !table.HasColumn(keyColumn)) return;

        for (int i = 0; i < table.RowCount; i++)
        {
            string key = table.Text(i, keyColumn);
            if (key.Length == key.Trim().Length) continue;

            // This one is worth a line of its own. The name in the sheet is matched against
            // a string constant in C#, and a trailing space makes them different strings
            // while looking identical in every place a person would go to check.
            this._warnings.Add(
                $"Sheet '{spec.Name}' row {table.ExcelRowOf(i)}: {keyColumn} is '{key}' " +
                "with leading or trailing space. It will not match the same name in your C#.");
        }
    }

    private void AddMetaWarnings()
    {
        SheetTable meta = this.Sheet(SheetSpec.Meta);
        if (!meta.HasColumn("Setting") || !meta.HasColumn("Value")) return;

        foreach (string key in MetaKeys.All)
        {
            int row = meta.FindRowByKey("Setting", key);
            if (row < 0)
            {
                this._warnings.Add($"Sheet 'meta' has no '{key}' setting. The model will not load without it.");
            }
            else if (meta.Text(row, "Value").Trim().Length == 0)
            {
                this._warnings.Add($"Sheet 'meta': '{key}' is empty. The model will not load without it.");
            }
        }
    }
}
