# Design rules

**This is the "why" behind everything else in this repository.** Twenty-six rules, each with the
reasoning that produced it. They were settled while the Assistant was built, between 9 and 28
August 2026, and they are live rules rather than history.

## Who this file is for

**Read this before you change how the Assistant behaves** — its documentation, its conventions,
its skills, or the `jcass-dm` tool. Not before every session: for ordinary work with an engineer,
[`00-start-here.md`](00-start-here.md) is the entry point and this file is one click behind it.

Two different jobs, and it matters which one you are doing:

| You are | Read |
|---|---|
| Helping an engineer write a domain model | [`00-start-here.md`](00-start-here.md), and follow its routing table |
| **Changing this Assistant** — a doc, a rule, a skill, the tool | **This file, first.** Then make the change |

## The rules carry their reasoning, and that is deliberate

Every rule below says *why*, at whatever length the why needs. That is not padding. A rule whose
reasoning is not visible is a rule somebody simplifies away six months later while being helpful,
and the reasoning is then rediscovered the expensive way. **If a rule looks wrong, raise it — do
not quietly work around it.** Several of these were rewritten after exactly that conversation, and
one of them (rule 26) records the design it replaced.

Some rules are also *warnings about their own failure mode* — rule 14 and rule 17 in particular
explain how they break when read too strictly. Read those parts. A rule applied too broadly gets
abandoned wholesale, which is worse than never having had it.

## Numbering

The numbers are stable and are referred to elsewhere by number. The build plan that produced them
called these "Locked decisions" — decision *n* and rule *n* are the same thing.

Where a rule concerns how Lonrix operates the Juno Cassandra service rather than how you write a
domain model, the operational detail is deliberately not repeated here. This repository is public.

---

## 1. Name and home

The Assistant is **`JCassDomainModelAssistant`**, its own **public** GitHub repository under
`Lonrix-Limited`.

**Public is what makes it work without an account.** An engineer downloads a ZIP, with no
credential, no licence check and no Lonrix involvement. That is the whole distribution mechanism.

It is also **why `refs/` must contain reference assemblies rather than real ones**: a public
repository publishes whatever is committed to it. See rule 11.

## 2. The engineer's model is a sibling folder, never inside the Assistant

Debug upload requires **source only**, rooted at the `.csproj`, with exactly one `.csproj` at the
top of the zip. Two invariants follow, and everything else rests on them:

- **The Assistant is never uploaded.**
- **The model folder is always uploaded whole.**

That is what lets [`../examples/ExamplesLibrary/`](../examples/ExamplesLibrary/) and
[`../reference-model/DomainModelSample/`](../reference-model/DomainModelSample/) carry their own
`.csproj` files safely — they are inside the Assistant, so they are never in an upload.

It is also what makes rule 18 safe. Because the model is outside, the Assistant can be replaced
wholesale without touching the engineer's work.

**This is the thing this audience finds hardest**, and it is load-bearing rather than tidy. An
engineer who does not know where their model went cannot check your work, and finds out weeks
later — see rule 23.

## 3. Full tooling, not documentation alone

`jcass-dm` scaffolds a project, reads and writes the bundle, checks consistency and packages the
upload zip.

**Documentation describes; tools enforce.** A convention that exists only as prose is a convention
that is followed until somebody is in a hurry. A convention a tool refuses to violate is a
convention.

## 4. `DomainModelSample` is the worked reference, not the starting point

It lives at [`../reference-model/DomainModelSample/`](../reference-model/DomainModelSample/) and it
is there to be *read*. **Nobody starts by renaming it.**

Renaming a sample by hand is how the four-name failure class happens — four names must agree, a
manual rename gets three of them, and the failure appears much later as *"class not found in the
specified .dll"*. See [`conventions/four-names.md`](conventions/four-names.md).

## 5. Scaffold-from-sample is the walking skeleton

```powershell
.\tools\jcass-dm.exe scaffold MyRoadModel --from-sample --output ..\MyRoadModel
```

emits a **correctly-named** project carrying the reference model's working logic. The engineer
proves the whole pipeline on *that* — the artefact they keep — then replaces sample logic with
their own engineering, file by file, with a working build at every step.

