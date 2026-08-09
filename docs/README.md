# docs/

Everything an AI assistant needs to know about writing a Juno Cassandra domain model. Plain
markdown, readable by any agent — the Claude skills under `.claude/` are only a shortcut to what
is written here.

**The entry point is `docs/00-start-here.md`.** It does not exist yet.

| Folder | Holds | Written by |
|---|---|---|
| (this folder) | `00-start-here.md`, the single entry point everything else routes from | Session S8 |
| [`orientation/`](orientation/) | What the framework does, what a domain model does, the execution stages, prerequisites | Session S8 |
| [`conventions/`](conventions/) | The rules that fail silently — the four-name rule, where numbers live, what must match what | Session S8 |
| [`workflow/`](workflow/) | Scaffold, build, upload, debug, publish. The walking skeleton, end to end | Session S9 |
| [`patterns/`](patterns/) | The canonical shape of each recurring piece of a real model | Session S10 |
| [`framework/`](framework/) | The generated framework API reference, produced from the assemblies' XML documentation | Session S7 |

Until those land, the working reference is
[`../reference-model/DomainModelSample/README.md`](../reference-model/DomainModelSample/README.md),
which covers the four-name rule, the bundle's five sheets, where thresholds and rates belong, and
the two kinds of zip.
