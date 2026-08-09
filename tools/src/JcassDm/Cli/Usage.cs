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
    public const string Version = "1.1.0";

    public static void Write(TextWriter output)
    {
        output.WriteLine($"jcass-dm {Version} - create, check and package a Juno Cassandra domain model.");
        output.WriteLine();
        output.WriteLine("A domain model is a .csproj, the C# beside it, and domain_model_setup.xlsx - the");
        output.WriteLine("bundle, which has five required sheets: meta, input_headers, parameters,");
        output.WriteLine("treatments, network_functions.");
        output.WriteLine();
        output.WriteLine("THE MODEL");
        output.WriteLine();
        output.WriteLine("  scaffold <Name> [--output <path>] [--element <Noun>] [--namespace <ns>]");
        output.WriteLine("                  [--from-sample]");
        output.WriteLine("      Create a new model. The one name you give becomes the .csproj filename, the");
        output.WriteLine("      assembly name, the entry class and both meta settings - all four, from one");
        output.WriteLine("      name, so they cannot disagree. Emits one stub per modelling stage, with the");
        output.WriteLine("      places a threshold goes marked and left empty. --from-sample carries the");
        output.WriteLine("      reference model's working logic instead, so it runs end to end on day one.");
        output.WriteLine("      --element names the element class, e.g. RoadSegment. That one is not part of");
        output.WriteLine("      the four-name rule and is yours to choose.");
        output.WriteLine();
        output.WriteLine("  rename <NewName> [--project <path>] [--namespace]");
        output.WriteLine("      Change all four names on a model that already exists: .csproj filename,");
        output.WriteLine("      entry class and its file, meta.main_dll, meta.main_class. All of it lands or");
        output.WriteLine("      none of it does - a half-renamed model is worse than the original problem.");
        output.WriteLine("      --namespace moves the namespace too. The namespace is NOT one of the four.");
        output.WriteLine();
        output.WriteLine("  check [--project <path>] [--lookups <path to lookups.xlsx>]");
        output.WriteLine("      Report whether the C#, the bundle and the lookups still agree. Run this");
        output.WriteLine("      FIRST on a model you have inherited - it says what state it is in. It is a");
        output.WriteLine("      local subset: the web app's Check Setup is authoritative.");
        output.WriteLine();
        output.WriteLine("  package [--project <path>] [--output <path>] [--force]");
        output.WriteLine("      Build the upload zip for the web Debug Model page: source only, no refs\\,");
        output.WriteLine("      and opening straight to the .csproj rather than to a folder containing it.");
        output.WriteLine();
        output.WriteLine("THE BUNDLE");
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
        output.WriteLine("THE FOUR NAMES");
        output.WriteLine();
        output.WriteLine("  Four strings must be identical or your model does not load: the .csproj file");
        output.WriteLine("  name, the assembly name it implies, the entry class, and meta.main_dll /");
        output.WriteLine("  meta.main_class in the bundle. A normal run reads the bundle; a debug (F5) run");
        output.WriteLine("  ignores it and derives both names from the .csproj filename. So they agree only");
        output.WriteLine("  when all four match - and when they do not, everything looks fine until F5 says");
        output.WriteLine("  \"Domain Model class 'X' was not found in the specified .dll\".");
        output.WriteLine();
        output.WriteLine("  scaffold and rename are the only things that should ever write those four, and");
        output.WriteLine("  both take ONE name. There is no option that sets one of them on its own.");
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
        output.WriteLine("  4  check found problems with the model. The tool worked; the answer is \"not yet\"");
        output.WriteLine("  9  jcass-dm itself failed. A bug in the tool - please report it.");
        output.WriteLine();
        output.WriteLine("EXAMPLES");
        output.WriteLine();
        output.WriteLine("  jcass-dm scaffold MyRoadModel --output ..\\MyRoadModel --element RoadSegment");
        output.WriteLine("  jcass-dm scaffold MyRoadModel --output ..\\MyRoadModel --from-sample");
        output.WriteLine("  jcass-dm check --project ..\\MyRoadModel");
        output.WriteLine("  jcass-dm rename MyRoadModel --project ..\\InheritedModel --namespace");
        output.WriteLine("  jcass-dm package --project ..\\MyRoadModel");
        output.WriteLine("  jcass-dm dump MyRoadModel/domain_model_setup.xlsx");
        output.WriteLine("  jcass-dm dump MyRoadModel/domain_model_setup.xlsx --sheet parameters");
        output.WriteLine("  jcass-dm set-meta MyRoadModel/domain_model_setup.xlsx \\");
        output.WriteLine("      --main-dll MyRoadModel.dll --main-class MyRoadModel");
        output.WriteLine("  jcass-dm add-treatment MyRoadModel/domain_model_setup.xlsx \\");
        output.WriteLine("      --name reseal --budget-category resurfacing");
    }
}
