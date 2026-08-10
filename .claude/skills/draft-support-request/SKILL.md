---
name: draft-support-request
description: Draft a filled-in support request to Lonrix after hitting a stop condition — the framework version stamp, the exact error, what was tried, and why you stopped. Use whenever a stop condition fires, a framework call is not in the API reference, or the engineer says "I'm stuck" / "who do I ask".
---

# Draft a support request

**This skill is a wrapper.** One template and one stamp file. Without it, do the same job by reading
[`docs/support-request-template.md`](../../../docs/support-request-template.md).

**This is what a stop looks like from the engineer's side.** "Contact support" on its own produces
an email that says *it doesn't work*, and then a day is spent establishing the basics they had in
front of them the whole time.

## 1. Read

- [`docs/support-request-template.md`](../../../docs/support-request-template.md) — the template, field by field, and what not to put in.
- [`docs/conventions/when-to-stop.md`](../../../docs/conventions/when-to-stop.md) — which stop condition fired, in its own words. Name it in the `WHY WE STOPPED` field.

## 2. Fill in everything you can see

Most of the template is already in front of you: the command output, the exact error, the changes
made in this session. Fill those in verbatim — **not summarised or tidied**. Leave a marked gap only
where you genuinely cannot know the value.

Two fields have a command:

```powershell
Get-Content .\refs\FRAMEWORK-VERSION.txt | Select-String "Framework commit"
```

and the model name is the `.csproj` filename — `jcass-dm check --project ..\TheirModel` prints it,
along with output worth pasting into `WHAT WE TRIED`.

## 3. Hand it over

Present it as a message ready to send, and say plainly:

- that you have stopped, and why;
- that it goes to **support@lonrix.com** — the only destination;
- that **they** send it. Do not send it yourself, and do not offer to.

## 4. Never

- **Never soften the stop by finding a nearby thing to do instead.** If you have stopped, say you
  have stopped. A drafted request is the deliverable, not a consolation for one.
- **Never present a guess as a finding.** Say what was observed. A hypothesis goes at the end,
  labelled as one.
- **Never tidy the error.** The exact wording is the most useful thing in the message.
