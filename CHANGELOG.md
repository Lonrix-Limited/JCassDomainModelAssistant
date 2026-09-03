# Changelog

What changed in each release of the Domain Model Assistant, and — the part worth your time —
**what to re-check in your own model** because of it.

**Releases are named by date**, because the only question anyone actually asks is *"is the one I am
holding older than the one on GitHub?"* and two dates answer that without anybody having to know a
numbering convention. Your copy's date is in [`ASSISTANT-VERSION.txt`](ASSISTANT-VERSION.txt) at the
root, beside the commit it was cut from. Note that the `jcass-dm` tool carries a version number of
its own (`jcass-dm version`) and it is a different thing.

**Taking a newer release without losing anything is a short procedure, not a straight swap** —
[`docs/orientation/updating-the-assistant.md`](docs/orientation/updating-the-assistant.md). Read it
before you download.

---

## Unreleased

**Headline: `jcass-dm check` now tells you when your model is compiling against an older framework
than this Assistant documents.**

### Added

- **`check` has a new `framework reference` rule.** It reads the framework build stamped on the
  assemblies in your model's `refs\` folder and compares it with the Assistant's own, and says so
  when they differ. Until now the only thing between you and a model quietly built against a
  superseded framework was having read step 5 of the update page. It reports as a **NOTE**, not a
  problem: a stale reference still builds and still runs, and refusing to check your model over it
  would block work for something that is not wrong yet.
- **It also catches a `refs\` folder holding two different framework builds**, which is what copying
  files in over the old ones leaves behind. That one is not harmless — the project references
  `refs\*.dll` with a wildcard, so the leftover is compiled against alongside its replacement.
- **[`docs/conventions/silent-failures.md`](docs/conventions/silent-failures.md) § 13** describes
  the failure and the fix.

### Fixed

- **`scaffold` printed the wrong instruction** when it could not find the Assistant's `refs\` folder
  — it said to copy the files across by hand, which tops the folder up rather than replacing it. It
  now names `scripts\refresh-model-refs.ps1` and the project folder to pass it. This only ever
  appeared when `jcass-dm.exe` had been copied out of the Assistant on its own.

### What to re-check in your model

- **Run `.\tools\jcass-dm.exe check --project ..\YourModel` and read the `framework reference`
  line.** If it says your model is on a different build from this Assistant, run
  `.\scripts\refresh-model-refs.ps1 -Project ..\YourModel` and rebuild.
- **Nothing else in your model needs changing.** No C# convention changed, and no bundle sheet
  changed.

---

## 2026-09-03

**Headline: your assistant can now remember things about your model across an update, and there is a
procedure for updating that does not lose them.**

### Added

- **[`model-knowledge/`](model-knowledge/)** — a folder at the root where your assistant writes what
  it learns about *your* models, one `.md` file per model. Your lookup set names, your budget
  categories, what an input column actually measures, and the reasoning behind decisions you made
  weeks ago. It reads the file when you ask about an existing model, and asks you for it when there
  isn't one. **It is the only place in the Assistant your assistant writes to.**
- **[`docs/orientation/updating-the-assistant.md`](docs/orientation/updating-the-assistant.md)** —
  how to move to a newer release. The single most important instruction on it is *unpack the new one
  to exactly the same folder path as the old one*, because your assistant's memory of every previous
  conversation is filed under that path and moving the folder orphans all of it.
- **[`scripts/refresh-model-refs.ps1`](scripts/refresh-model-refs.ps1)** — updates your model's own
  `refs\` folder from the Assistant's. **This is the gap that mattered most**: your model has a
  private copy of the framework reference assemblies, made when it was scaffolded, and until now
  nothing ever refreshed it. Downloading a newer Assistant left your model still compiling against
  the older framework, with no error and nothing to notice.
- **[`ASSISTANT-VERSION.txt`](ASSISTANT-VERSION.txt)** — says which release you are holding. Quote it
  when you email support.
- **This changelog.**

### Fixed

- **The claim that the Assistant is "entirely stateless with respect to your work" was wrong**, and
  it is what made all of the above invisible. It is true of your *model*, which lives in its own
  folder next door and is never touched. It was never true of your *assistant*, whose notes and
  conversation history live inside the folder an update replaces.
- **Adopting an inherited model** now refreshes `refs\` with the script rather than copying files in
  over whatever was there. The old instruction topped the folder up; because the project references
  `refs\*.dll` with a wildcard, a leftover assembly from an older framework was compiled against
  alongside its replacement.

### What to re-check in your model

- **Run `.\scripts\refresh-model-refs.ps1 -Project ..\YourModel` and rebuild.** Do this whether or
  not you think your model is current. If your model was scaffolded with an Assistant from before
  2026-08-17 it is compiling against an older framework than this release documents, and you would
  have no way of telling from the outside.
- **Nothing else in your model needs changing.** No C# convention changed in this release, no bundle
  sheet changed, and `jcass-dm` is unchanged.

---

## 2026-08-11 — first beta

The release the first engineers worked with: the framework reference assemblies and generated API
reference, the concepts mirror, the conventions and pattern library, the end-to-end workflow, the
`DomainModelSample` reference model, `jcass-dm`, and the nine Claude skills.

**If this is what you are holding**, the update procedure above is written for you, and the thing it
is protecting is your assistant's memory of the conversations you have already had.
