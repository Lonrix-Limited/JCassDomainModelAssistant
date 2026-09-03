# Copilot instructions

This repository is the **Juno Cassandra Domain Model Assistant**. It gives an AI coding assistant
the context it does not otherwise have: how the Juno Cassandra framework expects a **domain model**
to be written, which of its conventions fail silently, and the tooling to check your work.

**Read [`docs/00-start-here.md`](../docs/00-start-here.md) before doing anything else.** It is the
single entry point and it routes you from there.

Three things to know before you read it, because they change what you should do:

- **The engineer's model is a sibling folder, never inside this repository.** You edit their model;
  you do not edit the Assistant.
- **You do the plumbing. You do not supply engineering judgement.** Deterioration rates, trigger
  thresholds and unit rates come from the engineer, and they go in `inputs/lookups.xlsx` — never
  hard-coded into C#.
- **`model-knowledge/` is the one place in this repository you write to.** It holds what a previous
  session learned about the engineer's own models, one `<ModelName>.md` per model. Checking it is
  your first action on any model that already exists — see below.

## Before you work on a model that already exists

**The engineer names a model, or says "my model", or points you at its folder. Do this first, before
reading their code.**

1. **Look for `model-knowledge/<ModelName>.md`.** The `README.md` in that folder is not a notes file
   — a folder holding only the README is a folder with no notes in it.
2. **If it is missing, list the folder that *contains* this repository** and look for a sibling
   `JCassDomainModelAssistant*-old` or `*-main` with `model-knowledge` files in it. After an update
   that is where their notes usually are.
3. **What you do next depends on what they asked for, and this is the part to get right.**

| They asked you to | You |
|---|---|
| **Change the model** — add a treatment, a parameter, an input column or a lookup value; rename; refactor; fix something | **Stop.** Ask them to copy the notes across — naming the exact folder if step 2 found one — and **wait for their reply before making the change.** Not a remark at the end, and not after the edit |
| **Answer a question** — run `check`, explain how something works, diagnose a failure, tell them whether something is right | **Answer it first.** Then say the notes file is missing and ask for it **before anything is changed** |

**Why the split.** A read-only answer cannot be wrong for lack of notes — `check` reads the project
file, the bundle and the C#, and no note an engineer writes changes what it reports. A stop in front
of it costs a round trip, buys nothing, and lands on what is often somebody's first contact with the
Assistant. **An edit is different**: the notes are where the reasons behind the model's decisions
are, and changing it without them is how a deliberate choice gets tidied away.

If they say the model is new to them too, believe them, offer to start the file, and do not raise it
again in that session.

**And when they say "remember that..." about their model, that goes in
`model-knowledge/<ModelName>.md` too — an ordinary edit to a file in this repository.** Not into a
memory feature of your own, wherever your tool keeps one. A note the engineer cannot open is a note
they cannot correct, and the next session may not be the same assistant you are. Say which file you
wrote it to.

**Why this is a stop, what to write in the file, and what belongs upstream to Lonrix instead:**
[`docs/00-start-here.md`](../docs/00-start-here.md) § 4.

## Everything else

Conventions, patterns, the `jcass-dm` tool, when to stop and escalate — all in `docs/`.

**If you are changing the Assistant itself** — a document, a convention, a skill, or the `jcass-dm`
tool — read [`docs/design-rules.md`](../docs/design-rules.md) **first**. It carries the twenty-eight
design rules and, more importantly, the reasoning behind each one.

Beyond the procedure above, this file carries no knowledge of its own: three copies of a fact become
three different facts.
