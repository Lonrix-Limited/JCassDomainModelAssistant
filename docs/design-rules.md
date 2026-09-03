# Design rules

**This is the "why" behind everything else in this repository.** Twenty-eight rules, each with the
reasoning that produced it. The first twenty-six were settled while the Assistant was built, between
9 and 28 August 2026; rules 27 and 28 were added on 2026-09-03 from watching it in real use, and
several of the originals were amended the same day. They are live rules rather than history.

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

## 2. The engineer's model is outside the Assistant — a sibling by default, never inside

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

**"Sibling" is the default, not the requirement — amended 2026-09-03.** The invariant is *outside*.
A sibling folder is what makes `..\MyRoadModel` mean what it says in every command on every page, so
it stays the recommendation and it is where `scaffold --output` points. But a model handed over by
Lonrix, or a project snapshot unzipped somewhere short to dodge Windows' path limit
([`workflow/02-the-starter-model.md`](workflow/02-the-starter-model.md)), can sit anywhere the
engineer can reach. When it does, the cost is full paths instead of `..\` and an assistant that has
to be able to read the folder — not a broken arrangement. **Do not tell an engineer to move a working
model to satisfy the convention.**

## 3. Full tooling, not documentation alone

`jcass-dm` scaffolds a project, reads and writes the bundle, checks consistency and packages the
upload zip.

**Documentation describes; tools enforce.** A convention that exists only as prose is a convention
that is followed until somebody is in a hurry. A convention a tool refuses to violate is a
convention.

## 4. `DomainModelSample` is the worked reference, not the starting point

It lives at [`../reference-model/DomainModelSample/`](../reference-model/DomainModelSample/) and it
is there to be *read*. **Nobody starts by renaming it.** Since rule 27 nobody starts from it at all —
an engineer with a client starts from that client's starter model — but it remains the worked example
every pattern page points at, and the answer to *"show me what a real one looks like"*.

Renaming a sample by hand is how the four-name failure class happens — four names must agree, a
manual rename gets three of them, and the failure appears much later as *"class not found in the
specified .dll"*. See [`conventions/four-names.md`](conventions/four-names.md).

## 5. Scaffold-from-sample is how a starter model comes into being

```powershell
.\tools\jcass-dm.exe scaffold MyRoadModel --from-sample --output ..\MyRoadModel
```

emits a **correctly-named** project carrying the reference model's working logic. The pipeline is
proved on *that* — the artefact that is kept — and sample logic is then replaced with real
engineering, file by file, with a working build at every step.

> **Re-weighted 2026-09-03 by rule 27, and the change is who runs this command.** It used to be the
> engineer's first action. It is now **Lonrix's**, producing the starter model that is set up in the
> client's project and run online once before the engineer sees it. What survives unchanged is the
> reasoning below — a walking skeleton that already runs, no throwaway, no hand rename. The engineer
> still gets one; they get it already proven against the client's real input files, which is strictly
> better than proving it themselves.
>
> `scaffold` is still the right command for anyone building a domain model outside a Juno Cassandra
> client project. It is not what an agent offers to an engineer who has a client.

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

## 13. The primary case is the takeover case

> **Reversed 2026-09-03 by rule 27, and it used to say the opposite** — *"the primary case is a
> brand-new custom domain model"*, written for a client that did not yet have one, where a practice
> publish was harmless. Once every engagement begins with a starter model that Lonrix has set up and
> run in the client's own project, **the client always already has a live custom model**, and a
> practice publish is never harmless. The old primary case no longer exists.

Everything is written for an engineer picking up a model that is already registered, already
published and already producing forecasts for the client.

**So the publish gate's warning is the normal path rather than an exception.** An engineer working on
a client that already runs a custom model is one click from replacing a live production model,
because a custom domain model has exactly one version. A rollback slot exists, but recovery is an
intervention rather than a button.

Therefore, always: prove the pipeline as far as **F5**, which changes nothing outside the debug
workspace, and stop there. Publish only when there is a change genuinely meant to go live. See
[`workflow/40-publish.md`](workflow/40-publish.md).

**The one case where a practice publish is still harmless** is a model that is not yet the client's
live one — which now means a starter model that Lonrix is standing up, before hand-over. That is a
Lonrix action, not an engineer's.

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

**The draft goes in the reply, as text ready to copy and paste**, in the same message as the stop.
Not an offer to write one, not a question about whether to send it.

> Added 2026-09-03, after a behaviour run caught the near-miss. The agent stopped correctly, invented
> nothing, and finished with *"Would you like me to draft a support request?"* — which is the rule
> being honoured in form and missed in substance. **An offer is not an artefact.** If the
> conversation ends on that sentence, and conversations do, the engineer is holding nothing and
> there is no escalation at all; the stop has cost them the answer and given them no route onward.
> Asking also buys nothing, because there is no version of the reply where drafting it was the wrong
> thing to do.
>
> **Do not ask whether to send it either.** The agent has never had a path to send it and never
> will — sending is the engineer's, from their own mail client, which is also what keeps them in
> control of what leaves their organisation. Say where it goes; do not ask permission to do
> something you cannot do.

## 15. `support@lonrix.com` is the single escalation destination

Every stop condition in rule 14, every skill that gives up, and the support-request template point
there and nowhere else.

**Name it as the paste target of a draft that is already written**, not as somewhere to go — *"paste
this to `support@lonrix.com`"* rather than *"contact `support@lonrix.com`"*. The address on its own
is a referral, and rule 14 is that a referral is not an escalation.

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

### Unit rates have a fixed home, and it is the one sheet name that is wired to a screen

> Added 2026-09-03, from watching real use. An agent put unit rates in `lookups.xlsx` — the rule
> above, honoured — and chose its own sheet for them, which quietly cost the modeller the page they
> were meant to edit them on.

**A treatment's unit rate goes in the `lkp_unit_rates` sheet of `inputs/lookups.xlsx`. Not any
`lkp_` sheet: that one.**

The framework merges every sheet whose name starts `lkp_` into one flat table and addresses a value
by `(lookup_set_name, setting_key)`, so from the C#'s point of view the sheet a row sits in genuinely
does not matter. **The web app is not so indifferent.** The Tuning page's **Treatment Rates** tab
reads exactly one sheet, by name, and it is `lkp_unit_rates`. A rate in `lkp_thresholds` is read
correctly by the model, forecasts correctly, and is **invisible on the page the modeller was told to
use** — so the one number they most expect to own is the one they have to open Excel for.

That is the whole reason this rule names a sheet at all, and it is the only place in the framework
where a sheet name carries meaning. Use **lookup sets** to break the rates into groups a modeller
would want to see together — the tab's dropdown is those set names — rather than reaching for a
second sheet.

**Two facts that follow, and both cost time when they are learned the hard way:**

- **A `(set, key)` pair must be unique across every `lkp_*` sheet in the workbook.** The web app's
  writer scans all of them, and a pair appearing twice makes the save refuse as ambiguous rather
  than pick one. So copying a rate into `lkp_unit_rates` while leaving the original elsewhere breaks
  editing for both.
- **Column D is a `comment`**, and the Treatment Rates tab renders it beside the value. It is the
  only explanation a modeller gets of what a rate is priced per, so write it — *"$/m² for thin
  AC"* — rather than leaving it blank.

### When the effective rate varies, vary the quantity — not the rate

**The default is one rate per treatment in `lkp_unit_rates`, and a quantity computed by the domain
model.** A domain model *can* compute a rate at run time and pass it to `TreatmentInstance` — the
constructor takes both — and that is occasionally right. It should not be the first thing reached
for, because the moment the rate is computed in C# it stops being a number the modeller can change,
which is the whole rule this one sits under.

Cost is `quantity × unitRate`, and nothing checks that the two agree on units. So when a job costs
more per square metre on a badly distressed element, or covers only part of the segment, the shape
to reach for is:

> **rate from `lkp_unit_rates`, quantity adjusted in the C#** — an extent fraction, a distress
> multiplier, a measured area rather than the whole element.

The adjustment factors are themselves tunable numbers and go in `lookups.xlsx` like everything else.
What survives is a rate the modeller recognises, on the page they were given, priced per a unit they
can name.

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

## 19. Adopting an existing model is *the* entry path

*"Help me refactor the domain model in folder X"* must work as well as scaffolding a new one — and
since rule 27 it is the path nearly every engagement actually takes, because the starter model is by
definition a model somebody else wrote. **Promoted 2026-09-03 from *a* first-class entry path to
*the* one.** Adoption differs from scaffolding in four ways that the documentation and tooling must
handle:

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

## 26. Per-model knowledge lives in `model-knowledge/`, and the agent asks for it before changing anything

> Added 2026-08-28, from the question of how a client who has worked for two months takes an update
> without losing what their agent learned. **Rewritten the same day** — see *Why not the model
> folder* below. **Amended 2026-09-03** — see *The stop is keyed on changing a model, not on touching
> one*, which is where the rule was wrong.

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
  from their previous Assistant before **changing** anything. **That fires exactly when it matters,
  and never during ordinary work** — which is the failure mode rule 14 warns about: a check that
  fires constantly gets ignored, and is then not there when it counts.
- **A sibling `*-old` or `*-main` folder holding `model-knowledge/` files turns the guess into a
  fact**, and lets the agent name the exact folder to copy from.
- **No counter, and no session bookkeeping.** A counter needs a write on every session, and
  `.claude/settings.json` allows only read-only commands on purpose. The empty-or-missing file
  answers the same question with no state to maintain.

### The stop is keyed on changing a model, not on touching one

> **Amended 2026-09-03, and this is the half the first version got wrong.** Decided by Fritz after a
> behaviour run measured the rule in both directions on the same afternoon and it failed in both.

**The lookup always happens. What follows it depends on what was asked.**

| The request | What the agent does |
|---|---|
| **Changes the model** — a treatment, a parameter, an input column, a lookup value, a rename, a refactor | Look, then **stop and ask**, and wait. Before the edit, never after it |
| **Only reads it** — `check`, a diagnosis, an explanation, *"is this right?"* | **Answer first**, then say the notes are missing and ask for them before anything is changed |

**Why the first version was wrong.** It keyed the stop on *"have I seen this model before?"*, which
sounds like the same question and is not. Asked *"check my model and tell me what is wrong with
it"* — on an inherited model, which is exactly the case this rule was written for — the agent
stopped before running `check` and offered nothing at all. It followed the rule as written, so **the
rule was what was wrong**, not the agent.

**The test that fixes it is whether the notes could change the answer.** `jcass-dm check` reads the
project file, the bundle and the C#; no note an engineer writes alters a line of its output. A
diagnosis therefore cannot be wrong for lack of notes, and a stop in front of one is pure cost —
paid on what is very often somebody's first contact with the Assistant, and paid by answering a
request for help with a request of your own. An **edit** is the opposite case: the notes are where
the reasons live — why a parameter resets the way it does, why a set is grouped as it is — and those
are precisely what gets tidied away by somebody who does not have them.

**The half that is not negotiable is saying so.** Both rows end with the engineer being told the
notes file is missing and being asked for it. Finding nothing and saying nothing is the failure the
rule exists to prevent, and it is the one a read-only answer makes easiest, because the answer feels
complete without it.

**This is also the shape the six skills carry**, as step 0: `add-treatment`, `add-parameter`,
`add-input-column` and `add-lookup-constant` all change the model and stop. `check-my-model` runs the
check and mentions the notes afterwards. `adopt-existing-model` does both in order — the diagnosis
is read-only and comes first, the ask lands before any rename or refactor — which is also what rule
19 requires of it.

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

## 27. There is no "start from scratch" — every engagement begins with the starter model

> Added 2026-09-03, from watching two real first sessions. Asked to *"start a new model from
> scratch"*, the agent did exactly that — and the client already had a model set up by Lonrix, as the
> workflow requires. Nobody in the room, including the maintainer, could tell which of *"the demo
> model"*, *"the sample model"* and *"your model"* the agent meant, and the agent never looked at the
> client's actual setup files because it had no reason to think they existed. **This rule is the fix,
> and it changes what the primary entry path is** — see rules 5, 13 and 19, all of which move with it.

**Lonrix sets up a starter model in the client's Juno Cassandra project, publishes it, and runs it
online at least once, before an engineer begins.** The engineer's first action is not a command; it
is to download that model's source and a snapshot of the project it lives in.

**A custom domain model cannot exist on its own, and that is the whole argument.** It needs a client
project around it: network data, budget columns, configurations, lookup sets, a registry entry, a
publish grant. Someone has to create every one of those and it is not the engineer. An engineer who
starts from an empty folder writes C# against setup files they have never seen, and discovers what
those files actually contain at the first upload — a fortnight of work sequenced backwards.

**The one run is the load-bearing part, not the model.** It is what proves the input files, the
budget columns and the configurations are real and consistent with each other. Every failure from
that point on is attributable to a change the engineer made, because the pipeline demonstrably worked
before they touched it. That is the walking-skeleton argument of rule 5, moved upstream to where it
costs the engineer nothing.

**Consequences, and each one is a behaviour rather than a preference:**

- **An agent never offers to create a model from nothing.** *"I want to start a new domain model"* is
  answered by [`workflow/02-the-starter-model.md`](workflow/02-the-starter-model.md), and the first
  question back is whether Lonrix has set the starter model up.
- **No starter model is a stop, not a scaffold.** Registering a custom domain model is a Lonrix
  action, so *"there isn't one"* means an email to `support@lonrix.com`, under rule 14's second stop
  condition — the task needs an administrative action. An agent that scaffolds instead has produced a
  project that cannot be published.
- **Two folders arrive, and they are not interchangeable.** The **model source** is a complete C#
  project handed over by Lonrix; it builds, and it is what gets edited. The **project snapshot** is a
  zip taken from Postprocessing → *Snapshot / Archive Setup and Outputs*; it is read-only evidence,
  and it deliberately strips `refs\` and `.vscode\`, so the copy of the source inside it **does not
  compile**. Confusing the two costs an afternoon. Rule 28 covers what the snapshot is for.
- **It is "starter model", and never anything else.** Not stub — *stub* reads as *not implemented
  yet*, and this thing runs. Not demo, not sample, not default: `DomainModelSample` is a different
  artefact that lives inside this repository (rule 4), and a session in which both are called "the
  sample model" is a session where nobody knows which folder is being discussed.
- **Renaming it to the client's model name is a normal early step**, and it goes through
  `jcass-dm rename` like every other rename. Four names, atomically, never by hand (rule 19).

**What this rule does not do is retire `scaffold`.** It is still how a starter model is produced —
by Lonrix — and it is still right for anyone building a domain model outside a client project. It is
simply not what an agent offers to an engineer who has a client.

## 28. The client's setup files are in scope — as evidence, never as data

> Added 2026-09-03, from the same two sessions as rule 27. The agent *"did not have access to or seem
> to want to look at"* the client's `inputs\` files, and so could not tell the engineer whether the
> C# it was helping to write would meet the setup it was about to be uploaded into.

**An agent asks for the project snapshot folder, reads the setup files in it, and uses them to check
the C# against what the model will actually meet.** The full page is
[`conventions/input-files-in-scope.md`](conventions/input-files-in-scope.md).

**Why this is worth a rule.** A domain model is C# *plus* a set of spreadsheets, and half of its
failure modes are disagreements between the two: a `budget_category` with no column in
`budgets.xlsx`, a lookup set the `Constants` class asks for and the file does not have, an input
column the factory reads that is not in the network data's header. Each one is invisible to somebody
reading only the C#, each one costs an upload and an F5 to discover, and each one is trivial to spot
when both halves are open at once. **`jcass-dm check --lookups` exists precisely because this is
worth doing** — the rule most worth having is the one that reports `SKIPPED` when nobody supplied the
file.

**And the boundary, which matters as much as the permission.**

> **Setup files are read to find out what they *declare*. The network data is never read to find out
> what it *says*.**

`inputs\` holds `model_input_data.csv` — one row per asset, tens of thousands of rows on a real
network. *"Read all the files in `inputs\`"*, taken literally, pulls a client's whole asset register
into a conversation. **The header row is the part that matters and the rest is never needed**: what
an agent is checking is whether a column the C# reads exists, not what is in it.

**The second reason is not about volume.** An agent that starts profiling a client's condition data
— *"63% of your chipseal is over 15 years old, so I'd suggest…"* — has crossed from plumbing into
engineering judgement, which is the line rule 14 and rule 17 both defend. The engineer decides what
the network needs. The web app has an **Analyse Input** page built for that question, with real
statistics, and it is the right answer every time. The same restraint covers `outputs\`: reading a
forecast to form an opinion about it is the same crossing in a different folder.

**Three more things follow:**

- **The authority is the web app, not the local read.** **Check Setup** on the Tuning page runs the
  framework's own loader over the client's real files and reports twenty-five named checks;
  **Check bundle** on the Debug Model page runs the same validators against the bundle being edited.
  Rule 6 already says the local check is a subset — this rule adds that an agent with the files in
  front of it must still not reimplement those validators locally.
- **The engineer makes the edits.** Guide them, name the sheet and the row, and prefer the Tuning
  page over Excel wherever both would work — it is the route they will use again every time they
  recalibrate. Rule 22's reasoning, applied to spreadsheets.
- **Nothing is ever written into the snapshot.** It is a download, it goes stale the moment the client
  changes anything, and an edit there reaches nothing at all.
