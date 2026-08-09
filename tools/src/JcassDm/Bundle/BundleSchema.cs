using System;
using System.Collections.Generic;

namespace JcassDm.Bundle;

/// <summary>
/// What <c>domain_model_setup.xlsx</c> must contain.
///
/// <para>These names mirror the framework's own reader (<c>WebFolderBundleReader</c>) and
/// the setup code that consumes each sheet. They are matched case-sensitively because the
/// framework matches them case-sensitively - a sheet called <c>Treatments</c> is a missing
/// sheet, not a spelling variant, and saying so here is cheaper than finding out at run time.</para>
///
/// <para><b>This is structural only.</b> Locked decision 6: the web app's Check Setup is
/// authoritative for whether a model makes sense. jcass-dm answers "is this a well-formed
/// bundle", never "is this a sensible model".</para>
/// </summary>
public sealed class SheetSpec
{
    private SheetSpec(string name, string[] requiredColumns, string? keyColumn, string purpose)
    {
        this.Name = name;
        this.RequiredColumns = requiredColumns;
        this.KeyColumn = keyColumn;
        this.Purpose = purpose;
    }

    /// <summary>Sheet name, exactly as the framework spells it.</summary>
    public string Name { get; }

    /// <summary>Columns the framework reads by name. A bundle missing one cannot run.</summary>
    public IReadOnlyList<string> RequiredColumns { get; }

    /// <summary>The column that identifies a row, for the add-* verbs. Null where rows have no identity.</summary>
    public string? KeyColumn { get; }

    /// <summary>One line for the dump header, so a reader who has never seen the file knows what they are looking at.</summary>
    public string Purpose { get; }

    // -----------------------------------------------------------------------------
    // The five sheets
    // -----------------------------------------------------------------------------

    public static readonly SheetSpec Meta = new(
        "meta",
        new[] { "Setting", "Value" },
        keyColumn: "Setting",
        "Which DLL and which class to load, plus the display name.");

    public static readonly SheetSpec InputHeaders = new(
        "input_headers",
        new[] { "column_name", "data_type" },
        keyColumn: "column_name",
        "The columns this model expects in the client's input CSV.");

    public static readonly SheetSpec Parameters = new(
        "parameters",
        new[] { "parameter_name", "data_type", "minimum", "maximum" },
        keyColumn: "parameter_name",
        "Per-element state carried from one modelling period to the next.");

    // 'description' is required in practice even though the framework's own column check
    // asks only for the other three: TreatmentType's constructor reads row["description"]
    // unconditionally, so a treatments sheet without that column throws at setup. Requiring
    // it here turns a run-time KeyNotFoundException into a message naming the column.
    public static readonly SheetSpec Treatments = new(
        "treatments",
        new[] { "treatment_name", "category", "budget_category", "description" },
        keyColumn: "treatment_name",
        "The treatments this model can produce, and which budget each is charged to.");

    public static readonly SheetSpec NetworkFunctions = new(
        "network_functions",
        new[] { "input_parameter", "function_type", "output_parameter" },
        keyColumn: null,
        "Framework-computed network statistics. Header row only is normal and valid.");

    /// <summary>
    /// All five, in the order the framework lists them and the order dump prints them.
    /// Fixed order is what lets two dumps be compared line by line.
    /// </summary>
    public static readonly IReadOnlyList<SheetSpec> All = new[]
    {
        Meta, InputHeaders, Parameters, Treatments, NetworkFunctions,
    };

    /// <summary>Finds a spec by sheet name, or null when the name is not one of the five.</summary>
    public static SheetSpec? Find(string sheetName)
    {
        foreach (SheetSpec spec in All)
        {
            if (string.Equals(spec.Name, sheetName, StringComparison.Ordinal)) return spec;
        }
        return null;
    }
}

/// <summary>Settings the framework reads out of the <c>meta</c> sheet.</summary>
public static class MetaKeys
{
    /// <summary>File name of the compiled domain model, including the .dll extension.</summary>
    public const string MainDll = "main_dll";

    /// <summary>Name of the entry class inside that DLL.</summary>
    public const string MainClass = "main_class";

    /// <summary>
    /// Human-readable model name shown in the web app. Note the key is <c>model_name</c>,
    /// not <c>display_name</c> - the command-line option is spelled --display-name because
    /// that is what it does, and --model-name is accepted as an alias for anyone reading
    /// the spreadsheet rather than the help text.
    /// </summary>
    public const string ModelName = "model_name";

    /// <summary>All three, in the order they appear in a well-formed bundle.</summary>
    public static readonly IReadOnlyList<string> All = new[] { MainDll, MainClass, ModelName };
}

/// <summary>Values the framework understands in a <c>data_type</c> column.</summary>
public static class DataTypes
{
    public const string Number = "number";
    public const string Text = "text";

    /// <summary>
    /// The two jcass-dm will write. The framework's enum also carries <c>date</c>, and its
    /// input-header reader treats anything that is not <c>text</c> as numeric - so writing
    /// a typo produces a numeric column rather than an error. That is the silent failure
    /// this restriction exists to prevent.
    /// </summary>
    public static readonly IReadOnlyList<string> Writable = new[] { Number, Text };
}
