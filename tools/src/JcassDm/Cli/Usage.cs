using System.IO;

namespace JcassDm.Cli;

/// <summary>
/// The help text. Written for two readers at once: an engineer who has never used a command
/// line, and an agent deciding which verb to call. Both are served by the same thing -
/// saying what each verb does to the file, and what the exit code will be.
/// </summary>
internal static class Usage
{
    /// <summary>Tool version, printed by <c>jcass-dm version</c>.</summary>
    public const string Version = "1.0.0";

    public static void Write(TextWriter output)
    {
        output.WriteLine($"jcass-dm {Version} - read and write a Juno Cassandra domain model bundle.");
        output.WriteLine();
        output.WriteLine("The bundle is domain_model_setup.xlsx, which sits beside your .csproj. It has five");
        output.WriteLine("sheets and every one of them is required: meta, input_headers, parameters,");
        output.WriteLine("treatments, network_functions.");
        output.WriteLine();
        output.WriteLine("COMMANDS");
        output.WriteLine();
        output.WriteLine("  dump <bundle> [--sheet <name>]");
        output.WriteLine("      Print the whole bundle as text. Stable and ordered, so two dumps either side");
        output.WriteLine("      of a change can be compared line by line. Use this to check that the bundle");
        output.WriteLine("      and your C# still agree.");
        output.WriteLine();
        output.WriteLine("  set-meta <bundle> [--main-dll <x>] [--main-class <y>] [--display-name <z>]");
        output.WriteLine("      Set which DLL to load, which class inside it, and the name shown in the web");
        output.WriteLine("      app. At least one is required. All three are written together or not at all.");
        output.WriteLine();
        output.WriteLine("  add-treatment <bundle> --name <x> --budget-category <y>");
        output.WriteLine("                         [--category <c>] [--description <d>] [--comments <m>]");
        output.WriteLine("      Declare a treatment. --category defaults to the treatment name.");
        output.WriteLine();
        output.WriteLine("  add-parameter <bundle> --name <x> [--type number|text] --min <n> --max <n>");
        output.WriteLine("                         [--decimals <d>] [--comment <c>]");
        output.WriteLine("      Declare per-element state carried between periods. --min and --max are");
        output.WriteLine("      required for a numeric parameter: the framework CLAMPS values into that");
        output.WriteLine("      range rather than rejecting them, so a wrong range fails silently.");
        output.WriteLine();
        output.WriteLine("  add-input-header <bundle> --column <x> --type number|text");
        output.WriteLine("                            [--category <c>] [--example <e>] [--comment <m>]");
        output.WriteLine("      Declare a column this model expects in the client's input CSV.");
        output.WriteLine();
        output.WriteLine("  version");
        output.WriteLine();
        output.WriteLine("EVERY WRITE");
        output.WriteLine();
        output.WriteLine("  - is idempotent. Running it twice does not add the row twice.");
        output.WriteLine("  - refuses to overwrite. If the row is there with different values, nothing is");
        output.WriteLine("    written, the differences are printed, and the exit code is 3. Add --force to");
        output.WriteLine("    go ahead.");
        output.WriteLine("  - touches only the cells it was asked to. No other sheet, row or cell is");
        output.WriteLine("    rewritten.");
        output.WriteLine("  - refuses a bundle missing any of the five sheets, naming the missing one.");
        output.WriteLine();
        output.WriteLine("EXIT CODES");
        output.WriteLine();
        output.WriteLine("  0  done, including \"already correct, nothing to write\"");
        output.WriteLine("  1  the command line was wrong - unknown option, missing or unparseable value");
        output.WriteLine("  2  the bundle is unusable - missing file, missing sheet, missing column");
        output.WriteLine("  3  the row exists with different values and --force was not given");
        output.WriteLine("  9  jcass-dm itself failed. A bug in the tool - please report it.");
        output.WriteLine();
        output.WriteLine("EXAMPLES");
        output.WriteLine();
        output.WriteLine("  jcass-dm dump MyRoadModel/domain_model_setup.xlsx");
        output.WriteLine("  jcass-dm dump MyRoadModel/domain_model_setup.xlsx --sheet parameters");
        output.WriteLine("  jcass-dm set-meta MyRoadModel/domain_model_setup.xlsx \\");
        output.WriteLine("      --main-dll MyRoadModel.dll --main-class MyRoadModel");
        output.WriteLine("  jcass-dm add-treatment MyRoadModel/domain_model_setup.xlsx \\");
        output.WriteLine("      --name reseal --budget-category resurfacing");
    }
}
