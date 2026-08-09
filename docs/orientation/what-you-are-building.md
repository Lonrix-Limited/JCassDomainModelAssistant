# What you are building

**A domain model: one compiled DLL and one spreadsheet, which together tell Juno Cassandra how a
particular kind of asset behaves.**

The split between the framework and the domain model — what each owns, and the four questions your
code answers once per element per period — is set out in
[`DomainModelSample/README.md` § 1](../../reference-model/DomainModelSample/README.md#1-what-the-framework-does-and-what-you-do),
and at more length in
[`../framework/concepts/02-framework-model.md`](../framework/concepts/02-framework-model.md). Read
one of those first if the division is not yet clear.

This page is about the rest of it: what the thing you produce *is*, where it ends up, and what that
means for how carefully you work.

---

## A Custom Domain Model is the client's own, and it has one version

A **Custom Domain Model** — CDM — is owned by one client. Their own modeller writes it, debugs it on
the server with real breakpoints, and publishes it, with no step at Lonrix in between.

**It is deliberately single-version.** There is no v1 and v2 sitting side by side. Publishing
replaces what every subsequent run for that client loads. There is one rollback slot, and using it
is an intervention rather than a button.

Two consequences shape how you work.

**Publishing is safe from you, by design.** There is no command-line publish path, and publishing
needs a grant the modeller cannot give themselves. The worst an over-eager assistant can do is write
bad code in a folder on somebody's laptop. Treat that as a designed property and do not look for a
way around it — a run that reaches production is always a human decision.

**Publishing is not safe from *them*, if they have inherited a live model.** See below.

---

## Two entry paths, and they start differently

### A new model — the primary case

Nothing is running yet, so nothing can be broken. The engineer scaffolds, and the first thing they
do is prove the whole pipeline works before writing any engineering of their own:

```powershell
.\tools\jcass-dm.exe scaffold MyRoadModel --from-sample --output ..\MyRoadModel
```

`--from-sample` produces a **working** model under their own name — it builds, it uploads, it F5s,
it publishes, it runs. That artefact is the one they keep. They then replace sample logic with their
own engineering file by file, with a working build at every step.

**Nobody starts by copying the reference model and renaming it.** That was the old advice, and
renaming is the most reliable way to break a model — see
[`../conventions/four-names.md`](../conventions/four-names.md). The reference model exists to be
*read*.

### A model they have inherited — first-class, not an afterthought

*"Help me refactor the CDM in folder X"* is a normal request and differs in four ways:

1. **`check` is the first thing you run**, not a late one. It tells you what state the model is in
   before you form an opinion about it.
2. **It may already violate the four-name rule.** `jcass-dm rename` fixes all four atomically.
3. **They may have no local copy at all.** The web app offers a source download, and that is a
   legitimate entry route.
4. **It is by definition the takeover case.** Something is in production right now.

**Warn them before the publish, in plain words.** Publishing overwrites the model that is currently
producing their forecasts. Not "be careful" — say what happens: *"this replaces the model your
production runs use, immediately, and there is one rollback slot."*

---

## Where the work happens

**You author locally and you run on the server.** The framework assemblies shipped in `refs\` are
*reference assemblies* — the full public API with no method bodies. They compile, they give full
IntelliSense, and they deliberately cannot be executed. Anything that tries to run the framework on
the engineer's machine compiles cleanly and then fails at load.

That is expected, not a fault. Running and debugging happen on the web app's **Debug Model** page,
where a real debugger attaches and breakpoints in their source fire mid-run.

Say this early if the engineer seems to be trying to run the model locally, because the failure
message does not explain itself.

---

## What you do, and what you never do

**You do the plumbing.** Wiring files together. Keeping names matched across the C#, the bundle and
the lookups. Scaffolding the place a number goes. Reading errors. Explaining what a stage does.

**You never supply the engineering judgement.** Deterioration rates, trigger thresholds, unit rates,
treatment effectiveness — those are the engineer's, and they are what they are paid for. Ask, and
then put the answer where a modeller can change it:
[`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md).

**And you work from canonical guidance, never invention.** The boundary, and what to do at it, is
[`../conventions/when-to-stop.md`](../conventions/when-to-stop.md).
