# Updating the Assistant

**When a newer Assistant is released you download it and replace this folder.** That is the whole
mechanism — no git, no merge, no partial update. This page is the ten minutes of care that makes it
safe.

Read it before you download, not after.

---

## The one thing that actually matters

> ## Unpack the new folder to **exactly** the same path the old one is at.
>
> Same drive, same parent folder, same spelling, same `-main` on the end — or the same *absence* of
> `-main`, if you renamed it last time. Character for character.

**This is what preserves your assistant's memory of your project.** Every conversation you have ever
had with your assistant about this work is stored under the **absolute path of the folder it was
open in**. Unpack somewhere else — a different drive, `Downloads` instead of `Work`, or keeping the
`-main` when last time you took it off — and every one of those conversations is orphaned. They are
still on your disk. No session will ever find them again.

**And nothing tells you.** There is no warning and no error. Your assistant simply behaves as though
it has never met you: it re-asks what your lookup sets are called, what your budget categories are,
what you decided about rut depth six weeks ago. It looks like the new version being worse, and it is
not.

That is the reason this page exists. Everything below is comparatively minor.

---

## Before you start

**Do you actually need the new one?** Check what you are holding against what is on GitHub:

| Where | What it tells you |
|---|---|
| [`../../ASSISTANT-VERSION.txt`](../../ASSISTANT-VERSION.txt) at the root of this folder | The release date and commit of the Assistant you have now |
| [`../../CHANGELOG.md`](../../CHANGELOG.md) | What changed in each release, and **what to re-check in your model** because of it |

Also worth two seconds — is your framework reference actually behind?

```powershell
.\scripts\check-framework-version.ps1
```

---

## The procedure

Nine steps. Steps 0 and 8 are the ones people skip, and they are the two that keep you from
deleting something of yours.

### 0. Open your current Assistant folder and look inside it

**Look. Do not move anything yet.** You are making a list, and there are two reasons this is a
separate step from moving:

- VS Code still has this folder open, and **Windows will not let you rename or move a folder that
  something has open.** You would get *"The process cannot access the file because it is being used
  by another process"* half-way through.
- If you move things now, step 2 reads as a repeat and one of the two gets skipped.

**What you are looking for is anything of yours.** In order of how badly it would be missed:

| Look for | Why |
|---|---|
| **A model folder sitting inside the Assistant** — a folder with a `.csproj` in it | The serious one. Your model should be *beside* the Assistant, never inside it, but if it ended up in here then deleting the old Assistant later would delete your model with it |
| **`model-knowledge\`** | What your assistant has learned about your models. It is inside the folder you are about to replace, and nothing else carries it across |
| **Loose notes** — a `.txt` or `.md` at the root that you or your assistant wrote | |
| **A file you edited** — most often `CLAUDE.md` at the root, if you ever used the `#` shortcut to tell your assistant to remember something | Saved for reference, **not** copied back over the new one. Step 4 says what to do with it |