**No throwaway, and no hand rename.** The generator cannot emit a mismatched name, which removes
the four-name failure class rather than documenting it. That is the difference between a trap you
warn about and a trap that no longer exists.

If the engineer's own model is the first thing that fails at F5, they cannot tell whether the C# is
wrong or the setup is. Prove the pipeline first and every later failure is attributable.

## 6. The local check is an explicit subset; the web app is authoritative

`jcass-dm check` covers what is visible locally, and **says so**. It does not fork the web app's
setup validators.

Two implementations of the same rule drift, and the one the engineer runs locally is the one that
will be wrong. The web app's Check Setup is the authority; `jcass-dm check` is the fast local
subset that catches the common failures before an upload.

## 7. `supporting/` is the convention for side-car setup data

Coefficient tables, fitted parameter sets and anything else too big for `lookups.xlsx` go in the
project's `supporting/` folder, loaded at setup — see
[`patterns/setup-data-from-supporting-csv.md`](patterns/setup-data-from-supporting-csv.md).

Bundle-side CSVs are mentioned in the documentation only as *what some existing models happen to
do*, never as the recommendation.

## 8. GitHub-into-sidecar is out of scope

There is no path that pulls a model from GitHub directly into the debug workspace. It would need
outbound network access from a deliberately confined environment plus a credential story, and it is
untested.

**An untested path in client-facing documentation costs more than an absent one.** Zip in, zip out,
both ways.

## 9. Skills are thin wrappers, never holders of unique knowledge

Anything a skill under `.claude/` does, a non-Claude agent must be able to do by reading `docs/`
and calling `jcass-dm`.

**The test is mechanical: delete `.claude/` and the Assistant still works at full capability, with
more typing.** That is stronger than "agent-agnostic", and it is why `jcass-dm` has to be genuinely
good — the tool is the mechanism, the skill is convenience.

The failure this prevents is a two-tier product, where the Claude users get a working Assistant and
the Copilot and Cursor users get a folder of markdown.

## 10. Publish is browser-only, and therefore agent-proof

There is no command-line publish path, and publishing needs an administrative grant the modeller
cannot give themselves.

**The worst an over-eager agent can do is write bad code locally.** This is a designed safety
property; do not weaken it by inventing a shortcut around it.

## 11. Reference assemblies are a deterrent, not a security control

The assemblies in [`../refs/`](../refs/) are metadata-only. They stop casual local running of the
framework; anyone holding real framework DLLs from elsewhere can host a domain model.

**The real control is the web app's licensing.** Do not let the documentation imply otherwise —
overstating a deterrent is how people come to rely on it.

## 12. Nothing about how Lonrix operates the service

This repository is public and client-facing. It carries **no** server or infrastructure detail, no
administrative tooling or procedures, no operational file paths, no database internals, and **never
another client's name or model**.

**Enforced mechanically, not by care.** [`../scripts/leak-scan.ps1`](../scripts/leak-scan.ps1) runs
in CI and fails the build on a match. Curation by human attention fails *silently* — nobody notices
the paragraph that should not have shipped, there is no error, and the first signal is somebody
reading it. Curation by CI fails *loudly*, on the push that introduced it, while the author still
remembers what they were doing. That asymmetry is the entire reason the scanner exists. It is not a
security control; it is a smoke alarm.

If a scan hit is in prose describing how the service works, **the content is wrong — change the
content.** Suppression is for a genuine word collision, and every suppression is printed on every
run so that it cannot hide.

## 13. The primary case is a brand-new custom domain model

Everything is written for an engineer building a model for a client that does not yet have one,
where the walkthrough's practice publish is harmless.

**The takeover case is different and gets an explicit warning at the publish gate.** An engineer
inheriting a client that already runs a custom model is one click from replacing a live production
model, because a custom domain model has exactly one version. A rollback slot exists, but recovery
is an intervention rather than a button.

So in the takeover case: prove the pipeline as far as **F5**, which changes nothing outside the
debug workspace, and stop there. Publish only when there is a change genuinely meant to go live.
See [`workflow/40-publish.md`](workflow/40-publish.md).

