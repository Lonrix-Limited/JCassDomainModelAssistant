---
name: new-domain-model
description: Start a brand-new Juno Cassandra domain model. Walks the engineer through choosing the one name, runs jcass-dm scaffold, sets the bundle's meta sheet, and confirms the model builds and checks clean. Use when they say "new model", "start a model", "scaffold a model", or have nothing yet.
---

# New domain model

**This skill is a wrapper.** Every step is a page in `docs/` plus a `jcass-dm` verb. Without it,
do the same job by reading [`docs/workflow/10-scaffold-and-build.md`](../../../docs/workflow/10-scaffold-and-build.md)
and running the verbs it names.

## 0. Before the first step

- **Honour the verb.** "Guide me through starting a model" and "scaffold me a model called X" are
  different requests — [`docs/00-start-here.md` § 2](../../../docs/00-start-here.md). In guided
  mode, walk them through `workflow/10-scaffold-and-build.md` one step at a time and let *them*
  type the commands. This skill is not a way to skip that.
- **Stop conditions apply throughout** — [`docs/conventions/when-to-stop.md`](../../../docs/conventions/when-to-stop.md).
  If a step here cannot be completed as written, that is a stop, not licence to find another route.
  Use the `draft-support-request` skill.

## 1. Read

- [`docs/workflow/01-plan-your-model.md`](../../../docs/workflow/01-plan-your-model.md) — **the step before this one.** Four engineering questions, walked one at a time, before any command is typed.
- [`docs/workflow/10-scaffold-and-build.md`](../../../docs/workflow/10-scaffold-and-build.md) — the procedure this skill runs.
- [`docs/workflow/README.md`](../../../docs/workflow/README.md) — why the walking skeleton comes before any modelling.
- [`docs/conventions/four-names.md`](../../../docs/conventions/four-names.md) — what the name they choose becomes.
- [`docs/workflow/00-prerequisites.md`](../../../docs/workflow/00-prerequisites.md) — if the model does not yet exist in the web app, or they do not hold the project lock.

## 2. Plan before you scaffold, then ask, do not decide

**Walk [`docs/workflow/01-plan-your-model.md`](../../../docs/workflow/01-plan-your-model.md) first,
one guideline at a time.** Start simple; the three lists (treatments, input columns, parameters);
increment and reset rules per parameter; grouped thresholds and constants. Wait for each answer —
all four in one message produces agreement and no lists. The answers are the engineer's; you are
helping them see the shape, catching the input column that is really a parameter, and recognising a
fitted coefficient set the moment it appears.

Then three answers are the engineer's:

| Ask | Why it is not yours |
|---|---|
| **The model name** | It becomes all four names and it is the model's identity for life. Constraints and how to change it later: `conventions/four-names.md`. |
| **The element noun** (`--element`) — `RoadSegment`, `PipeSegment`, `Bridge` | It names the thing being modelled. Not one of the four; theirs to choose. |
| **Whether the client already runs a custom domain model** | If yes, this is really the adoption case — stop and use `adopt-existing-model`, and read the takeover warning in [`docs/workflow/40-publish.md`](../../../docs/workflow/40-publish.md#-before-a-first-publish-on-a-client-that-already-runs-a-custom-model) before publish is mentioned at all. |

Default to `--from-sample` unless they say otherwise, and say why in one sentence
(`workflow/README.md`, the walking skeleton).

## 3. Say where it will land, then run

**Before the command, resolve the path out loud and get a yes.** `--output ..\<Name>` is relative
to the terminal's folder, which the engineer cannot see, and folder layout is what this audience
finds confusing:

> This will create `C:\Work\<Name>`, beside the Assistant folder — not inside it. Can you create
> files in `C:\Work`? If you are not sure: open it in File Explorer, right-click → New → Text
> Document, then delete it.

Name the terminal too, every time — *"in your PowerShell terminal, the one already open in the
Assistant folder"*. [`docs/orientation/running-commands.md`](../../../docs/orientation/running-commands.md).

```powershell
.\tools\jcass-dm.exe scaffold <Name> --from-sample --element <Noun> --output ..\<Name>
```

`--output` puts the project **beside** this repository. Read the tool's own `Next:` block back to
the engineer — it lists the four names and the lookup sets the model will need — and read back the
`Scaffolded ... at ...` path so they can confirm it is where they expected.

If it exits with *"Cannot create files in ..."*, that is a Windows permission on their folder, not
a fault in the tool or the model. Move to a folder they own and run it again.

`scaffold` has already written the meta sheet — both of the four names that live there, and the
display name. **Only ask about the display name**, which is what the modeller sees in the web app
and is the one meta value that may reasonably differ from the model name:

```powershell
.\tools\jcass-dm.exe set-meta ..\<Name>\domain_model_setup.xlsx --display-name "<what the modeller sees>" --force
```

`--force` is needed because scaffold already wrote a value there; without it the tool prints the
difference and exits `3` rather than overwriting — [`tools/README.md`](../../../tools/README.md)
§ Exit codes. Read the difference before forcing it.

**Never use `set-meta` to "fix" `main_dll` or `main_class`.** Those are two of the four names;
`rename` is the only correct way to change any of them.

## 4. Confirm it builds and checks

```powershell
dotnet build ..\<Name>\<Name>.csproj -c Debug --no-incremental
.\tools\jcass-dm.exe check --project ..\<Name>
```

A warning is a failure — `workflow/10-scaffold-and-build.md` § step 3. If they have the client's
`inputs\lookups.xlsx`, re-run `check` with `--lookups` so the last rule stops being skipped. Reading
the result is the `check-my-model` skill.

## 5. Never

- **Never edit any of the four names by hand**, in the `.csproj`, the C# or the bundle. `rename`.
- **Never write a threshold, rate or factor into C#** — even one the engineer just told you.
  [`docs/conventions/where-numbers-live.md`](../../../docs/conventions/where-numbers-live.md).
  A scaffolded project has no numeric literal in its trigger; keep it that way.
- **Never publish**, and do not raise publishing as the next step. Next is
  [`docs/workflow/20-upload-and-debug.md`](../../../docs/workflow/20-upload-and-debug.md).

## 6. Done when

`workflow/10-scaffold-and-build.md` § Done when. Then hand off to
[`20-upload-and-debug.md`](../../../docs/workflow/20-upload-and-debug.md), or to the
`package-for-upload` skill.
