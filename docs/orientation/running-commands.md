# Running the commands — the terminal, and which folder you are in

Almost every step in [`../workflow/`](../workflow/README.md) has a command in it, and every one of
those commands is typed into a **PowerShell terminal**. If you have never used one, this page is the
whole of what you need. It takes five minutes and it removes the single most common way these steps
go wrong: **the command was right and the terminal was pointing at the wrong folder.**

---

## 1. Open a PowerShell terminal in VS Code

**Terminal → New Terminal**, from the menu at the top. A panel opens at the bottom of the window
with a prompt in it that looks something like this:

```
PS C:\Work\JCassDomainModelAssistant>
```

That is a PowerShell terminal. **Leave it open for the whole session** — you do not need a new one
per command, and opening a second one is a common way to end up in the wrong folder without
noticing.

To run a command: click into that panel, paste the command, press **Enter**. To copy the output back
to your assistant, select it with the mouse and press **Ctrl+C**.

## 2. The text before the `>` is the folder you are in, and it matters

```
PS C:\Work\JCassDomainModelAssistant>
   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
   this is the current folder
```

**Commands in these pages are written relative to that folder**, and two pieces of punctuation carry
all the weight:

| In a command | Means | Example |
|---|---|---|
| `.\` | *in the folder I am in* | `.\tools\jcass-dm.exe` — the tool inside the Assistant folder |
| `..\` | *in the folder next door* | `..\MyRoadModel` — your model, beside the Assistant |

So `.\tools\jcass-dm.exe scaffold MyRoadModel --output ..\MyRoadModel` only does what it says when
the terminal is sitting in the **Assistant folder**. Run the same line from somewhere else and it
either fails with *"The term '.\tools\jcass-dm.exe' is not recognized"*, or — worse — it works and
puts your model somewhere you did not expect.

**To check where you are:**

```powershell
pwd
```

**To move:**

```powershell
cd C:\Work\JCassDomainModelAssistant
```

Use the real path on your machine. Copy it from the address bar in File Explorer if you are not
sure, and keep the quotes if it contains a space: `cd "C:\My Work\JCassDomainModelAssistant"`.

## 3. The two folders, and how they sit

There are exactly two folders in play, side by side:

```
C:\Work\                                 <- pick this yourself. Anywhere you can write.
    JCassDomainModelAssistant\           <- the Assistant. Replaced wholesale on every update.
        tools\jcass-dm.exe               <- the tool the commands call
        docs\                            <- these pages
    MyRoadModel\                         <- YOUR model. Never touched by an update.
        MyRoadModel.csproj
        Objects\
```

**Your model is a sibling of the Assistant, never inside it.** That is what makes it safe to throw
the Assistant away and download a newer one — full reasoning in
[`prerequisites.md`](prerequisites.md) and the layout in
[`../conventions/naming-and-folders.md`](../conventions/naming-and-folders.md).

The terminal normally sits in `JCassDomainModelAssistant\` and reaches sideways into the model with
`..\MyRoadModel`. That is why nearly every command on these pages starts either `.\tools\` or
`dotnet build ..\`.

## 4. You need write permission in both folders

**Before you start**, make sure the folder you have chosen — `C:\Work\` above — is one your Windows
account can **create and change files in**. A company-managed drive, a folder someone else set up,
`C:\Program Files`, or the root of `C:\` are all places where you may be able to read and not write.

**How to be sure in ten seconds:** open the folder in File Explorer, right-click in the empty space,
choose **New → Text Document**, and then delete it again. If both work, you have what you need.

Anywhere under your own **Documents** folder is always safe, and is a good default:
`C:\Users\<you>\Documents\Cassandra\`.

If you get this wrong, `jcass-dm scaffold` tells you plainly:

```
Cannot create files in 'C:\Program Files'. Nothing was written.

Your Windows account does not have permission to write there.
```

That is not a fault in the tool or in your model — move to a folder you own and run it again.

**A synced folder — OneDrive, Dropbox, SharePoint — will usually work, and is worth avoiding
anyway.** Builds write thousands of small files into `bin\` and `obj\`, the sync client tries to
upload every one of them, and the result is a slow build and occasional file-locked errors that look
like something else entirely.

## 5. Reading what came back

Three commands produce nearly all the output you will see:

| Command | It worked when you see |
|---|---|
| `dotnet build ...` | `Build succeeded.` with `0 Warning(s)` and `0 Error(s)` |
| `.\tools\jcass-dm.exe check ...` | `No problems.` at the end |
| `.\tools\jcass-dm.exe package ...` | the path of the zip it wrote |

**Anything else — paste it back to your assistant in full.** Not a summary and not the last line.
The line that explains a failure is very often three lines above the one that looks like the error,
and it is the one people leave out. [`reading-errors.md`](reading-errors.md) is the guide to which
line matters.

---

## For the assistant — say where the command runs, every time

The engineer is a civil engineer. **Do not assume they know what a terminal is, which one to use, or
that the one they already have open is the right one.** A command block on its own reads, to
somebody who has not done this before, as something they are supposed to already know where to put.

Every command block gets one line in front of it naming the terminal and the folder:

> In your PowerShell terminal — the one you already have open in the `JCassDomainModelAssistant`
> folder — run:

If they do not have one open yet, say how: **Terminal → New Terminal** in VS Code. If the command
must run somewhere else, say that instead, and say it before the block rather than after it.

**Before any command that creates or writes files outside the Assistant folder** — `scaffold` above
all — say the **absolute path** it will land at, and ask them to confirm they can write there:

> This will create `C:\Work\MyRoadModel`, beside the Assistant folder. Can you create files in
> `C:\Work`? If you are not sure, open it in File Explorer and try New → Text Document.

`--output ..\MyRoadModel` is a relative path, and relative to *the terminal's* folder rather than to
anything the engineer can see. Resolving it out loud is the difference between them checking your
work and them finding out later where their model went.
