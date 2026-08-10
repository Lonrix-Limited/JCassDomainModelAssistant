# 60 — Get your code back

**Goal of this page:** a fix you made in the browser editor, safely back in the local project on
your own machine.

**Your local project is the source of truth.** A change that exists only on the server is a change
you will lose — the next **Reset source**, the next full re-upload, or simply the next time you
build locally and overwrite it. Bringing it home is not optional tidying; it is the last leg of
the loop.

---

## When you need this

You will fix things in the browser. It is quicker than the local edit → build → package → upload
cycle when the fix is three lines, and it is the only place you can see the failure while it is
happening. That is fine — as long as the fix comes home the same day.

Typical: a null check added while a breakpoint was sitting on the line that threw; a lookup key
corrected against the client's real spreadsheet; a condition inverted after watching it in the
debugger.

---

## Step 1 — Download

On the **Debug Model** page, on the ribbon beside **Upload zip**: **Download source zip**.

It is beside the upload on purpose. The two are one loop.

**You should see** a zip download, named for the client's workspace. It opens **straight to the
`.csproj`**, with no wrapper folder — the exact inverse of what the upload expects, so the round
trip is lossless.

## Step 2 — Compare, do not overwrite

**Unzip it somewhere temporary first.** Do not extract it over your project folder.

Then compare it against your local project — in VS Code, or with whatever diff tool you have — and
copy across only what actually changed. There are usually two or three lines.

> **Extracting over your project is how a day's local work disappears.** The server's copy is
> whatever you last uploaded plus the browser edits; it is not a superset of your local project,
> and anything you changed locally since the upload is not in it.

## Step 3 — Rebuild and re-check

```powershell
dotnet build ..\MyRoadModel\MyRoadModel.csproj -c Debug --no-incremental
.\tools\jcass-dm.exe check --project ..\MyRoadModel
```

**You should see** the same clean result as before. Now the two copies agree again, and the next
upload will not undo the fix.

---

## What is in the zip, and what is not

| In | Out |
|---|---|
| Your `.csproj` | `refs\` — these are the **server's** framework assemblies, and they must never reach a local project |
| Everything under `Objects\` | `bin\`, `obj\` — build output |
| `domain_model_setup.xlsx` | `.git\`, `.vs\`, `node_modules\` |
| Anything else you uploaded | The editor configuration and server state files |

**The editor configuration is withheld deliberately, and this is the part people ask about.** It
carries a live session token for the account that initialised the workspace. Downloading it would
put that token on your laptop, in a folder you might well commit to a repository. It would also
break the trip home: re-uploading a stale copy replaces the current one, and the next F5 fails to
authenticate in a way that looks nothing like *"I re-uploaded my project"*.

**Nothing is lost.** **Initialize workspace** rewrites both files on demand. If somebody reports
that their editor configuration has vanished, the answer is Initialize, not a workaround.

---

## The bundle spreadsheet is a separate download

`domain_model_setup.xlsx` and the other setup files live in the **debug bundle**, and the source
zip carries whatever copy you uploaded — not necessarily the one the server is running.

To get the server's copy, use the ribbon's **second row**: pick the file in **Bundle file**, then
**Download**. Edit it in Excel, then **Upload bundle file(s)** to put it back. Same-named files
are replaced; nothing is deleted.

**Keep the bundle in your local project in step with it.** The bundle and the C# have to agree —
that is most of what `jcass-dm check` checks — and a bundle edited only on the server is a
disagreement waiting to be discovered at publish.

---

## If the download is refused

| What it says | What it means |
|---|---|
| Nothing to download / the folder does not exist | The workspace was never initialised, or nothing was ever uploaded into it. Run **Initialize workspace**, or upload a project first |
| Over the size limit | Bulk data has been parked in the workspace. Everything that normally grows is already excluded, so hitting the cap means something is in the wrong place |

**The size one is a symptom, not a limit to raise.** Data at that scale belongs in the client's
`supporting\` folder — uploaded on **Files → Inputs**, visible on the **Analyse Input** page,
changeable without a republish, and resolving to the same path under a debug run and a normal one.
[`../conventions/naming-and-folders.md`](../conventions/naming-and-folders.md#supporting-versus-the-bundle-for-a-side-car-csv).
Left in the workspace it also bloats every publish.

## Done when

- [ ] The browser-side change is in your local project.
- [ ] It builds and checks clean locally.
- [ ] The bundle spreadsheet in your local project matches the one on the server.
