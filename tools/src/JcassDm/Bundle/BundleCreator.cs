using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;

namespace JcassDm.Bundle;

/// <summary>
/// Writes a new, empty <c>domain_model_setup.xlsx</c>: all five sheets, correct headers, the
/// three <c>meta</c> settings filled in, and no data rows anywhere else.
///
/// <para><b>Empty rather than seeded, deliberately.</b> An example row in <c>input_headers</c>
/// looks helpful and is not: the one nobody deletes becomes a column the client's CSV does not
/// have, and the run fails at setup naming a column the engineer never chose. A sheet with a
/// header and no rows is a state <c>check</c> can honestly report as consistent with an empty
/// model, and there is nothing to remember to remove.</para>
///
/// <para>The column sets come from <see cref="SheetSpec"/> plus the optional columns the
/// framework reads when they are present. They are written here rather than copied from the
/// reference model's bundle so that a scaffolded model carries no trace of somebody else's
/// formatting, comments or stray content.</para>
/// </summary>
internal static class BundleCreator
{
    /// <summary>Columns written for each sheet, in order. A superset of the required ones.</summary>
    private static readonly IReadOnlyDictionary<string, string[]> Columns =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["meta"] = new[] { "Setting", "Value", "comment" },
            ["input_headers"] = new[] { "category", "column_name", "data_type", "example_value", "comment" },
            ["parameters"] = new[] { "parameter_name", "data_type", "minimum", "maximum", "decimals", "comment" },
            ["treatments"] = new[] { "treatment_name", "category", "budget_category", "description", "comments" },
            ["network_functions"] = new[] { "input_parameter", "function_type", "output_parameter", "comment" },
        };

    /// <summary>Comments written beside the three meta settings, so the sheet explains itself.</summary>
    private static readonly IReadOnlyDictionary<string, string> MetaComments =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [MetaKeys.MainDll] = "Compiled file name of this model. Must match the .csproj filename.",
            [MetaKeys.MainClass] = "Entry class inside that DLL. Must match the .csproj filename too.",
            [MetaKeys.ModelName] = "Name shown in the web app. This one is free text.",
        };

    /// <summary>
    /// Creates the bundle at <paramref name="path"/>, overwriting nothing - the caller checks
    /// first, because a bundle silently replaced is somebody's declared model gone.
    /// </summary>
    /// <param name="path">Where to write it.</param>
    /// <param name="mainDll">Value for <c>meta.main_dll</c>, e.g. <c>MyRoadModel.dll</c>.</param>
    /// <param name="mainClass">Value for <c>meta.main_class</c>, e.g. <c>MyRoadModel</c>.</param>
    /// <param name="displayName">Value for <c>meta.model_name</c>, shown in the web app.</param>
    public static void Create(string path, string mainDll, string mainClass, string displayName)
    {
        using var workbook = new XLWorkbook();

        foreach (SheetSpec spec in SheetSpec.All)
        {
            IXLWorksheet sheet = workbook.Worksheets.Add(spec.Name);
            string[] columns = Columns[spec.Name];

            for (int i = 0; i < columns.Length; i++)
            {
                IXLCell cell = sheet.Cell(1, i + 1);
                cell.Value = columns[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }

            if (spec.Name == SheetSpec.Meta.Name)
            {
                WriteMeta(sheet, mainDll, mainClass, displayName);
            }

            sheet.Columns(1, columns.Length).AdjustToContents();
        }

        // Same two-step as BundleFile.Save: ClosedXML writes the whole package, so a failure
        // part-way through a direct save would leave a truncated file where the caller is about
        // to report success.
        string folder = Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".";
        Directory.CreateDirectory(folder);

        string temporary = Path.Combine(folder, $".{Path.GetFileNameWithoutExtension(path)}.jcass-dm-new.tmp.xlsx");
        workbook.SaveAs(temporary);
        File.Move(temporary, path, overwrite: false);
    }

    private static void WriteMeta(IXLWorksheet sheet, string mainDll, string mainClass, string displayName)
    {
        var values = new (string Key, string Value)[]
        {
            (MetaKeys.MainDll, mainDll),
            (MetaKeys.MainClass, mainClass),
            (MetaKeys.ModelName, displayName),
        };

        for (int i = 0; i < values.Length; i++)
        {
            int row = i + 2;
            sheet.Cell(row, 1).Value = values[i].Key;
            sheet.Cell(row, 2).Value = values[i].Value;
            sheet.Cell(row, 3).Value = MetaComments[values[i].Key];
        }
    }
}
