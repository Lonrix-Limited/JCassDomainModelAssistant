# docs/

Everything an AI assistant needs to know about writing a Juno Cassandra domain model. Plain
markdown, readable by any agent. Claude skills will be added under `.claude/` later; they will be a
shortcut to what is written here and will never hold knowledge of their own.

**The entry point is [`00-start-here.md`](00-start-here.md).** Read it first, every session. It
routes; it carries almost no content of its own.

| Folder | Holds | State |
|---|---|---|
| (this folder) | [`00-start-here.md`](00-start-here.md), the single entry point, and [`support-request-template.md`](support-request-template.md) | **Present** |
| [`orientation/`](orientation/) | What you are building, how a run works, prerequisites, the C# you need, reading errors | **Present** |
| [`conventions/`](conventions/) | The rules that fail silently — where numbers live, the four names, when to stop | **Present** |
| [`workflow/`](workflow/) | Scaffold, build, upload, debug, publish. The walking skeleton, end to end | Not written yet |
| [`patterns/`](patterns/) | The canonical shape of each recurring piece of a real model | Not written yet |
| [`framework/`](framework/) | The generated framework API reference and concepts mirror | **Present** — generated, never edited by hand |

Two things outside this folder are part of the same set:

- [`../reference-model/DomainModelSample/README.md`](../reference-model/DomainModelSample/README.md)
  — a complete, small, working model, written to be read.
- `.\tools\jcass-dm.exe --help` — the tool that enforces what these pages describe.
