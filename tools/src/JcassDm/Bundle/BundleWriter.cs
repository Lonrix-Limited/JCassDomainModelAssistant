using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JcassDm.Cli;

namespace JcassDm.Bundle;

/// <summary>One cell that would change: what is there now, and what was asked for.</summary>
internal sealed record CellChange(string Column, string Current, string Requested);

/// <summary>
/// What a verb intends to do to one row, worked out before anything is written.
/// </summary>
internal sealed class RowPlan
{
    public required SheetSpec Spec { get; init; }

    /// <summary>The row's identifying value, for messages.</summary>
    public required string Key { get; init; }

    /// <summary>0-based index of the existing row, or -1 when the row will be appended.</summary>
    public required int ExistingRowIndex { get; init; }

    /// <summary>Column name to value, including the key column when appending.</summary>
    public required IReadOnlyDictionary<string, object> Values { get; init; }

    /// <summary>Cells that differ from what is already there. Empty for an append.</summary>
    public required IReadOnlyList<CellChange> Changes { get; init; }

    /// <summary>True when the row exists and already holds everything that was asked for.</summary>
    public bool IsAlreadyCorrect => this.ExistingRowIndex >= 0 && this.Changes.Count == 0;

    /// <summary>True when the row does not exist yet.</summary>
    public bool IsNew => this.ExistingRowIndex < 0;
}

/// <summary>
/// The write half of the tool, and the place the two non-negotiable rules live.
///
/// <para><b>Idempotent.</b> Running the same add twice adds one row. An agent will run it
/// twice - after a failed build, after a lost scrollback, after deciding it is not sure
/// whether the first attempt worked - and the second run has to be safe.</para>
///
/// <para><b>Never a silent overwrite.</b> When a row exists with different values the
/// operation stops, prints what differs, and exits <see cref="ExitCode.Conflict"/>.
/// <c>--force</c> is the only way past, and it is deliberately a decision somebody has to
/// take rather than a default.</para>
///
/// <para>Both rules need the whole operation decided before any of it is applied, which is
/// why planning and applying are separate steps here: <c>set-meta</c> touches three rows,
/// and stopping half way through a rename would leave a bundle whose DLL and class names
/// disagree - the exact failure the four-name rule exists to prevent.</para>
/// </summary>
internal static class BundleWriter
{
    /// <summary>
    /// Works out what would happen to the row identified by <paramref name="key"/> without
    /// changing anything.
    /// </summary>
    /// <param name="values">
    /// Column name to value. Must include the sheet's key column. Only these columns are
    /// compared, so a caller that says nothing about <c>comment</c> is not asserting that
    /// <c>comment</c> is empty.
    /// </param>
    public static RowPlan Plan(BundleFile bundle, SheetSpec spec, string key, IReadOnlyDictionary<string, object> values)
    {
        if (spec.KeyColumn is not string keyColumn)
        {
            throw new InvalidOperationException($"Sheet '{spec.Name}' has no key column, so rows cannot be addressed by name.");
        }

        bundle.RequireWritable(spec);
        SheetTable table = bundle.Sheet(spec);

        RequireColumnsExist(table, values.Keys);

        int existing = table.FindRowByKey(keyColumn, key);
        if (existing < 0)
        {
            return new RowPlan
            {
                Spec = spec,
                Key = key,
                ExistingRowIndex = -1,
                Values = values,
                Changes = Array.Empty<CellChange>(),
            };
        }

        var changes = new List<CellChange>();
        foreach (string column in values.Keys)
        {
            object? current = table.Value(existing, column);
            object requested = values[column];
            if (CellValue.Matches(current, requested)) continue;

            changes.Add(new CellChange(column, CellValue.Render(current), CellValue.Render(requested)));
        }

        return new RowPlan
        {
            Spec = spec,
            Key = key,
            ExistingRowIndex = existing,
            Values = values,
            Changes = changes,
        };
    }

