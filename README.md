# Juno Cassandra — Domain Model Assistant

A self-contained VS Code project that lets a civil engineer, working alongside an AI coding
assistant, design, build, debug, publish and maintain a **Custom Domain Model** for
[Juno Cassandra](https://junocassandra.com) — without being a software developer.

> **Status: under construction.** The skeleton and the reference model are in place. The
> documentation, the `jcass-dm` tool and the skills are not written yet. See
> [What is here today](#what-is-here-today).

---

## What this is for

Juno Cassandra runs a **framework model** — it reads your network data, steps through modelling
periods, asks an optimiser which treatments to fund within budget, and writes the outputs. You never
write any of that.

A **domain model** is the part you do write: the engineering. How an element deteriorates, when work
is triggered, what it costs, and how condition recovers afterwards. Juno Cassandra lets a client's
own modeller write one in C#, debug it in the browser with real breakpoints, and publish it as their
live model.

The obstacle is that the AI assistant helping them knows nothing about the framework. It cannot see
the framework source, it does not know which conventions fail silently, and left to itself it
invents method signatures that do not exist and deterioration rates it has no basis for. **This
repository is the context that makes the assistant useful instead.**

## How to use it

1. Download or clone this repository.
2. Open it in VS Code, alongside your own model folder — use
   [`assistant.code-workspace`](assistant.code-workspace).
3. Point your AI assistant at [`docs/00-start-here.md`](docs/00-start-here.md).

**Your model lives in its own folder, beside this one — never inside it.** That is deliberate, and
it is what makes this repository safe to replace wholesale when a newer version is released:
re-downloading the Assistant never touches your model.

You need a paid AI coding assistant that runs inside your editor and can run commands and read
their output. A browser chat window is not enough. Claude is the recommended and supported choice;
others work but are untested. The subscription is yours, not Lonrix's — and it is an accelerant,
not a requirement. A Custom Domain Model can still be written entirely by hand, exactly as before.

## What is here today

| Folder | Holds | State |
|---|---|---|
| [`docs/`](docs/) | Orientation, conventions, workflow, patterns, framework API reference | Sessions S7–S10 |
| [`reference-model/`](reference-model/) | `DomainModelSample`, a small working model, plus a snapshot of sample inputs | **Present** |
| [`examples/`](examples/) | Focused examples of individual patterns | Session S10 |
| [`tools/`](tools/) | `jcass-dm` — scaffolds, reads and writes the bundle, checks, packages | Sessions S5–S6 |
| [`refs/`](refs/) | Framework reference assemblies to compile against, and their API documentation | **Present** |
| [`scripts/`](scripts/) | Maintenance scripts, including the leak scanner | **Present** |
| [`.claude/`](.claude/) | Claude skills — a convenience layer, never a holder of unique knowledge | Session S11 |

## Getting help

Anything this repository does not cover — a framework call that is not in the API reference, a
failure the docs do not explain, or the docs contradicting what you see on screen — goes to
**support@lonrix.com**. Include what you tried, the exact error, the framework version stamp from
`refs/FRAMEWORK-VERSION.txt`, and your model's name.

## Licence

See [`LICENSE`](LICENSE).
