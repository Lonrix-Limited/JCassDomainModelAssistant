using System;
using System.Globalization;
using System.IO;
using JcassDm.Cli;
using JcassDm.Verbs;

namespace JcassDm;

/// <summary>
/// Entry point and verb dispatch for <c>jcass-dm</c>.
///
/// <para>The tool exists because <c>domain_model_setup.xlsx</c> is a binary file that an AI
/// coding assistant can neither read nor edit. Without it, every bundle change is "ask the
/// engineer to open Excel and follow five steps, three of which involve typing a name that
/// has to match a C# string exactly". With it, a bundle change is a command that either
/// works or says why not.</para>
/// </summary>
public static class Program
{
    /// <summary>Real entry point. Returns the process exit code - see <see cref="ExitCode"/>.</summary>
    public static int Main(string[] args)
    {
        // Pin the culture before anything reads a workbook. ClosedXML formats cell text
        // through the current culture, so an unpinned tool dumps 19.1 as "19,1" on a machine
        // set to a comma decimal separator - and two dumps that should be identical are not.
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
        catch (IOException)
        {
            // Setting the console encoding fails when output is redirected on some hosts.
            // Not worth failing a command over.
        }

        return Run(args, Console.Out, Console.Error);
    }

    /// <summary>
    /// Dispatches one command. Separate from <see cref="Main"/> so the tests drive the real
    /// code path with captured output rather than a stand-in for it.
    /// </summary>
    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        try
        {
            ArgumentSet parsed = ArgumentSet.Parse(args);

            switch (parsed.Verb)
            {
                case "":
                case "help":
                case "--help":
                case "-h":
                case "/?":
                    Usage.Write(output);
                    return ExitCode.Ok;

                case "version":
                case "--version":
                    output.WriteLine($"jcass-dm {Usage.Version}");
                    return ExitCode.Ok;

                case "dump":
                    return DumpVerb.Run(parsed, output);

                case "set-meta":
                    return SetMetaVerb.Run(parsed, output);

                case "add-treatment":
                    return AddTreatmentVerb.Run(parsed, output);

                case "add-parameter":
                    return AddParameterVerb.Run(parsed, output);

                case "add-input-header":
                    return AddInputHeaderVerb.Run(parsed, output);

                case "scaffold":
                    return ScaffoldVerb.Run(parsed, output);

                case "rename":
                    return RenameVerb.Run(parsed, output);

                case "check":
                    return CheckVerb.Run(parsed, output);

                case "package":
                    return PackageVerb.Run(parsed, output);

                default:
                    throw new UsageFailure(
                        $"Unknown command '{parsed.Verb}'." + Environment.NewLine +
                        "Run jcass-dm with no arguments to see the commands it knows.");
            }
        }
        catch (CommandFailure failure)
        {
            error.WriteLine(failure.Message);
            return failure.ExitCode;
        }
        catch (Exception ex)
        {
            // Anything reaching here is a defect in jcass-dm rather than a problem with the
            // bundle or the command line, and the exit code says so, so an agent does not
            // spend the next ten minutes rewording its arguments.
            error.WriteLine("jcass-dm failed unexpectedly. This is a bug in the tool, not in your bundle.");
            error.WriteLine();
            error.WriteLine(ex.ToString());
            return ExitCode.ToolFailure;
        }
    }
}
