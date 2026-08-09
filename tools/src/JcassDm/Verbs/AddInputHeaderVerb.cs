using System;
using System.Collections.Generic;
using System.IO;
using JcassDm.Bundle;
using JcassDm.Cli;

namespace JcassDm.Verbs;

/// <summary>
/// <c>jcass-dm add-input-header &lt;bundle&gt; --column x --type number|text
/// [--category c] [--example e] [--comment m] [--force]</c>
///
/// <para>Declares a column the model expects to find in the client's input CSV.</para>
///
/// <para><c>--category</c> defaults to <c>general</c>. It is an organisational label the
/// framework does not read - only <c>column_name</c> and <c>data_type</c> reach the model -
/// so the default costs nothing and keeps the sheet consistent with what is already there.</para>
/// </summary>
internal static class AddInputHeaderVerb
{
    public static int Run(ArgumentSet args, TextWriter output)
    {
        string path = args.BundlePath();

        string column = NameRules.RequireClean(args.Required("--column", "--name"), "--column");
        string dataType = NameRules.RequireDataType(args.Required("--type"), "--type");
        string? categoryGiven = args.Optional("--category");
        string category = NameRules.RequireClean(categoryGiven ?? "general", "--category");
        string? example = args.Optional("--example");
        string? comment = args.Optional("--comment");
        bool force = args.Flag("--force");
        args.CheckForUnknownOptions();

        var values = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["column_name"] = column,
            ["data_type"] = dataType,
            ["category"] = category,
        };
        if (example is not null)
        {
            // A numeric column's example is written as a number so the sheet looks like one
            // a person filled in, and reads back as a number for anything that inspects it.
            values["example"] = CellValue.ForCell(example, numericIfPossible: dataType == DataTypes.Number);
        }
        if (comment is not null) values["comment"] = comment;

        using BundleFile bundle = BundleFile.Open(path);
        bundle.RequireWellFormed();

        // 'category' and 'example' are the sample's columns rather than the framework's, so a
        // bundle without them is still valid. Drop what the sheet cannot hold, unless the
        // caller asked for it explicitly - then BundleWriter refuses and says which columns exist.
        SheetTable table = bundle.Sheet(SheetSpec.InputHeaders);
        if (!table.HasColumn("category") && categoryGiven is null) values.Remove("category");

        RowPlan plan = BundleWriter.Plan(bundle, SheetSpec.InputHeaders, column, values);
        bool written = BundleWriter.Apply(bundle, new[] { plan }, force, output);

        if (written && plan.IsNew) WriteNextSteps(column, dataType, output);
        return ExitCode.Ok;
    }

    private static void WriteNextSteps(string column, string dataType, TextWriter output)
    {
        string source = dataType == DataTypes.Number ? "numInputs" : "textInputs";

        output.WriteLine();
        output.WriteLine("Two things the bundle cannot do for you:");
        output.WriteLine($"  1. The element factory - read {source}[\"{column}\"] in BOTH factory methods.");
        output.WriteLine( "                           Missing the second is the classic bug: correct in period 0,");
        output.WriteLine( "                           wrong from period 1 on.");
        output.WriteLine( "  2. The element class   - add the matching property.");

        if (dataType == DataTypes.Number)
        {
            output.WriteLine();
            output.WriteLine("note       the framework rejects blanks in a numeric column. If the client's CSV can");
            output.WriteLine("           have gaps here, the CSV needs a sentinel value rather than an empty cell.");
        }
    }
}
