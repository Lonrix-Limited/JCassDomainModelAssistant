---
name: package-for-upload
description: Build the upload zip for the web Debug Model page, after the checks that should run first. Use for "package my model", "make the zip", "I want to upload it", "get it ready for debugging".
---

# Package for upload

**This skill is a wrapper.** One verb, and the two checks that come before it. Without it, run
`jcass-dm package` and read
[`docs/workflow/20-upload-and-debug.md`](../../../docs/workflow/20-upload-and-debug.md).

## 0. Before the first step

- **Honour the verb** — [`docs/00-start-here.md` § 2](../../../docs/00-start-here.md).
- **Packaging is not publishing.** The zip goes into a debug workspace and changes nothing the
  client runs. Do not offer publishing as the next step —
  [`docs/workflow/40-publish.md`](../../../docs/workflow/40-publish.md).

## 1. Read

- [`docs/workflow/20-upload-and-debug.md`](../../../docs/workflow/20-upload-and-debug.md) — where the zip goes and what happens to it.
- [`docs/conventions/naming-and-folders.md` § the two zips](../../../docs/conventions/naming-and-folders.md#part-3--the-two-zips) — the two things about this zip that are load-bearing.

## 2. Run the checks first

```powershell
dotnet build ..\MyRoadModel\MyRoadModel.csproj -c Debug --no-incremental
.\tools\jcass-dm.exe check --project ..\MyRoadModel --lookups ..\lookups.xlsx
```

A warning is a failure. **If `check` reports a failure, say what it is and stop** — packaging a
model with a known inconsistency spends an upload and a wait to find out what you already knew. The
`check-my-model` skill reads the output.

## 3. Package

```powershell
.\tools\jcass-dm.exe package --project ..\MyRoadModel
```

Re-running refuses to overwrite an existing zip; `--force` when the new one is wanted. Read the
tool's own output back — it lists what went in and what was left out.

## 4. Do not build the zip by hand

Two properties are load-bearing and both are easy to get wrong in File Explorer: it must open
straight to the `.csproj` rather than to a folder containing it, and it must not contain `refs\`.
The second surfaces as a `BadImageFormatException` that looks like nothing to do with a zip —
[`docs/conventions/silent-failures.md` § 8](../../../docs/conventions/silent-failures.md#8-a-refs-folder-inside-the-upload-zip).

If `package` refuses — two `.csproj` files at the root, for instance — that is the finding. Fix the
cause; do not assemble the zip another way.

## 5. Then

The upload, **Initialize workspace**, the server build, F5, and **Check bundle** are all on
[`docs/workflow/20-upload-and-debug.md`](../../../docs/workflow/20-upload-and-debug.md), from step 3.
Those are browser steps: walk the engineer through them, one at a time.

## 6. Never

- **Never hand-assemble the zip** — § 4.
- **Never package past a failing check** — § 2.
- **Never publish.** There is no command-line publish path and you cannot make one.