## 14. Work from canonical guidance, never invention — and escalate concretely

This is the second half of the stance whose first half is *plumbing, never engineering judgement*,
and it applies across every document and every skill. Three tiers, hinged on a test the agent can
actually decide:

- **Proceed** — composing documented patterns, where every framework call appears in the generated
  API reference.
- **Proceed and flag** — not a documented pattern, but built only from documented API. Do it, and
  say plainly that it is not canonical and is worth checking with Lonrix.
- **Stop and refer to Lonrix Support** — any framework call **not** in the API reference; anything
  needing a server or administrative action; the documentation contradicting what the engineer sees
  on screen; a failure the documentation does not cover.

**"Not in the API reference" is the load-bearing test**, because it is exactly the moment an agent
is inventing, and because it is mechanically checkable rather than a judgement call.

**This is deliberately not an absolute ban on undocumented work**, and reading it that way breaks
it. Most real work is composition rather than exact match, so a strict reading makes the agent
refuse constantly — and **a rule that fires too often gets switched off, which means it is not there
when it matters.**

**Escalation must be a drafted support request**: what was attempted, the exact error, the framework
version stamp, the model name. *"Contact support"* on its own produces *"it doesn't work"* and a day
spent establishing basics. Use [`support-request-template.md`](support-request-template.md).

## 15. `support@lonrix.com` is the single escalation destination

Every stop condition in rule 14, every skill that gives up, and the support-request template point
there and nowhere else.

**Three routes means none of them stays maintained.** One destination is a destination somebody
watches.

## 16. The engineer needs a paid AI assistant, they choose which, and they pay for it

Three parts, all of which belong in
[`orientation/prerequisites.md`](orientation/prerequisites.md):

- **Agent-agnostic is about *which*, not *whether*.** The Assistant works with any assistant that
  runs in the editor and can run commands and read their output. It does not work from a browser
  chat window.
- **Claude is the recommendation and the supported configuration.** Other editor-based agents work;
  they are untested. State one default rather than a comparison — this audience needs a path, not a
  survey.
- **The client pays. Lonrix does not pay and does not procure.** Say so plainly, and pair it with
  the honest framing that defuses it: **the Assistant is an accelerant, not a licence requirement.**
  A client is free to write a custom domain model from scratch unaided, exactly as before. Nobody is
  being made to buy a subscription in order to use Juno Cassandra.

## 17. Thresholds and rates never go in C#

Every **tunable** number — a trigger age, a condition limit, a unit rate — goes in
`inputs/lookups.xlsx` and is read through the `Constants.cs` pattern
([`patterns/constants-from-lookups.md`](patterns/constants-from-lookups.md)).

**This is not style.** A number hard-coded in C# is a number the modeller cannot change without a
developer, a rebuild and a republish. A number in `lookups.xlsx` is one they change themselves on
the Tuning page and re-run. That difference is the whole distance between a model somebody can
calibrate and a model they can only file tickets against.

**It binds the agent specifically.** Asking the engineer for a value is **not sufficient**. An agent
that asks *"what age threshold?"* and then writes `const int ResealAgeYears = 12;` has done the
wrong thing while appearing cooperative. The value goes in a lookup row; the code reads it through
`Constants`.

### It applies to tunable numbers, not to every numeric literal

Read too broadly — *"no numbers in C#"* — this rule pushes array bounds, unit conversions and
sentinel values into `lookups.xlsx`, makes models unreadable, and then gets abandoned wholesale. The
test is one question:

> **Would a modeller ever change this to recalibrate the model?**

Yes → `lookups.xlsx`. Changing it would break the *code* rather than change the *forecast* → it
stays in C#, as a named constant.

Legitimately staying in C#: unit conversions, mathematical constants, array indices and bounds,
normalisation factors, framework sentinel values such as the `-999` invalid-coordinate marker, and
structural limits that are part of how the code works rather than what the model predicts. Name them
properly — a magic literal is still bad practice, it is just not a *lookup*.

### Three tiers, and the documentation teaches the boundaries, not just the middle

