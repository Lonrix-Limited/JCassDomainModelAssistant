# Prerequisites

What has to be in place before an engineer can build a domain model, and who provides each thing.

---

## On their machine

**The download links live once, in [`../../README.md` § Prerequisites](../../README.md#prerequisites--have-these-before-you-download)** — that is the section written for somebody who has not
downloaded anything yet. This page owns the *reasoning*: why each item is needed, which assistant,
who pays, and how updates arrive. Send an engineer to the README for the links; extend this page
when the reasoning changes.

| What | Why | Note |
|---|---|---|
| **A Windows PC** | `jcass-dm` ships as a Windows executable and every command in `workflow\` is PowerShell. | Windows 10 or 11. |
| **VS Code** | Where the work happens. They open this repository by double-clicking [`assistant.code-workspace`](../../assistant.code-workspace), and add their model folder to that same window once it exists — **File → Add Folder to Workspace…**. | Free. |
| **The .NET 9 SDK** | Builds the model. `dotnet build` comes from it. | Free. Check with `dotnet --version`. |
| **This repository** | The framework reference assemblies, the API reference, the conventions, and `jcass-dm`. | Download the ZIP; no account needed. |
| **A folder they can read *and write*** | Both this repository and their model folder are written into — the build alone creates thousands of files. | Under **Documents** is always safe. See below. |
| **A paid AI coding assistant** | See below. | Theirs to choose and to pay for. |

Nothing else. There is no framework installer, no NuGet feed and no licence key on the engineer's
side — the reference assemblies are committed in `refs\`, so the project compiles the moment it is
unzipped.

### The folder, and why it is on this list

**Ask about write permission before the first command, not after it fails.** Engineers commonly work
on machines where the obvious folder is not theirs to write to: a managed corporate drive, a shared
network location, `C:\Program Files`, the root of `C:\`. Reading works, so nothing looks wrong until
the first write.

`jcass-dm scaffold` now probes for this and refuses with a plain message naming the folder rather
than failing part-way through, but the cheaper moment to catch it is in conversation. How to check
it in ten seconds, and why a synced folder (OneDrive, Dropbox, SharePoint) is worth avoiding for a
model folder: [`running-commands.md` § 4](running-commands.md#4-you-need-write-permission-in-both-folders).

### Git and GitHub — recommended, never required

**A GitHub account is worth having and nothing here depends on it.** A model folder in a git
repository gives the engineer a history of every change, a way back to last week's version, and a
way to hand the model to a colleague or to Lonrix support. That is genuinely useful for a model that
will be maintained for years.

**It changes nothing about how an assistant works, and the rule stands:** never assume git
knowledge, never make a git step part of a procedure, and never route an Assistant *update* through
git — updates are a re-download ([below](#getting-a-newer-version)). If the engineer raises git, or
already keeps their model in a repository, help them with it as an ordinary request. Do not raise it
unprompted in the middle of a modelling task.

## On the web app

| What | Who arranges it |
|---|---|
| A Juno Cassandra client folder with `inputs\` populated | Already in place for any operating client |
| Access to the **Debug Model** page | An administrator grants it |
| Permission to **publish** a custom domain model | An administrator grants it, and the modeller cannot grant it to themselves |

**That last row is a deliberate safety property, not an obstacle.** There is no command-line publish
path at all, so nothing an assistant does can reach production. See
[`what-you-are-building.md`](what-you-are-building.md).

---

## The AI assistant — which one, and who pays

You need a paid AI coding assistant that runs inside your editor and can run commands and read
their output. A browser chat window is not enough. Claude is the recommended and supported choice;
others work but are untested.

**The requirement is a capability, not a brand.** The assistant has to be able to do three things:
read the files in your project, run a command in the terminal, and **read what that command
printed**. Three tiers, and the third is the one to watch for:

| | What you get |
|---|---|
| **Supported** — VS Code with Claude Code | Everything, including the Claude skills under `.claude\` |
| **Works, untested** — VS Code with Copilot agent mode, Cursor, Codex CLI, or any other editor-based agent with terminal access | Everything except the skills. The agent reads `docs\` and calls `jcass-dm` directly, which is exactly what the skills do for it |
| **Not sufficient** — a chat window in a browser, with copy and paste | The documentation is readable and nothing else works. No build, no `jcass-dm check`, no scaffold |

**That last row is the trap**, and it is an easy one to fall into: the pages in this repository read
perfectly well in a browser, so it feels like it is working. It is not. `dotnet build` and
`jcass-dm check` are the feedback loops that catch confidently-wrong C# before it reaches a
forecast, and an assistant that cannot run them and read the result is guessing — while sounding
exactly as certain as one that is not.

**The subscription is yours.** Lonrix does not pay for it and does not procure it on your behalf —
you choose a provider and you hold the account.

It is an accelerant, not a licence requirement. A Custom Domain Model can still be written entirely
by hand, exactly as before, and nothing about Juno Cassandra obliges you to buy anything.

**Why "inside your editor" is the requirement.** Half of what makes this repository useful is the
assistant running `dotnet build` and `jcass-dm check` and reading the results. An assistant that can
only read text you paste into it cannot do that, which is why the choice is about *which* assistant
rather than *whether* to use one at all.

---

## Getting a newer version

**Improvements arrive by re-downloading this repository, not by `git pull`.** When a better version
is released, the engineer is told, downloads it, and — if they want — asks their assistant to run
over the model they already have in refactor mode.

No git, no merge, no partial update, no branch.

**Re-downloading replaces the Assistant only. It never touches their model.** That is worth saying
out loud, unprompted, the first time an update comes up: a non-developer's first fear is losing
their work. It is safe for a structural reason — **their model lives in a sibling folder, never
inside this repository** — so the Assistant is entirely stateless with respect to their work and can
be swapped wholesale.

Two things ride along with a new version:

- **`refs\` is replaced too**, so the framework update comes with it. Each release is stamped with
  the framework build it carries, in [`../../refs/FRAMEWORK-VERSION.txt`](../../refs/FRAMEWORK-VERSION.txt).
  Compare what they have against what their model expects:

  ```powershell
  .\scripts\check-framework-version.ps1
  ```

- **A "what to re-check in your model" note in the changelog.** `jcass-dm check` catches what is
  mechanically checkable; a new *guidance* rule is prose, and nothing will surface it for them.
