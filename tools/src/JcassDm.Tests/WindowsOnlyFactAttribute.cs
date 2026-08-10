using System;
using System.Runtime.InteropServices;
using Xunit;

namespace JcassDm.Tests;

/// <summary>
/// A <see cref="FactAttribute"/> that skips itself on anything other than Windows.
/// </summary>
/// <remarks>
/// <para>Use this only where the <em>test technique</em> needs Windows, not where the behaviour
/// under test happens to have been written there. There are exactly two at the time of writing,
/// both in <c>RenameTests</c>, and both for the same reason.</para>
///
/// <para>They force a write failure with
/// <c>File.SetAttributes(path, FileAttributes.ReadOnly)</c>. On Windows that makes the file
/// genuinely unwritable. On Linux and macOS it does not: <see cref="System.IO.File.Replace"/>
/// swaps one directory entry for another, and POSIX governs that by the <em>containing
/// directory's</em> write permission, not the file's. The read-only bit is simply not consulted,
/// the replace succeeds, and a test asserting a failed rename fails instead.</para>
///
/// <para>Making the directory unwritable does not rescue it. The rollback these tests exist to
/// prove has to move the <c>.csproj</c> back into that same directory, so it would fail too —
/// for a different reason, at a different point, proving nothing.</para>
///
/// <para><b>What this costs:</b> CI runs on <c>ubuntu-latest</c>, so the rename rollback promise
/// is verified on a developer machine and not in CI. That is a real gap and worth remembering
/// before changing <c>RenameVerb</c>'s backup-and-restore. It is accepted rather than papered
/// over because <c>jcass-dm</c> ships <c>win-x64</c> and only ever runs on an engineer's Windows
/// machine — the read-only semantics being asserted are the ones the tool will actually meet.</para>
/// </remarks>
public sealed class WindowsOnlyFactAttribute : FactAttribute
{
    /// <param name="because">
    /// Why this test needs Windows, in a few words. It is appended to the skip reason so that a
    /// skipped test in a CI log explains itself without anybody opening the source.
    /// </param>
    public WindowsOnlyFactAttribute(string because)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            this.Skip = $"Windows-only test technique: {because}";
        }
    }
}
