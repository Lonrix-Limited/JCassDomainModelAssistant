# scripts/

Maintenance scripts for this repository. These are not part of building a domain model — an
engineer never needs to run them.

| Script | What it does |
|---|---|
| [`leak-scan.ps1`](leak-scan.ps1) | Fails if anything committed here names Juno Cassandra server or admin internals. Runs in CI on every push. |
| [`check-framework-version.ps1`](check-framework-version.ps1) | Reports which framework build the assemblies in `refs/` came from. The one script here an engineer might actually want. |

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
