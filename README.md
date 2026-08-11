# Juno Cassandra — Domain Model Assistant

A self-contained VS Code project that lets a civil engineer, working alongside an AI coding
assistant, design, build, debug, publish and maintain a **Custom Domain Model** for
[Juno Cassandra](https://junocassandra.com) — without being a software developer.

> **Status: beta.** Everything an engineer needs is in place — the reference
> model, the `jcass-dm` tool, the framework API reference, the conventions, the end-to-end workflow,
> the pattern library and the Claude skills. What has not happened yet is a full end-to-end
> acceptance pass on Lonrix's side. Expect the occasional rough edge, and
> send it to **support@lonrix.com**.

---

## What this is for

Juno Cassandra runs a **framework model** — it reads your network data, steps through modelling
periods, asks an optimiser which treatments to fund within budget, and writes the outputs. You never
write any of that.

A **domain model** is the part you do write: the engineering. How an element deteriorates, when work
is triggered, what it costs, and how condition recovers afterwards. Juno Cassandra lets a client's
own modeller write one in C#, debug it in the browser with real breakpoints, and publish it as their
live model.

The obstacle is that the AI assistant helping them knows nothing about the framework. It cannot see
the framework source, it does not know which conventions fail silently, and left to itself it
invents method signatures that do not exist and deterioration rates it has no basis for. **This
repository is the context that makes the assistant useful instead.**

---

## Prerequisites — have these before you download

Six things. Four are free downloads, one is a subscription you already have or will buy, and one is
a decision about a folder.

| | What | Where / how |
|---|---|---|
| 1 | **A Windows PC** you can install software on | `jcass-dm` is a Windows program and the commands in these pages are PowerShell. Windows 10 or 11. |
| 2 | **Visual Studio Code** — free | <https://code.visualstudio.com/> · Free and open source. Not the same product as Visual Studio; you want **Code**. When you open this repository's workspace it will offer to install the recommended C# extension — accept it. |
| 3 | **The .NET 9 SDK** — free | <https://dotnet.microsoft.com/download/dotnet/9.0> · Take the **SDK** (not the Runtime), x64, for Windows. Confirm afterwards by opening a terminal and running `dotnet --version` — it should print `9.` something. |
| 4 | **A paid subscription to an AI coding assistant that runs inside VS Code** | Claude is the recommended and supported choice. Copilot agent mode, Cursor and other editor-based agents work but are untested. **A browser chat window is not sufficient** — the assistant has to be able to run commands and read what they printed. The subscription is yours; Lonrix does not pay for it and does not procure it. |
| 5 | **A folder you can read *and write* in**, for both this repository and your model | Anywhere under your **Documents** folder is always safe. Avoid `C:\Program Files`, the root of `C:\`, and managed corporate drives where you may be able to read but not write. Check in ten seconds: open the folder in File Explorer, right-click → **New → Text Document**, then delete it. If both work, you are fine. |
| 6 | **Access in Juno Cassandra**, arranged by an administrator | A client with `inputs\` populated, access to the **Debug Model** page, and permission to publish. Ask at **support@lonrix.com**. You can do all your local work before this is in place. |

**Recommended, not required — a GitHub account** (free, <https://github.com/>). Your model folder is
yours, and putting it in a git repository gives you a history of every change, a way to get back to
last week's version, and a way to share the model with a colleague or with Lonrix support. Nothing
in this Assistant needs git and no step below asks you to use it — your assistant will not raise it
unless you do.

**Your model does *not* live in this repository**, so nothing here has to be preserved. See
[How to use it](#how-to-use-it) below.

Full detail — why the requirement is a *capability* rather than a brand, why a browser chat window
falls down, the plain statement that this Assistant is an **accelerant and not a licence
requirement**, and how updates arrive:
[`docs/orientation/prerequisites.md`](docs/orientation/prerequisites.md). That page is the canonical
one; this section is the short list you need before you download anything.

---

## How to use it

1. Download this repository as a ZIP and unpack it into the folder you chose in prerequisite 5.
   **GitHub names the unpacked folder `JCassDomainModelAssistant-main`**, with the `-main` on the
   end. Rename it to `JCassDomainModelAssistant` if you like — nothing depends on the name, but
   these pages use the short one, and a folder you can spell is a folder you can `cd` into.
2. **Open it in VS Code.** Double-click [`assistant.code-workspace`](assistant.code-workspace)
   inside the unpacked folder. VS Code opens with this repository loaded and offers to install the
   C# extension — accept it.

   *A `.code-workspace` file is just a small VS Code settings file that says which folders to open
   and how. Nothing more; you never have to edit it by hand.* If double-clicking does nothing,
   open VS Code first and use **File → Open Workspace from File…**.
3. Point your AI assistant at [`docs/00-start-here.md`](docs/00-start-here.md), and tell it what you
   want to do — see [What to say to your assistant](#what-to-say-to-your-assistant) below.
4. **Later, once your model folder exists**, add it to the same VS Code window:
   **File → Add Folder to Workspace…**, pick your model folder, and say yes if VS Code offers to
   save the workspace. You then see the Assistant and your model side by side in one window, and
   your assistant can read the guidance and edit your model without you switching between them.

   There is nothing to add at step 2 — **your model does not exist yet**. Your assistant creates it
   in your first session, in a folder beside this one.

**Your model lives in its own folder, beside this one — never inside it.** That is deliberate, and
it is what makes this repository safe to replace wholesale when a newer version is released:
re-downloading the Assistant never touches your model.

New to the terminal? [`docs/orientation/running-commands.md`](docs/orientation/running-commands.md)
covers opening a PowerShell terminal in VS Code, why the folder it is sitting in decides what the
commands do, and how to check you can write where you are about to work.

---

## What to say to your assistant

Once the workspace is open, you talk to your assistant in ordinary English. These are prompts that
work, and what each one gets you.

**Starting out**

> *"I want to start a new domain model, walk me through it."*

The most useful opening line there is. It takes you through the engineering questions first —
which treatments, which input columns, which parameters
([`docs/workflow/01-plan-your-model.md`](docs/workflow/01-plan-your-model.md)) — then scaffolds a
correctly-named project **in a folder it names and confirms with you first**, builds it, and checks
it.

> *"I have an existing domain model in the folder next door. Check it and tell me what state it is
> in."*

The other way in. Your assistant runs `jcass-dm check` before it changes anything, and reports what
does and does not hang together.

**Making a change**

> *"I have an existing domain model. I want to add a new input column and an associated model
> parameter. Guide me through it."*

An input column and a parameter each touch several files, and missing one of the places is silent —
the model builds, runs, and is wrong. Your assistant walks the change through every place it lands
and then proves it with a check.

> *"I want to add a treatment called reseal."*

Note the verb. *"Add"* means **do it**; *"guide me through adding"* means **teach me while I do
it**. Both are supported and they are deliberately different — say which you want.

**Getting the engineering into code**

> *"I have a parameter for which the increment is determined with a regression equation with many
> constants. How do I program that?"*

An excellent question to ask early. There is a canonical answer — the coefficients go in a CSV in
the client's `supporting\` folder rather than into your C# or into forty rows of `lookups.xlsx` —
and your assistant has the pattern with a working example.

> *"A reseal triggers when rut depth is over 10 mm. Where does the 10 go?"*

Into `inputs\lookups.xlsx`, so you can change it yourself on the Tuning page and re-run in minutes.
Your assistant will put it there rather than into the code, and this is worth watching it do once.

**When something breaks**

> *"The build failed. Here is the output: ..."* · *"My breakpoint is hollow and will not bind."* ·
> *"Check my model and explain anything that is not OK."*

Paste output in full rather than summarising it. The line that explains a failure is often three
lines above the one that looks like the error.

### Prompts that will not work, and why

Your assistant does the **plumbing**. It does not supply the **engineering judgement** — and it is
built to say so rather than to guess convincingly.

| Ask | Why it gets refused |
|---|---|
| *"What treatment should I do when rut depth is high?"* | That is your engineering decision. It depends on your network, your materials, your budget and your standards, and an assistant answering it is inventing. |
| *"What is more important for my model, rutting or roughness?"* | Same. Nobody outside your organisation can answer it, and a confident answer is worse than none. |
| *"What deterioration rate should I use for chipseal?"* | It will help you put a rate into `lookups.xlsx` and read it correctly in code. It will not tell you what the number is. |
| *"Publish this model for me."* | Publishing overwrites your client's live model, and there is no command-line route to it by design. It happens in the browser, by you, deliberately. |
| *"Just hard-code the threshold for now, we'll move it later."* | Tunable numbers go in `lookups.xlsx`. "Later" does not arrive, and the model becomes one only a developer can recalibrate. |

If a question genuinely cannot be answered from this repository, the assistant is told to stop and
draft you a support request rather than improvise —
[`docs/conventions/when-to-stop.md`](docs/conventions/when-to-stop.md).

---

## What is here today

| Folder | Holds | State |
|---|---|---|
| [`docs/`](docs/) | Orientation, conventions, the framework API reference, the ten canonical patterns, and the end-to-end workflow | **Present** |
| [`reference-model/`](reference-model/) | `DomainModelSample`, a small working model, plus a snapshot of sample inputs | **Present** |
| [`examples/`](examples/) | One compiling example per pattern, built in CI | **Present** |
| [`tools/`](tools/) | `jcass-dm` — scaffolds, reads and writes the bundle, checks, packages | **Present** |
| [`refs/`](refs/) | Framework reference assemblies to compile against, and their API documentation | **Present** |
| [`scripts/`](scripts/) | Maintenance scripts | **Present** |
| [`.claude/`](.claude/) | Claude skills — a convenience layer, never a holder of unique knowledge | **Present** — and optional: delete the folder and everything still works, with more typing |

## Getting help

Anything this repository does not cover — a framework call that is not in the API reference, a
failure the docs do not explain, or the docs contradicting what you see on screen — goes to
**support@lonrix.com**. Include what you tried, the exact error, the framework version stamp from
`refs/FRAMEWORK-VERSION.txt`, and your model's name.

## Licence

See [`LICENSE`](LICENSE).
