# refs\

The Juno Cassandra framework assemblies this project compiles against.

**These are committed to the repository, deliberately.** You do not have to fetch anything: clone
or unzip the kit, open it in VS Code or Visual Studio, and the project builds with full IntelliSense
on `DomainModelBase`, `TreatmentInstance` and the rest. That is worth more than a tidy repository —
a starter kit that opens to a screen of red squiggles has failed before anyone has read a line of it.

`FRAMEWORK-VERSION.txt` records which framework build they came from. The `ProductVersion` column
carries the framework's own **git commit SHA**, so two sets can be compared exactly rather than by
date or by guess.

## Refreshing them

The committed DLLs are a snapshot, and snapshots go stale. Refresh when the framework moves:

```powershell
.\scripts\populate-refs.ps1 -Source "<folder-with-the-dlls-or-refs.zip>"
```

Set the location once per machine and you can then run it with no arguments:

```powershell
[Environment]::SetEnvironmentVariable('JCASS_FRAMEWORK_DLLS', '<folder-with-the-dlls>', 'User')
```

Pass `-Clean` when moving between framework versions — it empties the folder first, so an assembly
dropped from a newer release does not linger and get picked up by the wildcard. The script
regenerates `FRAMEWORK-VERSION.txt` on every run, so the stamp cannot drift from what is actually
in the folder.

If you do not have a current DLL set, ask your Juno Cassandra contact for the one matching the
framework build you are expected to run against.

## When it is stale, and why that bites

Building locally against old framework DLLs while the server runs a newer build produces behaviour
that differs between your machine and the real run, with no error to point at it. If a model
behaves differently locally than it does in the web app, compare the SHA in
`FRAMEWORK-VERSION.txt` against the framework build the server is on. That is the first thing to
check, not the last.

## On the web Debug Model page

You need none of this. That workspace stages `refs\` for you when you initialise it, and it stages
considerably more than this folder holds because it includes NuGet transitives.

**Do not include `refs\` in a zip you upload to the Debug Model page.** Its staged set is the
authoritative one for that environment, and overwriting part of it with your local snapshot is
exactly how the two drift apart. See README section 4 for the two kinds of zip and what goes in each.
