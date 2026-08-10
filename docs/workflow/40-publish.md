# 40 — Publish

**Publishing overwrites the domain model this client runs.** There is no "publish as a new
version" — a custom domain model has exactly one version, and the current build is replaced in
place. Every run started after a publish uses the new one.

This is the only step in the workflow that reaches outside your own workspace. Read the whole page
before you press anything.

---

## Rules for an AI assistant, and they are not suggestions

**1. Never publish unless the user asks for it explicitly, in that turn.**

Not as the natural conclusion of *"help me add a treatment"*. Not because the change is finished
and publishing is obviously what comes next. Not because they published the last three times.
Finishing a change and putting it into production are two decisions, and only one of them was
delegated to you.

**2. Always run Check bundle first, and refuse if it reports failures.**

If the report has failures, say what they are and stop. Do not publish and mention the failures
afterwards. Do not offer to publish anyway.

**3. Say what it does, in the same message as the offer.**

"This will replace the model the client's forecasts currently run on." Once, plainly, in the
message where they can still say no.

**4. If the Publish button is missing or greyed out, that is a permission, not a puzzle.**

Read the reason off the page (below), tell them what it means, and — where the fix is a grant —
tell them to contact **support@lonrix.com**. Do not look for another route. There is no
command-line publish and you cannot make one.

---

## Before you publish

- [ ] The model has been through **F5** and the run reached `[DebugMode] Success`
      ([`20-upload-and-debug.md`](20-upload-and-debug.md)). **Debugging does not require a
      publish, and publishing before you have debugged puts unverified code into production.**
- [ ] **Check bundle** reports no failures.
- [ ] `jcass-dm check` is clean locally.
- [ ] You hold the project lock.
- [ ] Nobody has a run queued or in progress on this model.

---

## ⚠ Before a *first* publish on a client that already runs a custom model

**Stop and ask, out loud, in the conversation:**

> Does this client already have a working custom domain model in production?

If the answer is yes, or if you do not know, **do not publish as part of learning the workflow.**