`lookups.xlsx` for tunable scalars addressed by (set, key); `supporting/` for coefficient data at
CSV scale; C# for structure.

**The agent must recommend the third tier proactively, not wait to be asked.** When a *set* of
related constants appears — regression coefficients, logistic model parameters, distribution
definitions, per-cohort or per-material parameter tables — the answer is a CSV in `supporting/`,
loaded at setup. Not forty rows in `lookups.xlsx`, and certainly not C#.

The test is **update granularity and provenance**, which is sharper than counting values:

> **Does this change one value at a time, or as a whole set?**
> **Was it chosen by judgement, or produced by a fit?**

Changed one at a time by a modeller exercising judgement → `lookups.xlsx`. Regenerated as a set —
refit the regression and every coefficient moves together, arriving from R or Python as a file →
`supporting/` CSV. Nobody hand-edits forty lookup rows after a refit, and a model that asks them to
will be recalibrated wrongly or not at all.

Size is a weaker signal but still worth stating: `lookups.xlsx` addresses a value by (set, key) and
stops being workable at a few hundred rows.

### The reference model contains deliberate counter-examples

`RoutineMaintenanceConditionGreaterThan` and the per-material rates in `SampleElement` are
hard-coded **on purpose**, as the contrast that makes the rule visible, and the reference model's
README sets them as the reader's first two exercises.

An agent reading that code will copy the shape unless the counter-examples are labelled **in the
code itself**, not only in the README. Keep them labelled.

## 18. Improvements reach engineers by re-download, not by `git pull`

Engineers download a version, work with it, and when a better one is released they are told,
re-download, and run it in *refactor mode* over the model they already have. No git knowledge, no
merge, no partial update.

**This is safe only because of rule 2**: the model is a sibling folder, so the Assistant is
replaceable wholesale. Had the model lived inside the Assistant, re-download would be destructive.
Say so in the documentation — a non-developer's first fear is losing their model, and *"this
replaces the Assistant only, never your model"* has to be unmissable.

**That is true of the model and it is not true of everything.** Rule 26 is the correction: the
folder being replaced also holds `model-knowledge/`, and the folder's absolute path is what the
agent's conversation history is keyed on. **Never state this rule without the qualifier** — an
unqualified *"the Assistant is stateless with respect to your work"* shipped for three weeks and is
exactly what made the loss invisible. The procedure that makes an update safe is
[`orientation/updating-the-assistant.md`](orientation/updating-the-assistant.md), and the
load-bearing instruction on it is *unpack to the identical absolute path*.

Refactor mode needs no new machinery: it is the adoption path (rule 19) reused — `check` first,
then recommend.

Five consequences the release process must carry:

- **`refs/` is replaced too**, so the framework update rides along, and each release is stamped with
  the framework version it carries.
- **A model's own `refs/` is *not*, and needs a script.** `scaffold` copies the Assistant's `refs/`
  into the model folder because the emitted `.csproj` references `refs\*.dll` relative to itself,
  and nothing ever refreshed that copy — so a re-download left the engineer compiling against the
  previous framework while the documentation described the current one, silently.
  `scripts/refresh-model-refs.ps1` is that step, and it is clean-by-default because the reference is
  a wildcard and a leftover assembly is compiled against rather than ignored. **The tool detects the
  drift as well as the script fixing it**: `jcass-dm check` compares the commit stamped on the
  model's assemblies against the one on the Assistant's and reports a NOTE when they differ. That
  was added in preference to another paragraph, because it fires on a command the engineer already
  runs rather than on an update page they see twice a year — and it is a NOTE rather than a refusal
  because a stale reference still builds, and a check that blocks work over something not yet wrong
  is a check somebody stops running.
- **A release has to be identifiable.** `ASSISTANT-VERSION.txt` carries the commit and the release
  date, because `refs/FRAMEWORK-VERSION.txt` identifies the *framework* and
  `tools/jcass-dm.build.txt` only moves when the tool source does — it read the same commit for
  every download over a month while the documentation around it changed repeatedly.
- **The changelog needs a per-release *"what to re-check in your model"* section**, because `check`
  catches only what is mechanically checkable and a new *guidance* rule is prose that nothing will
  surface.
