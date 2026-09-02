# model-knowledge/

**What your assistant has learned about *your* domain models, one file per model.**

This folder is written by your AI assistant, not by you — though you are welcome to read it, and to
correct it. Each file is named after the model it describes:

```
model-knowledge\
    README.md          <- this file. It ships with the Assistant.
    NelsonRoads.md     <- everything your assistant knows about NelsonRoads.
    BridgesV2.md       <- and about your second model.
```

---

## Why this folder exists

An AI assistant starts every conversation knowing nothing about the work you did yesterday. Left to
itself it re-asks the questions you already answered — which lookup set holds your trigger
thresholds, what your budget categories are called, what `par_rut_left` actually measures, and above
all *why* you decided something the way you did.

Written down here, it does not have to ask again.

## The one thing you must do

**When you update the Assistant, copy this folder across to the new one.**

Everything else in this repository is replaced when a newer version is released — that is how
improvements reach you, and it is safe, because your model lives in its own folder next door. This
folder is the exception: it is *inside* the thing that gets replaced, and it holds the only work of
yours that is in here.

The full procedure, in order, with the step that matters most first:
[`docs/orientation/updating-the-assistant.md`](../docs/orientation/updating-the-assistant.md).

## What belongs in here, and what does not

| What your assistant learned | Where it goes |
|---|---|
| Something about **one of your models** — the four names, your lookup set names, budget categories, what an input column actually measures, and the reasoning behind a modelling decision | **Here**, in `<YourModelName>.md` |
| Something about **the Juno Cassandra framework or this Assistant** — a signature that had to be worked out, a failure the documentation did not cover | **To support@lonrix.com.** Not kept here. If the documentation did not cover it, every other engineer is about to hit the same thing, and a note in your folder fixes it for you alone |
| Something about **your machine** — which folder you work in, a permission you granted | Nowhere. It costs nothing to re-establish |

## What is in a file

Whatever turned out to be worth remembering. There is no template, because a template gets filled in
dutifully with nothing in it. In practice these files accumulate:

- **The four names** of the model, and the client it belongs to.
- **Lookup sets** — which `lkp_` sheet holds what, and the naming you settled on.
- **Budget categories**, and which treatments land in which.
- **Input columns**, and what each one actually measures in the field.
- **Decisions and their reasons** — the most valuable rows here, and the ones you cannot recover
  from reading the code. *"Rut depth is measured on the left wheel path only, because that is what
  the survey vehicle records."*

Two rules your assistant follows when writing here:

- **It appends. It never rewrites a file wholesale.** A file rewritten from scratch loses the
  history the same way an update does.
- **This folder is the only place inside the Assistant it writes to.** Not the root `CLAUDE.md`, not
  a scratch file somewhere in `docs\`. Anything written elsewhere in here is lost at the next
  update, silently.

## It is empty right now, and that is normal

Nothing is created until your assistant has something to record. If you have been working with an
older Assistant and this folder is empty, that is the sign you have not copied it across yet — go
back to the old folder and look.
