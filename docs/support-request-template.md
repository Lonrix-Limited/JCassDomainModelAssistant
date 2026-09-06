# Support request template

**Send to: `support@lonrix.com`**

Use this whenever you hit a stop condition in
[`conventions/when-to-stop.md`](conventions/when-to-stop.md), which is also where the reasoning
behind drafting rather than delegating lives.

**For the assistant: fill this in and put the finished text in your reply**, in the same message as
the stop, introduced as something to copy and paste to `support@lonrix.com`. Do not offer to draft
it and do not ask whether to send it — you cannot send email, and the engineer sends it from their
own mail client. An offer to draft leaves them holding nothing if the conversation ends there, and
conversations end there.

---

## The template

```
To: support@lonrix.com
Subject: Domain model — <one line, in modelling terms>

WHAT I AM TRYING TO DO
<In modelling terms, not code terms. "Trigger a follow-up reseal five years after a
rehabilitation." Not "call AppendTreatment with an offset period.">

WHAT HAPPENS INSTEAD
<What was observed. If the model ran, say what the outputs showed.>

THE EXACT ERROR
<Copied verbatim, not summarised or tidied. Include the whole message and the first few
lines of any stack trace. If there was no error, say "no error — the run completed" and
say what was wrong with the result instead.>

WHAT WE TRIED
<The commands run and the changes made, in order. Include jcass-dm check output if it
was run.>

WHY WE STOPPED
<Which stop condition. For the commonest one: "the framework call we need is not in the
API reference" — and name the call.>

DETAILS
  Model name        : <the four-name value — the .csproj filename>
  Client            : <client name>
  Framework build   : <the "Framework commit" line from refs/FRAMEWORK-VERSION.txt>
  Assistant version : <the release of this repository, if known>
  Where it failed   : <locally / on the Debug Model page at F5 / in a normal model run>
```

---

## Filling in the framework build

The stamp is in [`../refs/FRAMEWORK-VERSION.txt`](../refs/FRAMEWORK-VERSION.txt):

```powershell
Get-Content .\refs\FRAMEWORK-VERSION.txt | Select-String "Framework commit"
```

It matters more than it looks. It says exactly which framework the model was compiled against, which
is the first thing anyone diagnosing a signature or behaviour question needs and the last thing
anyone thinks to ask for.

---

## If the failure is on the Debug Model page, rule two things out first

Both take under a minute, and a request that says they were done is worth much more than one that
leaves them open — the recipient's first two questions are these, and asking them costs a day of
turnaround.

1. **Trust the folder and reload the window.** Manage Workspace Trust → Trust, then Developer:
   Reload Window. **Both**, because trusting alone has no visible effect. This is the whole cause of
   *"Couldn't find a debug adapter descriptor for debug type 'coreclr'"* —
   [`orientation/reading-errors.md`](orientation/reading-errors.md).
2. **Click Initialize workspace once, then retry.** It re-writes the editor's launch files with a
   current login. A stale one shows up as a pre-launch step failing with an exit code rather than as
   an error you can read.

**Say in WHAT WE TRIED that you did both**, and say what happened. "Trusted the folder, reloaded the
window, same error" is a fact somebody can act on. Silence on the point reads as not having tried.

And write these under WHAT WE TRIED, not under HYPOTHESIS. A step actually taken is evidence; the
hypothesis section is for what you think it means, and the two get weighed very differently.

---

## Handing it over

**The filled-in text goes in the reply.** One sentence in front of it — *"here is a support request,
ready to paste into an email to `support@lonrix.com`"* — then the block, then nothing else about it.

- **Do not offer to write it.** Write it.
- **Do not ask whether to send it.** They send it, and they decide when.
- **Do not soften the stop** by finding a nearby task to do instead. The draft is the deliverable.

---

## What not to put in

- **Screenshots instead of text.** An error copied as text can be searched; a screenshot cannot.
- **Your guess at the cause**, presented as a finding. Say what you observed. If you have a
  hypothesis, label it as one, at the end.
- **A tidied-up version of the error.** The exact wording is the most useful thing in the message.