- **The notification channel is the web app's existing What's New feature**, not a new one.

## 19. Adopting an existing model is a first-class entry path

*"Help me refactor the domain model in folder X"* must work as well as scaffolding a new one. It
differs in four ways that the documentation and tooling must handle:

- **`check` becomes the *first* action**, not a late one. You do not yet know which of the model's
  conventions are deliberate.
- **The model may already violate the four-name rule**, so `jcass-dm rename` fixes all four
  atomically rather than leaving the engineer to do it by hand. **Never rename by hand** — a manual
  rename gets three of the four names, and the failure surfaces much later.
- **Downloading the source is an *entry* route**, because the engineer may have no local copy at
  all: **Download source zip** on the Debug Model page.
- **It is by definition the takeover case**, so rule 13's publish warning applies in full.

See [`workflow/05-adopt-an-existing-model.md`](workflow/05-adopt-an-existing-model.md).

## 20. Evidence must compile against the current framework

When looking for a pattern, a signature or an idiom, the sanctioned sources are in **this
repository** — [`framework/api/`](framework/api/README.md), [`patterns/`](patterns/README.md),
[`../examples/ExamplesLibrary/`](../examples/ExamplesLibrary/) and
[`../reference-model/DomainModelSample/`](../reference-model/DomainModelSample/).

For any other domain model an engineer hands you, one test decides whether it is evidence of
anything:

> **Does it compile against the current framework?**

No → **disregard it entirely, and say so** rather than working around it.

**Staleness matters more than it looks, because it is invisible.** A domain model that no longer
compiles looks *exactly* like a working model when you read it, and copying from one produces code
that fails at build time if you are lucky and misbehaves at run time if you are not. Real examples
caught doing precisely this: a call to a seven-argument `TreatmentInstance` constructor that no
longer exists, and calls to `GetRawData_Number` and `GetParametersForDomainModel`, neither of which
is in the framework at all any more.

If a pattern exists only in a model that does not build, that is a signal the pattern needs
establishing properly — not a licence to copy it.

Lonrix maintains its own list of which internal models may and may not be used as evidence; that
list is not part of this repository.

## 21. The behaviour scenarios are not shipped here

The scenarios that test whether an agent still honours these rules are maintained by Lonrix and are
deliberately **not** in this repository.

They carry their expected answers. Ship them alongside the rules and the agent under test can read
them, pattern-match the expected response, and the suite stops measuring anything. **It is the exam
paper: it does not go in the exam room.**

So do not add them here, and do not recreate them here. This is a rule, not an oversight.

## 22. Guided and direct are two settings of one job, and the agent honours the verb

*"Guide me through adding a treatment"* and *"add a treatment called reseal"* are different requests
and must get different responses.

**Why this exists.** The goal was never that engineers avoid C# — it was that they become competent
in it. An engineer who has never made the five-place treatment change themselves cannot maintain the
model, cannot judge whether the agent got it right, and has nothing to fall back on at six in the
evening when support is closed.

- **Honour the verb.** *"guide me"*, *"explain"*, *"how do I"*, *"walk me through"*, *"show me"* →
  **teach**: one step at a time, the engineer makes the edits, the agent confirms understanding
  before moving on. *"add"*, *"make"*, *"do"*, *"fix"*, *"change"* → **act**, then show what changed
  and where.
- **When it is ambiguous, ask.** One short question — *"Shall I make the change, or walk you through
  it?"* — not a paragraph weighing the options.
- **Explain either way.** Even when acting directly, present the change as the numbered procedure
  mapped to the places it touches. The engineer has to be able to check the work, and cannot check
  what they were never shown.

One principle with two settings, not two disjoint behaviours: the explanation is always present, and
only *who types it* varies.

**The tooling serves this rather than replacing it.** `jcass-dm check` in a guided session is not a
shortcut past the teaching — it is the feedback that proves the lesson landed, and explaining what
it just verified is part of the lesson. Likewise a skill invoked during a guided session still
teaches; it must not become the bypass.