    /// <summary>
    /// Applies a set of plans as one operation, or refuses the lot.
    ///
    /// <para>Writes nothing and saves nothing when every plan is already correct, so a
    /// re-run does not change the file's timestamp and show up as a modification in git.</para>
    /// </summary>
    /// <returns>True when the file was written.</returns>
    public static bool Apply(BundleFile bundle, IReadOnlyList<RowPlan> plans, bool force, TextWriter output)
    {
        var conflicted = plans.Where(p => !p.IsNew && p.Changes.Count > 0).ToList();
        if (conflicted.Count > 0 && !force)
        {
            throw new ConflictFailure(DescribeConflicts(conflicted));
        }

        var effective = plans.Where(p => p.IsNew || p.Changes.Count > 0).ToList();
        if (effective.Count == 0)
        {
            foreach (RowPlan plan in plans)
            {
                output.WriteLine($"unchanged  {plan.Spec.Name}: '{plan.Key}' is already exactly as requested.");
            }
            output.WriteLine();
            output.WriteLine("Nothing written.");
            return false;
        }

        foreach (RowPlan plan in effective)
        {
            SheetTable table = bundle.Sheet(plan.Spec);

            if (plan.IsNew)
            {
                int index = table.AppendRow(plan.Values);
                output.WriteLine($"added      {plan.Spec.Name} row {table.ExcelRowOf(index)}: '{plan.Key}'");
            }
            else
            {
                table.UpdateRow(plan.ExistingRowIndex, plan.Values);
                output.WriteLine($"updated    {plan.Spec.Name} row {table.ExcelRowOf(plan.ExistingRowIndex)}: '{plan.Key}'");
                foreach (CellChange change in plan.Changes)
                {
                    output.WriteLine($"             {change.Column}: '{change.Current}' -> '{change.Requested}'");
                }
            }

            table.AutoFitColumns();
        }

        foreach (RowPlan plan in plans.Where(p => p.IsAlreadyCorrect))
        {
            output.WriteLine($"unchanged  {plan.Spec.Name}: '{plan.Key}' is already exactly as requested.");
        }

        bundle.Save();
        output.WriteLine();
        output.WriteLine($"Saved {bundle.Path}");
        return true;
    }

    private static void RequireColumnsExist(SheetTable table, IEnumerable<string> columns)
    {
        var missing = columns.Where(c => !table.HasColumn(c)).ToList();
        if (missing.Count == 0) return;

        // Deliberately a refusal rather than an auto-added column. Adding one would change
        // the shape of a sheet somebody else designed, on the strength of a command-line
        // flag - and if the flag was a typo, the bundle now carries a column nobody wanted
        // and the value the author meant to set is still not set.
        throw new UsageFailure(
            $"Sheet '{table.Name}' has no column{(missing.Count > 1 ? "s" : "")} " +
            string.Join(", ", missing.Select(c => $"'{c}'")) + "." + Environment.NewLine +
            $"Its columns are: {string.Join(", ", table.Columns)}" + Environment.NewLine +
            "jcass-dm does not add columns to a sheet. Add the column in Excel first if you need it.");
    }

    private static string DescribeConflicts(IReadOnlyList<RowPlan> conflicted)
    {
        var lines = new List<string>();
        foreach (RowPlan plan in conflicted)
        {
            lines.Add($"'{plan.Key}' is already in sheet '{plan.Spec.Name}' with different values:");
            lines.Add(string.Empty);

            int columnWidth = Math.Max(6, plan.Changes.Max(c => c.Column.Length));
            int currentWidth = Math.Max(7, plan.Changes.Max(c => c.Current.Length));

            lines.Add($"  {"column".PadRight(columnWidth)}  {"current".PadRight(currentWidth)}  requested");
            lines.Add($"  {new string('-', columnWidth)}  {new string('-', currentWidth)}  {new string('-', 9)}");
            foreach (CellChange change in plan.Changes)
            {
                lines.Add($"  {change.Column.PadRight(columnWidth)}  {change.Current.PadRight(currentWidth)}  {change.Requested}");
            }
            lines.Add(string.Empty);
        }

        lines.Add("Nothing was written.");
        lines.Add("Re-run with --force to overwrite, or use a different name.");
        return string.Join(Environment.NewLine, lines);
    }
}
