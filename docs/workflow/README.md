# docs/workflow/ — the end-to-end path

From an engineer with nothing, to a domain model running in their client's production forecasts,
and back again for the next change.

**These pages are written to be followed by a human.** Numbered steps, what you should see after
each one, and the file every edit lands in. That is deliberate: when someone says *"guide me
through adding a treatment"*, the answer is to walk them through the page that already exists,
not to improvise a lesson that comes out differently every time.

**Every command on these pages is typed into a PowerShell terminal**, normally the one sitting in
the `JCassDomainModelAssistant` folder — which is what makes `.\tools\...` and `..\MyRoadModel` mean
what they say. If that sentence is not obvious, read
[`../orientation/running-commands.md`](../orientation/running-commands.md) first; it takes five
minutes and prevents the most common failure there is. Assistants: say which terminal, every time.

---

## The way in

**There is one, and it is not an empty folder.** Lonrix sets up a **starter model** in your client's
Juno Cassandra project, publishes it and runs it once, before you begin. You start from that.

| Where you are | Start at |
|---|---|
| **You are building a domain model for a client.** | [`02-the-starter-model.md`](02-the-starter-model.md) — get the model source and a project snapshot — then [`01-plan-your-model.md`](01-plan-your-model.md) for the engineering, and [`05-adopt-an-existing-model.md`](05-adopt-an-existing-model.md) to diagnose what you were given |
| **You have inherited a model somebody else wrote** — "help me refactor the model in folder X", or a model that only exists on the server. | [`05-adopt-an-existing-model.md`](05-adopt-an-existing-model.md) — **and read it before you touch anything.** The starter model is one of these too |

Both rejoin at [`20-upload-and-debug.md`](20-upload-and-debug.md) and are identical from there on.

**Why there is no "start from scratch".** A custom domain model does not exist on its own — it needs
a client project around it with input data, budget columns, configurations, lookups, a registry entry
and a publish grant, and creating all of that is a Lonrix action. Starting from an empty folder means
writing C# against setup files you have never seen. **If no starter model has been set up for your
client, email `support@lonrix.com`** rather than making one.

---

## The sequence

| | Page | What it covers |
|---|---|---|
| 00 | [`00-prerequisites.md`](00-prerequisites.md) | What has to be installed and granted first |
| 01 | [`01-plan-your-model.md`](01-plan-your-model.md) | The engineering settled on paper before any command — treatments, input columns, parameters, increment and reset rules, grouped thresholds |
| 02 | [`02-the-starter-model.md`](02-the-starter-model.md) | **The way in.** The starter model Lonrix set up, and a snapshot of the client's real setup files |
| 05 | [`05-adopt-an-existing-model.md`](05-adopt-an-existing-model.md) | Diagnose the model you were given, before you change it |
| 10 | [`10-scaffold-and-build.md`](10-scaffold-and-build.md) | The build loop — and `scaffold --from-sample`, which is how Lonrix produces a starter model |
| 20 | [`20-upload-and-debug.md`](20-upload-and-debug.md) | Package, upload, initialise the workspace, F5 with real breakpoints |
| 30 | [`30-make-a-change.md`](30-make-a-change.md) | The four common changes, and every place each one touches |
| 40 | [`40-publish.md`](40-publish.md) | **Overwrites the live model.** Hard rules, not suggestions |
| 50 | [`50-run-the-model.md`](50-run-the-model.md) | Queue a real run and read the result |
| 60 | [`60-get-your-code-back.md`](60-get-your-code-back.md) | Bring a browser-side fix home to the local project |

---

## The walking skeleton — you are handed one, already proven

**The first thing built is not your model. It is the pipeline** — and on this path somebody else has
already built it, which is the whole reason the starter model exists.

The starter model is a small, complete, working domain model that has been **published and run in
your client's own project** before you saw it. That run is what proves the input files, the budget
columns and the configurations are real and agree with each other.

**The reason is diagnostic.** If the first thing you build is your own model and F5 fails, you cannot
tell whether your C# is wrong or your setup is wrong — two unknowns and no way to separate them. With
the pipeline already proven, every failure from that point is attributable: it worked an hour ago, so
it is the change you just made.

**There is no throwaway and no rename to a different project.** The starter model *is* the project
you keep. You rename it to your client's model name once — with `jcass-dm rename`, all four names at
a time — and then replace its engineering with your own, one file at a time, with a working build at
every step. It never stops being a model that runs.

> **Lonrix produces a starter model with `jcass-dm scaffold MyModel --from-sample`**, which emits a
> correctly-named project already carrying the reference model's working logic.
> [`10-scaffold-and-build.md`](10-scaffold-and-build.md) documents it. If you are building a domain
> model with no Juno Cassandra client project behind it, that command is your starting point too.

```
  02  starter model + snapshot ──> rename
                                │
  10                            └──> build ──> check
                                                  │
  20                                              └──> package ──> upload ──> Initialize ──> F5  ← breakpoints work
                                                                                              │
  40                                                                                          └──> publish
                                                                                                    │
  50                                                                                                └──> run
                                                                                                          │
       ┌──────────────────────────────────────────────────────────────────────────────────────────────────┘
       │
  30   └──> your own change ──> build ──> check ──> package ──> upload ──> F5 ──> publish ──> run
                                                     └──────────── the loop you stay in ──────┘
```

**First time through, stop at step 20.** Rename the starter model, build it, package it and take it
as far as **F5** — and stop there. Steps 40 and 50 are for a change you actually mean to put live;
the client's model is already published and already running, so a rehearsal publish overwrites it
([`40-publish.md`](40-publish.md#-before-a-first-publish-on-a-client-that-already-runs-a-custom-model)).
Step 30 is where you go next.

**Note the order: F5 does not require a publish.** You debug the code sitting in your debug
workspace, not the published model. Publishing before you have debugged puts unverified code into
the client's production runs, and that is exactly backwards.

---

## Before you say a model is finished

- [`../conventions/silent-failures.md`](../conventions/silent-failures.md) — the things that go
  wrong without an error. Four of them nothing can detect for you.
- The web app's **Check Setup** on the Tuning page is authoritative. `jcass-dm check` is a local
  subset and says so in its own output.

## When something is not covered here

Stop rather than guess: [`../conventions/when-to-stop.md`](../conventions/when-to-stop.md).
A failure these pages do not describe is a stop condition, not an invitation to diagnose by
inspection.