**Ignore `bin\` and `obj\`.** Those are build output and there will be a great many of them. They
are not yours, nothing is lost by leaving them, and an engineer who tries to preserve "everything
that is not in the release" ends up carrying a hundred and forty files of build noise across. If it
is not something you or your assistant wrote, leave it.

**If the list is empty, that is a perfectly normal answer** and it is the most likely one on a first
update. Your model is next door where it belongs and there is nothing in here to rescue. The path
instruction above is still the important part.

### 1. Close VS Code

All of it, not just the folder. And **check no terminal is sitting inside the folder** — if you have
a PowerShell window open somewhere with this folder as its current directory, that alone is enough
to block the rename in step 3.

### 2. Now move what you listed

- **A model folder found inside the Assistant goes *beside* it, not into your safe copy.** Beside is
  where it belongs, and it stays there — it does not come back in at step 4. Move
  `JCassDomainModelAssistant\MyRoadModel` up one level, so that it sits next to
  `JCassDomainModelAssistant` rather than inside it. That is the layout everything else assumes:
  [`../conventions/naming-and-folders.md`](../conventions/naming-and-folders.md).
- **Everything else** — `model-knowledge\`, notes, an edited file — copy somewhere safe for the
  moment. Your Desktop is fine; it is coming back in twenty minutes.

If step 0 found nothing, there is nothing to do here. Move on.

### 3. Rename the old folder, and unpack the new one to the identical path

Rename your current folder to `JCassDomainModelAssistant-old`. Do not delete it — step 8 is what
deletes it, after you have checked.

Then download the new ZIP and unpack it **so the resulting folder path is exactly what the old one
was.**

**GitHub always names the unpacked folder `JCassDomainModelAssistant-main`.** So:

| Your old folder was called | Rename the newly unpacked one to |
|---|---|
| `JCassDomainModelAssistant-main` | leave it — it already matches |
| `JCassDomainModelAssistant` | `JCassDomainModelAssistant` — take the `-main` off |

And it goes in the **same parent folder** it was in. `C:\Work\JCassDomainModelAssistant` is not the
same path as `C:\Users\you\Downloads\JCassDomainModelAssistant`, and it is not the same path as
`D:\Work\JCassDomainModelAssistant`.

If you are unsure what the old path was, you can still read it: the `-old` folder is right there.
Open it in File Explorer and copy the path out of the address bar.

### 4. Copy your notes back in

`model-knowledge\` and your loose notes from step 2 go back into the new folder.

**Your model folder does not.** It is beside the Assistant now, and that is where it stays.

**And a file you edited that also exists in the new release does not go back either — most often
`CLAUDE.md` at the root.** Copying your old one over the new one puts the new release's instructions
back to the old ones, and nothing tells you: the file looks exactly like a `CLAUDE.md` should.
Open your saved copy, find the part *you* added, and paste that into
`model-knowledge\<YourModelName>.md` instead. That is where a note about your model belongs anyway,
and it is the one place an update carries across.

### 5. Refresh your model's framework references

In a PowerShell terminal in the **new** Assistant folder — **Terminal → New Terminal** in VS Code if
you do not have one:

```powershell
.\scripts\refresh-model-refs.ps1 -Project ..\MyRoadModel
```

Use your own model's folder name in place of `MyRoadModel`.

**Do not skip this, and do not skip it because your model is building fine.** Your model has its own
private copy of the framework reference assemblies, made when it was scaffolded. A newer Assistant
brings a newer framework in *its* `refs\` folder and does nothing whatsoever to your model's copy.
The result builds perfectly and is compiling against a framework older than the one this Assistant's
documentation describes — which is the same silent-staleness problem this step exists to close.
Details: [`../../scripts/README.md`](../../scripts/README.md).

Then rebuild, so that anything the new framework changed shows up here as a build error rather than
as odd behaviour in the web app:

```powershell
dotnet build ..\MyRoadModel\MyRoadModel.csproj -c Debug --no-incremental
```

### 6. Reopen the workspace and re-add your model

Double-click [`../../assistant.code-workspace`](../../assistant.code-workspace) in the new folder,
then **File → Add Folder to Workspace…** and pick your model folder. Say yes if VS Code offers to
save the workspace.

### 7. Read what to re-check, and let your assistant do it

[`../../CHANGELOG.md`](../../CHANGELOG.md) has a **"what to re-check in your model"** section for
each release. It exists because `jcass-dm check` catches only what is mechanically checkable — a new
*guidance* rule is prose, and nothing else will surface it for you.

Then ask your assistant to go over your model against the new release:

> *"There is a newer Assistant. Read the changelog's 'what to re-check in your model' section, run
> `jcass-dm check` on my model, and tell me what you would change."*

### 8. Look in the `-old` folder one more time — then delete it

**Open it and go through it before you delete anything.** You looked at step 0, but you were reading
a list of folder names then and you know more now.

Two questions:

- **Is there a folder with a `.csproj` in it?** If so, that is a model and it must not be deleted.
  Move it out beside the new Assistant.
- **Is there anything else you or your assistant wrote?** Copy it across.

Only when the answer to both is no: delete `JCassDomainModelAssistant-old`.

**If you are not sure, do not delete it.** It costs a few hundred megabytes of disk and nothing else.
Leaving it is also what makes the *next* update easier — your assistant can find `model-knowledge\`
in it and tell you the exact folder to copy from.

---

## Why re-download at all, rather than `git pull`

Because you should not have to know what git is to maintain a domain model. Improvements arrive as a
new download and you replace one folder; there is no branch, no merge, and no conflict to resolve.

**And your model is never in the folder that gets replaced.** It lives in its own folder beside the
Assistant, which is the reason the whole arrangement is safe —
[`../conventions/naming-and-folders.md`](../conventions/naming-and-folders.md).

**What is *not* true is that the Assistant holds nothing of yours.** It holds two things, and this
page exists because both of them are easy to lose:

| What | Where it lives | What carries it across |
|---|---|---|
| Your assistant's notes about your models | `model-knowledge\` **inside** the Assistant folder | Step 2 and step 4 — you copy it |
| Your assistant's memory of every conversation | **Outside** the folder, filed under the folder's absolute path | **Step 3** — unpacking to the identical path, and nothing else |

The second is the one you cannot recover from. If you copy no files at all but get the path right,
you lose very little. Get the path wrong and no amount of copying helps.

---

## For the assistant

**The engineer is a civil engineer, and this procedure is one they will do perhaps twice a year.**
Walk them through it a step at a time and wait for each one, as with any guided task
([`../00-start-here.md`](../00-start-here.md) § 2). Three things worth holding on to:

- **Lead with the path, not with the copying.** Step 3 is what the page is for. If they are short of
  time, *"unpack it to exactly the same path"* is the instruction that has to land.
- **Step 0 is a look, not a move**, and it must stay that way — VS Code has the folder open at that
  point and a rename will fail outright. Do not helpfully merge it into step 2.
- **A model folder found inside the Assistant is relocated, not round-tripped.** It goes beside the
  Assistant at step 2 and it stays there. Copying it back in would re-arm the same trap for the next
  update, and would break step 5, whose `..\MyRoadModel` looks *beside* the Assistant.

**After the update, the first thing to check is `model-knowledge\`.** If they ask you about a model
and there is no file for it, look for a sibling folder ending `-old` or `-main` that has
`model-knowledge` in it and name that exact folder — [`../00-start-here.md`](../00-start-here.md)
§ 4.
