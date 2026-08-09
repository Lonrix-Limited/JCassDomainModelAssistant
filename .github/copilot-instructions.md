# Copilot instructions

This repository is the **Juno Cassandra Domain Model Assistant**. It gives an AI coding assistant
the context it does not otherwise have: how the Juno Cassandra framework expects a **domain model**
to be written, which of its conventions fail silently, and the tooling to check your work.

**Read [`docs/00-start-here.md`](docs/00-start-here.md) before doing anything else.** It is the
single entry point and it routes you from there.

Two things to know before you read it, because they change what you should do:

- **The engineer's model is a sibling folder, never inside this repository.** You edit their model;
  you do not edit the Assistant.
- **You do the plumbing. You do not supply engineering judgement.** Deterioration rates, trigger
  thresholds and unit rates come from the engineer, and they go in `inputs/lookups.xlsx` — never
  hard-coded into C#.

Everything else — conventions, patterns, the `jcass-dm` tool, when to stop and escalate — is in
`docs/`. This file deliberately carries no knowledge of its own: three copies of a fact become
three different facts.
