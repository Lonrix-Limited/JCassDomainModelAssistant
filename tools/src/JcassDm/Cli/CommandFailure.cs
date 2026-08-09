using System;

namespace JcassDm.Cli;

/// <summary>
/// Base for the failures jcass-dm reports deliberately, as opposed to the ones that
/// escape as a defect. Each subclass carries the exit code it maps to, so the
/// dispatcher in <see cref="Program"/> never has to know which failure came from where.
/// </summary>
public abstract class CommandFailure : Exception
{
    protected CommandFailure(string message, int exitCode) : base(message)
    {
        this.ExitCode = exitCode;
    }

    /// <summary>Exit code this failure maps to. See <see cref="ExitCode"/>.</summary>
    public int ExitCode { get; }
}

/// <summary>The command line was wrong. Exits <see cref="ExitCode.UsageError"/>.</summary>
public sealed class UsageFailure : CommandFailure
{
    public UsageFailure(string message) : base(message, Cli.ExitCode.UsageError) { }
}

/// <summary>The bundle cannot be used as a domain model bundle. Exits <see cref="ExitCode.BundleInvalid"/>.</summary>
public sealed class BundleFailure : CommandFailure
{
    public BundleFailure(string message) : base(message, Cli.ExitCode.BundleInvalid) { }
}

/// <summary>
/// The row exists already with different values and <c>--force</c> was not given.
/// Exits <see cref="ExitCode.Conflict"/>. Nothing has been written when this is thrown -
/// every write verb decides the whole operation before touching the workbook.
/// </summary>
public sealed class ConflictFailure : CommandFailure
{
    public ConflictFailure(string message) : base(message, Cli.ExitCode.Conflict) { }
}
