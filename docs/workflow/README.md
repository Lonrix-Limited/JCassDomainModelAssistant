# docs/workflow/ — the end-to-end path

From an engineer with nothing, to a domain model running in their client's production forecasts,
and back again for the next change.

**These pages are written to be followed by a human.** Numbered steps, what you should see after
each one, and the file every edit lands in. That is deliberate: when someone says *"guide me
through adding a treatment"*, the answer is to walk them through the page that already exists,
not to improvise a lesson that comes out differently every time.

---

## The two ways in

| Where you are | Start at |
|---|---|
| **You are writing a new domain model.** Nothing exists yet. | [`10-scaffold-and-build.md`](10-scaffold-and-build.md) |
| **You have inherited a model somebody else wrote** — "help me refactor the model in folder X", or a model that only exists on the server. | [`05-adopt-an-existing-model.md`](05-adopt-an-existing-model.md) — **and read it before you touch anything** |

Both paths rejoin at [`20-upload-and-debug.md`](20-upload-and-debug.md) and are identical from
there on.

---

## The sequence

| | Page | What it covers |
|---|---|---|
| 00 | [`00-prerequisites.md`](00-prerequisites.md) | What has to be installed and granted first |
| 05 | [`05-adopt-an-existing-model.md`](05-adopt-an-existing-model.md) | The second entry path — diagnose an inherited model |
| 10 | [`10-scaffold-and-build.md`](10-scaffold-and-build.md) | `scaffold --from-sample`, build locally, `check` |
| 20 | [`20-upload-and-debug.md`](20-upload-and-debug.md) | Package, upload, initialise the workspace, F5 with real breakpoints |
| 30 | [`30-make-a-change.md`](30-make-a-change.md) | The four common changes, and every place each one touches |
| 40 | [`40-publish.md`](40-publish.md) | **Overwrites the live model.** Hard rules, not suggestions |
| 50 | [`50-run-the-model.md`](50-run-the-model.md) | Queue a real run and read the result |
| 60 | [`60-get-your-code-back.md`](60-get-your-code-back.md) | Bring a browser-side fix home to the local project |

---

## The walking skeleton — do this before you model anything

**The first thing you build is not your model. It is the pipeline.**

`jcass-dm scaffold MyModel --from-sample` gives you a correctly-named project that already carries
working logic. Take *that* all the way through — build, check, package, upload, F5, publish, run —
before you write a line of your own engineering.

**The reason is diagnostic.** If the first thing you build is your own model and F5 fails, you
cannot tell whether your C# is wrong or your setup is wrong, and you have two unknowns and no way
to separate them. Prove the pipeline first, and from then on every failure is attributable: the
pipeline worked an hour ago, so it is the change you just made.

**There is no throwaway and no rename.** `--from-sample` produces the project you keep. Once the
skeleton is proven you replace the sample's engineering with your own, one file at a time, with a
working build at every step. It never stops being a model that runs.

```
  10  scaffold ──> build ──> check
                                │
  20                            └──> package ──> upload ──> Initialize ──> F5  ← breakpoints work
                                                                            │
  40                                                                        └──> publish
                                                                                  │
  50                                                                              └──> run
                                                                                        │
       ┌────────────────────────────────────────────────────────────────────────────────┘
       │
  30   └──> your own change ──> build ──> check ──> package ──> upload ──> F5 ──> publish ──> run
                                                     └──────────── the loop you stay in ──────┘
```

**First time through, skip step 30.** You come back to it once the skeleton has run end to end.

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
