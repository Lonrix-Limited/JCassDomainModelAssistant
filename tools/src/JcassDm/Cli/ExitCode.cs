namespace JcassDm.Cli;

/// <summary>
/// Process exit codes.
///
/// <para>These are part of the tool's contract, not an implementation detail: agents
/// branch on them, and a skill that reads "3 means the row is already there with
/// different values, decide whether to overwrite" stops working the moment a code
/// changes meaning. Adding a code is safe; repurposing one is not.</para>
///
/// <para>The split that matters is between <see cref="Conflict"/> - the tool worked,
/// the bundle is fine, and the caller now has a decision to make - and
/// <see cref="ToolFailure"/>, which always means a defect in jcass-dm worth
/// reporting.</para>
/// </summary>
public static class ExitCode
{
    /// <summary>The operation completed. For a write verb this includes "already correct, nothing written".</summary>
    public const int Ok = 0;

    /// <summary>The command line was wrong: unknown verb, unknown option, missing or unparseable value.</summary>
    public const int UsageError = 1;

    /// <summary>The bundle is unusable: file missing, not a workbook, a required sheet or column absent.</summary>
    public const int BundleInvalid = 2;

    /// <summary>The row already exists with different values and <c>--force</c> was not given. Nothing was written.</summary>
    public const int Conflict = 3;

    /// <summary>Unexpected failure inside jcass-dm. Always a defect - report it.</summary>
    public const int ToolFailure = 9;
}
