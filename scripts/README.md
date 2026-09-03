# scripts/

Maintenance scripts for this repository. Most are maintainer-only, but **two of them an engineer
runs**: [`check-framework-version.ps1`](check-framework-version.ps1), when they want to know whether
their framework reference is behind the server, and
[`refresh-model-refs.ps1`](refresh-model-refs.ps1), which is a **required step every time they update
the Assistant** — [`../docs/orientation/updating-the-assistant.md`](../docs/orientation/updating-the-assistant.md).

| Script | What it does |
|---|---|
| [`leak-scan.ps1`](leak-scan.ps1) | Fails if anything committed here names Juno Cassandra server or admin internals. Runs in CI on every push. |
| [`check-framework-version.ps1`](check-framework-version.ps1) | Reports which framework build the assemblies in `refs/` came from, and answers *is this older than what the server runs?* Point it at a model's own `refs/` with `-RefsFolder`. |
| [`refresh-model-refs.ps1`](refresh-model-refs.ps1) | Replaces a model project's own `refs/` folder with the one this Assistant ships. **A step on the update procedure** — nothing else refreshes it, and the staleness is silent. |

## leak-scan.ps1

This repository is public, so everything in it is published permanently. The scanner greps every
file that would be published — tracked files plus untracked files that are not ignored — against a
denylist of server paths, service and account names, reverse-proxy and ACL configuration, database
filenames, admin tooling and other clients' model names.

```powershell
.\scripts\leak-scan.ps1
```

Exit `0` clean, `1` on a hit, `2` if the scan could not run.

**On a hit, the default fix is to change the content.** A hit almost always means a paragraph or a
comment describing how the server works, which does not belong in client-facing material at all.
Suppression is for a genuine collision — a word that is legitimately part of the subject matter and
happens to match a pattern. Put the marker on the same line as the match, with a reason:

```
...the matching line...            # jcass-leak-scan:allow reason goes here
```

Suppressions are printed on every run rather than applied silently, because a suppression nobody
can see is indistinguishable from a leak nobody noticed.

## check-framework-version.ps1

```powershell
.\scripts\check-framework-version.ps1
```

Prints every assembly in `refs/`, whether its `.xml` documentation file is beside it, and the
framework git commit SHA each was built from — read off the assemblies themselves rather than out
of a note, so it cannot report a version the folder does not hold.

Then it answers the question you actually had: **is this older than what the server runs?**

```powershell
.\scripts\check-framework-version.ps1 -ServerVersion 2de6b35
```

Given the framework commit the web app is running, the comparison is exact. Without it the script
falls back to the snapshot's own age — nobody can tell from here whether the server has moved, but
a reference taken several months ago almost certainly is behind, and saying so is more use than
saying nothing. `-StaleAfterDays` (default 90, roughly one release cycle) sets where that starts.

Exit `0` clean, `1` if the folder holds assemblies from more than one framework build (which the
`refs/*.dll` wildcard would compile against all at once), `2` if there is nothing to report on,
`3` if the reference looks older than what the server runs.

Nobody refreshes `refs/` by hand. It ships with the Assistant, so a newer framework arrives with a
newer download — which is also why there is no `populate-refs.ps1` here any more.

## refresh-model-refs.ps1

```powershell
.\scripts\refresh-model-refs.ps1 -Project ..\MyRoadModel
```

**A scaffolded model gets its own copy of `refs/`, and until this existed nothing ever refreshed
it.** `jcass-dm scaffold` copies the Assistant's `refs/` into the new project because the emitted
`.csproj` references `refs\*.dll` relative to itself — which it has to, since the web Debug Model
workspace stages its own framework assemblies into exactly that folder. So an engineer who
re-downloads a newer Assistant gets a newer `refs/` at the root and carries on compiling their model
against the older one, while `docs/framework/` describes the newer. That is the failure the whole
`refs/` staleness regime exists to prevent, reintroduced one level down.

**It replaces rather than tops up, and that is the point.** The `.csproj` reference is a wildcard,
so an assembly left behind from an older release is compiled against alongside its replacement — a
partial copy is worse than no copy at all. The folder is emptied first.

**It copies the `.xml` files and refuses to run without them**, on the same reasoning as CI's *Check
refs folders* step: a `refs/` folder holding only DLLs builds perfectly and quietly costs the
engineer every framework description, in IntelliSense and for the assistant reading them. It also
carries `FRAMEWORK-VERSION.txt` across, so
`check-framework-version.ps1 -RefsFolder ..\MyRoadModel\refs` can report the snapshot's age
afterwards.

**It validates everything before it deletes anything.** A missing source folder, a source missing its
`.xml` files, a `-Project` path that does not exist or holds no `.csproj` — each refuses with the
model untouched, rather than leaving an emptied folder behind. `-Project` is a path the engineer
typed and `..\` resolves against a terminal folder they cannot see, so the `.csproj` requirement is
what stops a typo emptying some other folder's `refs`.

**That includes checking the folder can be emptied at all**, which is the refusal an engineer will
actually meet. Windows will not delete a file another process has open, and VS Code holds a model's
reference assemblies open the whole time the model is loaded; a `Remove-Item` that stops half way
leaves a folder the model cannot build against. So every file in the target is probed first and the
run refuses, untouched, naming what is held.

Exit `0` clean, `1` if the source is missing `.xml` files, the target is held open, or the copy did
not complete, `2` if it could not run at all.

**This is not the only thing that overwrites a model's `refs/`.** The debug sidecar stages its own,
larger set into it during a debug run — roughly ten times as many files, because it also stages NuGet
transitives. Nothing in a model's `refs/` is the engineer's, none of it is ever uploaded, and none of
it is worth preserving.
