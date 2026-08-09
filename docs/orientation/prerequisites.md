# Prerequisites

What has to be in place before an engineer can build a domain model, and who provides each thing.

---

## On their machine

| What | Why | Note |
|---|---|---|
| **VS Code** | Where the work happens. Open this repository and their model folder side by side using [`assistant.code-workspace`](../../assistant.code-workspace). | Free. |
| **The .NET 9 SDK** | Builds the model. `dotnet build` comes from it. | Free. Check with `dotnet --version`. |
| **This repository** | The framework reference assemblies, the API reference, the conventions, and `jcass-dm`. | Download the ZIP; no account needed. |
| **A paid AI coding assistant** | See below. | Theirs to choose and to pay for. |

Nothing else. There is no framework installer, no NuGet feed and no licence key on the engineer's
side — the reference assemblies are committed in `refs\`, so the project compiles the moment it is
unzipped.

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
