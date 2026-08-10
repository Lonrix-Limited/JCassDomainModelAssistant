# 00 — Prerequisites

**This page is a pointer. The prerequisites are written once, in
[`../orientation/prerequisites.md`](../orientation/prerequisites.md).** Read that, then come back
here for the two things that are about *starting the workflow* rather than about being set up.

If something is missing from the prerequisites, extend that page. Do not add a second copy here —
two lists of what you need drift apart, and then neither can be trusted.

---

## In one line

| You need | Provided by |
|---|---|
| VS Code, the .NET 9 SDK, this repository | Free downloads. Nothing to licence, no NuGet feed, no installer |
| **A folder you can read *and write* in**, holding both this repository and your model beside it | You. Under **Documents** is always safe — [`../orientation/running-commands.md` § 4](../orientation/running-commands.md#4-you-need-write-permission-in-both-folders) |
| **A paid AI coding assistant that runs inside the editor** and can run commands *and read their output* | **The client.** Lonrix does not pay for it and does not procure it |
| A Juno Cassandra client with `inputs\` populated | Already true of any operating client |
| Access to the **Debug Model** page, and permission to **publish** | An administrator at Juno grants both |

Full detail, including why the requirement is a *capability* rather than a brand, why a browser
chat window is not sufficient, and the plain statement that the Assistant is an **accelerant, not
a licence requirement**: [`../orientation/prerequisites.md`](../orientation/prerequisites.md).

---

## Two things that block step 10, and are not on that page

### 1. Your model must already exist in Juno Cassandra as a custom domain model

The **Debug Model** page seeds its workspace from a domain-model version that already exists on
the server. Somebody at Juno creates the custom model and its single version for the client; you
cannot create one from the browser or from the command line.

If **Initialize workspace** reports

> Domain model version folder does not exist on disk … The registry may be out of sync — contact
> an admin.

or the **Model version** dropdown is empty, that is this. Email **support@lonrix.com** and ask for
a custom domain model to be created for the client. Do not look for a workaround; there is not one
on your side.

You can do all of step 10 — scaffold, build, check — before this is in place. It only blocks
step 20.

### 2. You must hold the project lock

**The Debug Model item in the navigation bar is greyed out until you hold the lock on the active
project.** Hovering it says *"Take the project lock on Project Home to open the Debug Model
page."*

To take it: **Project Home → Lock project.** You can add a note saying what you are doing; other
users see it. Release it with **Release my lock** when you are finished, so somebody else can
work.

This is not a formality. Publishing checks it separately and refuses with *"You must hold this
project's lock to publish"* if the project is merely unlocked — "nobody is blocking me" is not the
same as "this is mine to change".

---

Never used a terminal? Read [`../orientation/running-commands.md`](../orientation/running-commands.md)
before step 10. Five minutes, and it covers the one mistake that accounts for most "the command did
not work" — a terminal sitting in the wrong folder.

Next: [`01-plan-your-model.md`](01-plan-your-model.md) for a new model, or
[`05-adopt-an-existing-model.md`](05-adopt-an-existing-model.md) if you are picking up a model
somebody else wrote.
