# scripts/

Maintenance scripts for this repository. These are not part of building a domain model — an
engineer never needs to run them.

| Script | What it does |
|---|---|
| [`leak-scan.ps1`](leak-scan.ps1) | Fails if anything committed here names Juno Cassandra server or admin internals. Runs in CI on every push. |

Session S2 adds `check-framework-version.ps1`.

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
