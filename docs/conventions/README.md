# docs/conventions/

**The rules that fail *silently* — which is the whole reason this repository exists.** An error is
self-correcting; silence is not. A domain model can be wrong while the run completes, the outputs
look plausible, and nothing anywhere says a word.

| Read | When |
|---|---|
| [`silent-failures.md`](silent-failures.md) | **Before saying a model is finished.** Twelve ways a model is wrong without complaining, each with the symptom, the cause, and what catches it. |
| [`where-numbers-live.md`](where-numbers-live.md) | Before writing any number into C#. The three-tier split, the decidable test, and the boundaries on both sides of it. |
| [`when-to-stop.md`](when-to-stop.md) | Before writing a framework call you are not certain exists. Proceed, flag, or stop and escalate. |
| [`four-names.md`](four-names.md) | Renaming a model, or diagnosing *"class not found in the specified .dll"*. |
| [`naming-and-folders.md`](naming-and-folders.md) | Deciding which folder a file belongs in, or building an upload zip. |

Wherever a convention here can be checked mechanically, `jcass-dm check` checks it. Where it cannot,
the page says so — those are the ones to raise out loud.
