# Support request template

**Send to: `support@lonrix.com`**

Use this whenever you hit a stop condition in
[`conventions/when-to-stop.md`](conventions/when-to-stop.md), which is also where the reasoning
behind drafting rather than delegating lives.

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

## What not to put in

- **Screenshots instead of text.** An error copied as text can be searched; a screenshot cannot.
- **Your guess at the cause**, presented as a finding. Say what you observed. If you have a
  hypothesis, label it as one, at the end.
- **A tidied-up version of the error.** The exact wording is the most useful thing in the message.