**The workflow documents are the guided script.** [`workflow/`](workflow/README.md) is written as
procedures a *human* can follow rather than as instructions to an agent, which means guided mode is
walking somebody through a document that already exists rather than improvising a lesson each time.

## 23. Say where a command runs, and resolve every path out loud before writing to it

Two halves of one failure, and both were observed in real cold starts:

- **Name the terminal, every time.** A bare command block assumes the reader knows what a terminal
  is, which one, and that the one already open is the right one. Civil engineers are not PowerShell
  users. Every command block gets a line in front of it — *"in your PowerShell terminal, the one you
  already have open in the Assistant folder"* — and **Terminal → New Terminal** when they have none.
- **Name the absolute folder before writing outside the Assistant.** `--output ..\MyRoadModel` is
  relative to *the terminal's* folder, which the engineer cannot see. Before `scaffold`, say *"this
  will create `C:\Work\MyRoadModel`, beside the Assistant folder"* and ask them to confirm they can
  create files there.

**The second half is not fussiness.** The relationship between the folder the Assistant lives in and
the folder the model lives in is the thing this audience finds hardest, and rule 2 rests entirely on
the two being siblings. An engineer who does not know where their model went cannot check the
agent's work, and finds out weeks later when an update deletes a model that was nested inside the
Assistant.

**The framework half of this was fixed in the tool rather than documented.** `jcass-dm scaffold`
against an unwritable folder used to throw out to the top-level handler and report *"jcass-dm failed
unexpectedly. This is a bug in the tool"* over a stack trace — a support email about a Windows
permission the engineer could have fixed in ten seconds. It now probes the nearest existing ancestor
for writability and refuses with a plain message naming the folder. **Guidance is what you write
when you cannot fix the thing itself**; here both were available and both were done.

The page is [`orientation/running-commands.md`](orientation/running-commands.md).

## 24. Plan the engineering before scaffolding, and walk the guidelines one at a time

An engineer who arrives at `scaffold` without their lists is designing the model and learning the
framework simultaneously, and every problem afterwards looks like both.

Four guidelines, and they are engineering rather than code, which is why they are a page of their
own — [`workflow/01-plan-your-model.md`](workflow/01-plan-your-model.md):

1. **Start simple and prove the pipeline on the simple version.** Gall's Law, stated explicitly.
   Upload, breakpoint, publish and run a nearly trivial model *first*, then add parameters,
   treatments and rules one at a time. This is the walking skeleton (rule 5) restated as an
   instruction to the engineer rather than a property of `--from-sample`.
2. **Three lists before any code** — the treatments, the input columns needed to initialise, and the
   parameters the model steps forward with. The test that separates the second from the third is
   *does this change as the model steps into the future?*
3. **Per parameter, an increment rule and a reset rule per treatment.** On paper, in a flow chart or
   as equations; the form does not matter. Complex reset rules start as simple ones — an untestable
   rule written first has nothing working to be compared against.
4. **Thresholds and constants listed separately and grouped from day one**, with set names like
   `trigger_thresholds` or `candidate_selection`, because a mature model has hundreds of them and
   they land in `lookups.xlsx` addressed by (set, key). Grouping is cheap on day one and not done
   later.

