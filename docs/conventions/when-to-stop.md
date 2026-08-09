# When to proceed, when to flag, and when to stop

**This is the rule that stops you inventing framework API for a reader who cannot tell that you
did.** The engineer you are helping usually cannot look at a method call and know whether it
exists. If you guess a signature confidently, they will believe you, and they will find out at F5
or — worse — in a forecast that runs to completion and is wrong.

Summarised in [`../00-start-here.md`](../00-start-here.md) § 3. This page is the full rule and the
reasoning behind it.

---

## The test

> **Is the framework call you are about to write listed in the API reference?**

That is the whole test. Check it in [`../framework/api/README.md`](../framework/api/README.md) —
the type table first, then the type's own page for the exact member and parameter order.

It is deliberately mechanical. *"Is it on the list?"* is something you can actually decide.
*"Am I confident enough about this API?"* is not — you will always feel confident, because
inventing a plausible signature feels exactly like remembering a real one.

---

## The three tiers

### Proceed

You are composing documented patterns, and **every framework call appears in the API reference**.

Build it. Compose as freely as the problem needs — different types, different order, a shape that
appears in no example. Composition is the normal case, not the exceptional one.

### Proceed and flag

It is **not a documented pattern**, but it is built **only from documented API**.

Build it, and then say so plainly to the engineer:

> This is not one of the canonical patterns — I have put it together from documented framework
> calls, and it should work, but it is worth checking with Lonrix before you rely on it in a
> production forecast.

Say it once, in plain words, at the point they can act on it. Do not decorate the code with
warnings.

### Stop and escalate

Stop, do not guess, and draft a support request when **any** of these is true:

| Stop when | Why this one |
|---|---|
| A framework call you need is **not in the API reference** | This is exactly the moment you would be inventing. Nothing else on this list is as reliable a signal. |
| The task needs a **server action or an admin action** | Registry changes, permissions, provisioning, another client's data. You cannot do these and neither can the engineer; guessing produces a confident dead end. |
| The docs **contradict what the engineer sees on screen** | One of the two is wrong and you cannot tell which. Continuing means building on whichever you happened to prefer. |
| A **failure is not covered** anywhere in these docs | Diagnosing an unfamiliar framework failure by inspection is how a wrong root cause gets fixed for an afternoon. |

Do not soften a stop by finding a nearby thing to do instead. If you have stopped, say you have
stopped, say why, and hand them the drafted request.

---

## This is not a ban on undocumented work

Read strictly — *"if it is not written down, refuse"* — this rule fires on nearly every edit,
because almost all real modelling is composition rather than exact match. An engineer whose
assistant refuses constantly does one of two things: they argue with it until it gives in, or they
stop telling it what they are doing. Either way the rule is no longer there for the case that
actually mattered.

**So it is scoped to what you can check, not to what you feel unsure about.** The trigger is a
*framework call absent from the reference*, not novelty, not difficulty, and not your own
uncertainty about the engineering. If the call is on the list, proceed.

**If you find yourself wanting to tighten this, that is the failure mode arriving.** A version of
this rule that fires on ordinary work is strictly worse than no rule, because it teaches everyone
to route around it.

The opposite over-reading is just as wrong. "I'll infer the signature from the similar method two
lines up" is not composition — it is invention wearing composition's clothes. The API reference is
generated from assembly metadata, so **every public member of every listed type is there with its
exact parameter order**, whether the framework documented it in prose or not. If a member is
genuinely missing from the page, it is not on the list.

---

## Escalation is concrete, or it does not work

"Contact support" on its own produces an email that says *"it doesn't work"*, and then a day is
spent establishing the basics that the engineer had in front of them the whole time.

**Draft the request for them**, filled in, using
[`../support-request-template.md`](../support-request-template.md) — it lists every field and where
to get it. Fill in everything you can see and leave a marked gap only where you genuinely cannot.
Then tell them plainly that it is ready to send, and where to send it.

**Do not send it yourself.**

**One destination: `support@lonrix.com`.** Every stop condition on this page, every skill that
gives up, and the template all point there. Not a second address, not a form, not a phone number —
three routes means none of them stays maintained.
