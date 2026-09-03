# docs/

Everything an AI assistant needs to know about writing a Juno Cassandra domain model. Plain
markdown, readable by any agent. The Claude skills under `.claude/` are a shortcut to what is
written here and hold no knowledge of their own — delete that folder and everything still works,
with more typing.

**The entry point is [`00-start-here.md`](00-start-here.md).** Read it first, every session. It
routes; it carries almost no content of its own.

| Folder | Holds | State |
|---|---|---|
| (this folder) | [`00-start-here.md`](00-start-here.md), the single entry point; [`design-rules.md`](design-rules.md), the rules behind everything here; and [`support-request-template.md`](support-request-template.md) | **Present** |
| [`orientation/`](orientation/) | What you are building, how a run works, prerequisites, running commands, the C# you need, reading errors | **Present** |
| [`conventions/`](conventions/) | The rules that fail silently — where numbers live, the four names, when to stop, what to do with the client's input files | **Present** |
| [`workflow/`](workflow/) | Plan, start from the client's starter model, build, upload, debug, publish, run — end to end | **Present** |
| [`patterns/`](patterns/) | The canonical shape of each recurring piece of a real model, each with a compiling example | **Present** |
| [`framework/`](framework/) | The generated framework API reference and concepts mirror | **Present** — generated, never edited by hand |

Four things outside this folder are part of the same set:

- [`../reference-model/DomainModelSample/README.md`](../reference-model/DomainModelSample/README.md)
  — a complete, small, working model, written to be read.
- [`../examples/ExamplesLibrary/`](../examples/ExamplesLibrary/) — the compiling code behind every
  page in [`patterns/`](patterns/). Built in CI, so it cannot drift from the framework.
- `.\tools\jcass-dm.exe --help` — the tool that enforces what these pages describe.
- [`../model-knowledge/README.md`](../model-knowledge/README.md) — per-model notes, one file per
  domain model, and the **only** place in this repository an assistant writes to. When to read it
  and when to ask for it: [`00-start-here.md`](00-start-here.md) § 4.
