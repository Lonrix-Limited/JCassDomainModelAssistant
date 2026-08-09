using System;
using System.Collections.Generic;
using System.IO;
using JcassDm.Bundle;
using JcassDm.Cli;

namespace JcassDm.Verbs;

/// <summary>
/// <c>jcass-dm add-treatment &lt;bundle&gt; --name x --budget-category y
/// [--category c] [--description d] [--comments m] [--force]</c>
///
/// <para>Step 2 of the five-step "add a treatment" procedure. The other four are C# and a
/// lookup row, and this verb deliberately does none of them - it prints them as a reminder
/// instead, because a treatment declared in the bundle and unhandled in <c>Reset</c> is a
/// run that throws, and a treatment with no unit rate is a run that costs nothing.</para>
///
/// <para><c>--category</c> defaults to the treatment name. It is a grouping label the
/// framework carries through to the outputs, so a treatment in a group of its own is the
/// choice that assumes least.</para>
/// </summary>
internal static class AddTreatmentVerb
{
    public static int Run(ArgumentSet args, TextWriter output)
    {
        string path = args.BundlePath();

        string name = NameRules.RequireClean(args.Required("--name"), "--name");
        string budgetCategory = NameRules.RequireClean(args.Required("--budget-category"), "--budget-category");
        string category = NameRules.RequireClean(args.Optional("--category") ?? name, "--category");
        string? description = args.Optional("--description");
        string? comments = args.Optional("--comments", "--comment");
        bool force = args.Flag("--force");
        args.CheckForUnknownOptions();

        using BundleFile bundle = BundleFile.Open(path);
        bundle.RequireWellFormed();

        var values = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["treatment_name"] = name,
            ["category"] = category,
            ["budget_category"] = budgetCategory,
        };
        if (description is not null) values["description"] = description;
        if (comments is not null) values["comments"] = comments;

        RowPlan plan = BundleWriter.Plan(bundle, SheetSpec.Treatments, name, values);
        bool written = BundleWriter.Apply(bundle, new[] { plan }, force, output);

        if (written && plan.IsNew) WriteNextSteps(name, budgetCategory, output);
        return ExitCode.Ok;
    }

    private static void WriteNextSteps(string name, string budgetCategory, TextWriter output)
    {
        output.WriteLine();
        output.WriteLine("The bundle now declares this treatment. Four things it cannot do for you:");
        output.WriteLine($"  1. TreatmentNames.cs      - add a constant whose value is exactly '{name}'.");
        output.WriteLine( "  2. TreatmentTrigger.cs    - decide when it fires and what it costs.");
        output.WriteLine( "  3. StrategyGenerator.cs   - decide whether it competes with the others.");
        output.WriteLine( "  4. The element's Reset    - handle it. The default branch throws, so a treatment");
        output.WriteLine( "                              missing here fails loudly rather than doing nothing.");
        output.WriteLine();
        output.WriteLine($"Also check that '{budgetCategory}' is a column in the client's inputs/budgets.xlsx.");
        output.WriteLine( "A budget category with no column there is never funded, and nothing says so.");
    }
}