Here is the situation this exists for. The walking skeleton
([`README.md`](README.md#the-walking-skeleton--do-this-before-you-model-anything)) says to prove
the whole pipeline before doing any of your own modelling, and that includes a publish. On a
brand-new model that is harmless: there is nothing live to lose. **On a client that is already
running a custom model written by somebody else, that same practice publish replaces their
production model with a sample.** Their next forecast runs the sample's logic. Nothing warns
anybody, because as far as the system is concerned you did exactly what you asked to do.

You are in this situation whenever the model came from somewhere other than
`jcass-dm scaffold` — you inherited a folder, you downloaded the source from the server, somebody
handed you a zip, or the client has been running forecasts for months and you are the new
modeller. See [`05-adopt-an-existing-model.md`](05-adopt-an-existing-model.md).

**What to do instead:** prove the pipeline as far as F5 and stop there. F5 runs your code against
the client's real data with real breakpoints and changes nothing outside the debug workspace.
That is the whole diagnostic value of the walking skeleton, and none of it needs a publish.
Publish only when you have a change you actually intend to put live.

---

## The procedure

1. **Debug Model page → the ribbon's third row.** The **Publish to live model** button is red, and
   it is disabled until the server says every gate is clear. Beside it is a refresh button that
   re-checks; beside that, the model and version it would write to — **read that line and confirm
   it is the client and model you think it is.**

2. If it is disabled, an amber **Publish is disabled** strip lists every reason. See
   [below](#why-publish-is-disabled).

3. Click **Publish to live model**. A confirmation appears — *"Publish over the live model?"* —
   naming the client, the domain model and the version, and stating that the version is unchanged
   because there is only one. **Cancel** is focused, deliberately; a stray Enter does not commit
   the overwrite.

4. Click **Publish and overwrite**.

5. **You should see** progress, then a result panel. The first compile can take a few minutes.

**You should see** on success:

> Published 14 file(s) to MyRoadModel v1.0. Every run of this client now loads this build.

### What publishing actually does

1. Compiles the C# in your debug workspace **on the server**. This is a compile only — **it does
   not start a model run and produces no results.**
2. The compiled files, plus everything currently in the debug bundle folder, replace the contents
   of the live version folder.
3. The version being replaced is kept in a **single rollback slot** — one publish back. The next
   publish overwrites it.

> **That rollback slot is not a button you have.** Restoring from it is a manual operation on the
> server by Juno, and the second publish destroys the first rollback point. Treat it as the thing
> that stops a bad publish being a catastrophe, not as an undo.
>
> If you need one: **support@lonrix.com**, with the client name, the model name and roughly when
> the bad publish happened. Draft it for the engineer using
> [`../support-request-template.md`](../support-request-template.md).

### Anything loose in the debug bundle folder ships

Publish takes **everything** in `debug_domain_model/`, not just the files it recognises. A probe
CSV or a scratch spreadsheet left there becomes part of the client's live model. The confirmation
reports the file count and the result names it — if the number is larger than you expect, that is
why. **Reset debug bundle** clears it back to the seeded state.

---

## Why publish is disabled

The strip lists the server's own reasons, in its own words. What each one means:

| What it says | What it means | What to do |
|---|---|---|
| *"You must hold this project's lock to publish."* | The project is **unlocked**. "Nobody is blocking me" is not the same as "this is mine to change" | **Project Home → Lock project** |
| *"Project locked by …"* / a 423 | Somebody else holds the lock | They release it. An administrator can take it over |
| *"An administrator has not granted you publish rights for this client."* | A per-user grant is missing, and you cannot give it to yourself | **support@lonrix.com** — ask for publish rights on the client. This is by design, not an oversight |
| *"Publishing a domain model is available to modellers and admins only."* | Your role does not include publishing | Nothing to fix on your side |
| *"N run(s) are queued or running against this model version."* | A forecast is in progress against the model you would overwrite | Wait for it, or cancel it on the **Run Model** page |
| *"debug_domain_model/ has no domain_model_setup.xlsx."* | The workspace was never initialised, or the bundle was reset and not re-seeded | **Initialize workspace** |
| Anything about the picked version being deprecated or not current | You have picked the wrong row in **Model version** | A custom model has exactly one version — pick it |
| The model is a shared Lonrix model, or belongs to another client | You cannot publish into a shared model, by design | **support@lonrix.com** — a custom model has to be created for this client |

**The publish grant is checked against live client access**, so losing access to a client removes
the publish right with it. There is nowhere else it could still be enabled.

---

## When publish fails

**A compile error.** The message says the domain model did not compile and the compiler output
stays on screen. This is yours to fix: read it in
[`../orientation/reading-errors.md`](../orientation/reading-errors.md), fix it, build in the
editor terminal until it is clean, and publish again.

Note the asymmetry that catches people: **the server compiles in Release, and your F5 built in
Debug.** A model that runs under F5 and refuses to publish is nearly always a warning-as-error or
a nullability difference, not a mystery.

### The refusal that catches people

> The bundle declares main_dll 'X' but the Release build produced 'Y'

**Working as designed, and the message is the fix.** Your bundle's `meta.main_dll` must name the
assembly your project actually builds. Publish refuses on a mismatch and **never rewrites your
spreadsheet** — silently editing the file you consider authoritative would be found out later, by
you, with nothing to explain it.

The confusing part is that **F5 does not care.** A debug run binds to the freshly-built assembly
and ignores `meta.main_dll` on purpose, because under active debugging your source has moved on
from the published assembly name. So the mismatch can sit there through a hundred green debug runs
and first surface here.

Fix it properly rather than by editing one of the four names:

```powershell
.\tools\jcass-dm.exe check --project ..\MyRoadModel
```

The `the four names` rule reports exactly which of the four disagrees. If a rename is what you
want, `jcass-dm rename` changes all four at once — never edit them individually.
[`../conventions/four-names.md`](../conventions/four-names.md).

### Anything else

A publish failure that is not on this page is a stop condition. Do not diagnose it by inspection —
draft a support request with the exact message, the framework version stamp from
[`../../refs/FRAMEWORK-VERSION.txt`](../../refs/FRAMEWORK-VERSION.txt), the client and the model
name. [`../conventions/when-to-stop.md`](../conventions/when-to-stop.md) and
[`../support-request-template.md`](../support-request-template.md).

---

## Why an assistant cannot do this to you by accident

**There is no command-line publish path and there will not be one.** Publishing is a browser
button, behind a grant an administrator gives and a modeller cannot give themselves. An AI
assistant working in your editor can write bad C# — that is what `dotnet build`, `jcass-dm check`
and F5 are for — but it physically cannot put anything into production. That is a designed
property of the system, not a policy.

## Done when

- [ ] The result panel names the model and version you intended.
- [ ] The file count is what you expected.

Next: [`50-run-the-model.md`](50-run-the-model.md).