**One at a time, waiting for each answer.** Four guidelines delivered in one message produce
agreement and no lists, which is the failure mode this rule exists to prevent. The lists are the
engineer's; the agent's job is to catch the input column that is really a parameter, to push back
once when the first model is not simple, and to recognise a fitted coefficient set on sight (rule
17's third tier).

## 25. A GitHub account is recommended and never required

Rule 18 says improvements arrive by re-download rather than `git pull`, and
[`00-start-here.md`](00-start-here.md) tells agents never to assume git knowledge. Both stand. What
was missing is that a model maintained for years deserves a history, and the engineer's own model
folder is exactly the thing git is good at.

So: **recommend it once**, in the prerequisites, as optional and with the reason — history, a way
back, sharing with a colleague or with support. **Never make a git step part of a procedure**, never
raise it unprompted mid-task, and never route an Assistant update through it. Help with it as an
ordinary request if the engineer raises it.

The distinction that keeps this from contradicting rule 18: **git is for *their model*, which is
theirs and stateful. It is never for *the Assistant*, which is replaced wholesale.**

## 26. Per-model knowledge lives in `model-knowledge/`, and the agent asks for it when it is missing

> Added 2026-08-28, from the question of how a client who has worked for two months takes an update
> without losing what their agent learned. **Rewritten the same day** — see *Why not the model
> folder* below.

Rule 18 says improvements arrive by re-download, and justifies it with rule 2: the model is a
sibling folder, so the Assistant is stateless with respect to the engineer's work and can be swapped
wholesale. **That is true of the model and false of the agent.** Claude Code's `#` shortcut writes
project memory to the root `CLAUDE.md`, which is a *shipped* file; permission grants land in
`.claude/settings.local.json`; notes land wherever the session happened to be. All of it sits inside
the folder an update replaces, and all of it goes with no diff, no conflict and no warning.

**The test, applied to anything an agent has learned:**

> **Is it about a specific domain model, about the framework, or about their machine?**

| It is about | It belongs |
|---|---|
| **A specific domain model** — the four names, lookup set names, budget categories, input column meanings, and above all the *why* behind a modelling decision | `model-knowledge/<DmName>.md` in the Assistant, one file per model, the model name as the H1 |
| **The framework or the Assistant** — a signature that had to be worked out, a failure the documentation did not cover | **Upstream, to `support@lonrix.com`.** Not kept locally |
| **Their machine** — permission grants, chat transcripts | Left alone. Cheap to recreate, and transcripts survive on their own if the folder path is stable |

**The middle row is the one that changes behaviour.** A framework fact recorded in a client's local
notes is a *symptom, not an asset*: either the documentation did not cover it, or it did and it was
not findable. Preserving the note fixes it for one client, leaves every other client rediscovering
it, and lets it go stale against the next release with nobody watching. It is routed upstream for
that reason.

### Why not the model folder

The first version of this rule put the notes in the engineer's model folder, on the reasoning that
the model is the stateful artefact and the Assistant the replaceable one. Two things kill it.

**An engineer often asks about a model whose folder is not open** — or which exists only on the
server, which rule 19 makes a first-class entry path — and there is then nowhere for the knowledge
to live, or to have lived before the folder existed. And it made the whole design depend on an agent
being able to write to a second workspace root, which is unverified.

`model-knowledge/` is reachable whenever the Assistant is open, which is always.

### The cost, stated plainly

**The folder is inside the thing that gets replaced.** That is accepted deliberately, because the
copy is *one folder* rather than a merge — which is not what rule 18 rules out — and because the
risk is closable without machinery:

- **The trigger is the ask, not the session.** When the engineer asks about an existing model and
  `model-knowledge/<name>.md` does not exist, the agent says so and asks them to copy the folder
  from their previous Assistant before going further. **That fires exactly when it matters, and
  never during ordinary work** — which is the failure mode rule 14 warns about: a check that fires
  constantly gets ignored, and is then not there when it counts.
- **A sibling `*-old` or `*-main` folder holding `model-knowledge/` files turns the guess into a
  fact**, and lets the agent name the exact folder to copy from.
- **No counter, and no session bookkeeping.** A counter needs a write on every session, and
  `.claude/settings.json` allows only read-only commands on purpose. The empty-or-missing file
  answers the same question with no state to maintain.

### Two rules bind the agent

- **Append to `model-knowledge/<name>.md`; never rewrite it.** A notes file rewritten wholesale
  loses history the same way an update does.
- **Write nothing else into the Assistant** — no memory in the root `CLAUDE.md`, no scratch files.
  `model-knowledge/` is the one writable place, and it is writable because it is the one place an
  update procedure carries across.

### Per-model skills are refused, deliberately

Rule 9 is that skills hold no unique knowledge — delete `.claude/` and everything still works. A
per-model skills folder is the first place that stops being true, and it rebuilds the two-tier
product rule 9 exists to prevent: the Copilot and Cursor users get nothing.

A client's recurring model-specific procedure is a **section in `model-knowledge/<name>.md`**, which
every agent can read.
