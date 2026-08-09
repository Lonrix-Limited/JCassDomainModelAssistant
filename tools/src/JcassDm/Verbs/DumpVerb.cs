using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JcassDm.Bundle;
using JcassDm.Cli;

namespace JcassDm.Verbs;

/// <summary>
/// <c>jcass-dm dump &lt;bundle&gt; [--sheet &lt;name&gt;]</c> - print the whole bundle as text.
///
/// <para>This is the verb the rest of the tool exists to make possible. The bundle is a
/// binary file: it cannot be read in a pull request, cannot be diffed, and cannot be
/// checked against the C# that has to agree with it. Everything downstream - "does my
/// parameter list match SetParameterValues", "did that add-treatment actually land",
/// "what changed in this commit" - is a dump away rather than a trip to Excel.</para>
///
/// <para><b>The output format is a contract.</b> Two dumps of the same file must be
/// byte-identical, and two dumps either side of one change must differ on exactly the lines
/// that changed. Three consequences, all of which look like sloppiness and are not:</para>
/// <list type="bullet">
///   <item>Columns are NOT padded to align. One long value would re-pad its whole column and
///   turn a one-line change into a whole-sheet diff.</item>
///   <item>Rows are printed in sheet order and never sorted. Row order is data - the
///   framework reads these sheets top to bottom - so sorting would hide a reordering.</item>
///   <item>Numbers are formatted invariantly, so the same file dumps the same on a machine
///   set to a comma decimal separator.</item>
/// </list>
/// </summary>
internal static class DumpVerb
{
    private const string Separator = " | ";

    public static int Run(ArgumentSet args, TextWriter output)
    {
        string path = args.BundlePath();
        string? only = args.Optional("--sheet");
        args.CheckForUnknownOptions();

        using BundleFile bundle = BundleFile.Open(path);

        var sheets = new List<SheetTable>();
        if (only is null)
        {
            // The five in framework order first, then anything else the workbook carries.
            foreach (SheetSpec spec in SheetSpec.All)
            {
                SheetTable? table = bundle.SheetByName(spec.Name);
                if (table is not null) sheets.Add(table);
            }
            foreach (string extra in bundle.ExtraSheetNames)
            {
                SheetTable? table = bundle.SheetByName(extra);
                if (table is not null) sheets.Add(table);
            }
        }
        else
        {
            SheetTable table = bundle.SheetByName(only)
                ?? throw new UsageFailure(
                    $"The bundle has no sheet called '{only}'." + Environment.NewLine +
                    $"It has: {string.Join(", ", bundle.SheetNames)}");
            sheets.Add(table);
        }

        WriteHeader(bundle, output, only);
        foreach (SheetTable table in sheets)
        {
            output.WriteLine();
            WriteSheet(table, output);
        }

        WriteNotes("warnings", bundle.Warnings, output);
        WriteNotes("problems", bundle.Problems, output);

        // A dump of a broken bundle still prints - being able to look at it is exactly what
        // a person diagnosing one needs - but the exit code says it is broken.
        return bundle.Problems.Count > 0 ? ExitCode.BundleInvalid : ExitCode.Ok;
    }

    private static void WriteHeader(BundleFile bundle, TextWriter output, string? only)
    {
        output.WriteLine("# jcass-dm dump");
        output.WriteLine($"# bundle: {bundle.Path}");
        output.WriteLine($"# sheets: {string.Join(", ", bundle.SheetNames)}");
        if (only is not null) output.WriteLine($"# showing: {only}");

        IReadOnlyList<string> extras = bundle.ExtraSheetNames;
        if (extras.Count > 0)
        {
            output.WriteLine($"# not part of the bundle contract, ignored by the framework: {string.Join(", ", extras)}");
        }
    }

    private static void WriteSheet(SheetTable table, TextWriter output)
    {
        SheetSpec? spec = SheetSpec.Find(table.Name);

        output.WriteLine($"[{table.Name}]");
        if (spec is not null) output.WriteLine($"# {spec.Purpose}");

        if (table.Columns.Count == 0)
        {
            output.WriteLine("# no header row - this sheet is empty");
            return;
        }

        output.WriteLine("# row" + Separator + string.Join(Separator, table.Columns));

        if (table.RowCount == 0)
        {
            output.WriteLine("# (header row only, no data rows)");
            return;
        }

        for (int i = 0; i < table.RowCount; i++)
        {
            IEnumerable<string> cells = table.Columns.Select(c => table.Text(i, c));
            output.WriteLine(table.ExcelRowOf(i) + Separator + string.Join(Separator, cells));
        }
    }

    private static void WriteNotes(string label, IReadOnlyList<string> notes, TextWriter output)
    {
        if (notes.Count == 0) return;

        output.WriteLine();
        output.WriteLine($"# {label} ({notes.Count})");
        foreach (string note in notes) output.WriteLine($"#   {note}");
    }
}
