using System;
using System.Collections.Generic;
using System.IO;
using JcassDm.Bundle;
using JcassDm.Cli;

namespace JcassDm.Verbs;

/// <summary>
/// <c>jcass-dm add-parameter &lt;bundle&gt; --name x [--type number|text] --min n --max n
/// [--decimals d] [--comment c] [--force]</c>
///
/// <para>Parameters are the per-element state that survives from one modelling period to
/// the next.</para>
///
/// <para><b><c>--min</c> and <c>--max</c> are required for a numeric parameter, and that is
/// on purpose.</b> The framework does not reject a value outside them - it CLAMPS to them,
/// in <c>ModelParameterData.SetNumericValue</c>. So a parameter that took a default of 0/0
/// would pin itself to zero for every element for the whole run, with no error, no warning
/// and a forecast that looks like a modelling result. There is no defensible default for a
/// number that behaves like that, so the tool asks.</para>
/// </summary>
internal static class AddParameterVerb
{
    public static int Run(ArgumentSet args, TextWriter output)
    {
        string path = args.BundlePath();

        string name = NameRules.RequireClean(args.Required("--name"), "--name");
        string dataType = NameRules.RequireDataType(args.Optional("--type") ?? DataTypes.Number, "--type");
        bool isNumeric = dataType == DataTypes.Number;

        double? minimum = args.OptionalNumber("--min");
        double? maximum = args.OptionalNumber("--max");
        int? decimals = args.OptionalInteger("--decimals");
        string? comment = args.Optional("--comment");
        bool force = args.Flag("--force");
        args.CheckForUnknownOptions();

        if (isNumeric && (minimum is null || maximum is null))
        {
            throw new UsageFailure(
                "A numeric parameter needs both --min and --max." + Environment.NewLine +
                "These are not validation bounds: the framework CLAMPS every value into this range, " +
                "silently. Leaving them at a default would quietly flatten the parameter across the " +
                "whole run." + Environment.NewLine +
                "Pass the range this quantity can genuinely take, e.g. --min 0 --max 100.");
        }

        if (minimum is double lower && maximum is double upper && lower > upper)
        {
            throw new UsageFailure(
                $"--min {lower} is greater than --max {upper}. Every value would be clamped to {upper}.");
        }

        if (!isNumeric && (minimum is not null || maximum is not null || decimals is not null))
        {
            throw new UsageFailure(
                "--min, --max and --decimals apply to a numeric parameter. " +
                "A text parameter is stored as written.");
        }

        var values = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["parameter_name"] = name,
            ["data_type"] = dataType,
            // Written for a text parameter too: the columns are required, and the framework
            // reads them for every row before it knows the type. It never uses them for text.
            ["minimum"] = minimum ?? 0d,
            ["maximum"] = maximum ?? 0d,
        };
        if (decimals is int places) values["decimals"] = (double)places;
        if (comment is not null) values["comment"] = comment;

        using BundleFile bundle = BundleFile.Open(path);
        bundle.RequireWellFormed();

        RowPlan plan = BundleWriter.Plan(bundle, SheetSpec.Parameters, name, values);
        bool written = BundleWriter.Apply(bundle, new[] { plan }, force, output);

        if (written && plan.IsNew) WriteNextSteps(name, isNumeric, output);
        return ExitCode.Ok;
    }

    private static void WriteNextSteps(string name, bool isNumeric, TextWriter output)
    {
        output.WriteLine();
        output.WriteLine("Two things the bundle cannot do for you:");
        output.WriteLine($"  1. SetParameterValues - write '{name}'. EVERY parameter on this sheet must be");
        output.WriteLine( "                          written there, or setup fails.");
        output.WriteLine($"  2. The element factory - read '{name}' back when rebuilding the element.");

        if (isNumeric && !name.StartsWith("par_", StringComparison.Ordinal))
        {
            output.WriteLine();
            output.WriteLine($"note       numeric parameter names conventionally start with 'par_'; this one is '{name}'.");
            output.WriteLine( "           Convention only - the framework does not care.");
        }
    }
}
