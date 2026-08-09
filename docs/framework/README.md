# The framework: API reference and concepts

Everything in this folder is **generated** from two upstream sources and mirrored one way. Nothing
here is written by hand.

| What | Generated from | By |
|---|---|---|
| [`api/`](api/) | The reference assemblies and XML documentation in [`../../refs/`](../../refs/) | `cassandra_main\scripts\assistant\generate-api-reference.ps1` |
| [`concepts/`](concepts/) | The public Juno Cassandra documentation source (`jcass_docs2`) | `cassandra_main\scripts\assistant\sync-framework-concepts.ps1` |

**Do not edit any file in this folder.** Every one of them carries a banner saying so, and the
banner names the script that will overwrite it. An improvement made here is lost at the next sync,
silently and without a merge conflict.

---

## Start here — which document answers your question

This is a routing table, and it is the part you read. There are roughly 13,000 words of concepts
below it plus a full API reference; **loading the corpus is not the goal, finding the one page is**.

| You need to | Read |
|---|---|
| **Check a framework call before writing it** | [`api/README.md`](api/README.md) — the allow-list, then the type's own page. **This is the check that matters most.** |
| Know whether you are allowed to call something at all | [`api/README.md`](api/README.md)'s table states, per type, whether a domain model should use it |
| Construct a `TreatmentInstance` | [`api/authoring/TreatmentInstance.md`](api/authoring/TreatmentInstance.md) — eight positional parameters; use named arguments |
| Split one treatment's cost across budget categories | `AssignBudgetCategoryFractions` on [`api/authoring/TreatmentInstance.md`](api/authoring/TreatmentInstance.md) |
| Know what to implement, and what the framework will call | [`api/authoring/DomainModelBase.md`](api/authoring/DomainModelBase.md) and [`api/authoring/IDomainModel.md`](api/authoring/IDomainModel.md) |
| Read a lookup, an input column, or a model parameter | [`api/authoring/ModelBase.md`](api/authoring/ModelBase.md) — the author-facing accessors only |
| Find the client folder from inside a model | `WorkFolder` on [`api/authoring/ModelConfiguration.md`](api/authoring/ModelConfiguration.md) |
| Load a side-car CSV at setup | [`api/authoring/CSVHelper.md`](api/authoring/CSVHelper.md) and [`api/authoring/jcDataSet.md`](api/authoring/jcDataSet.md) |
| Build a deterioration curve or a fitted sub-model | [`api/authoring/PieceWiseLinearModel.md`](api/authoring/PieceWiseLinearModel.md), [`LogisticModel`](api/authoring/LogisticModel.md), [`LinearRegressionModel`](api/authoring/LinearRegressionModel.md) |
| Work in a Monte Carlo model | [`api/authoring/DistributionSimulator.md`](api/authoring/DistributionSimulator.md), [`MarkovTransitionSimulator`](api/authoring/MarkovTransitionSimulator.md), [`NormalGenerator`](api/authoring/NormalGenerator.md) |
| **Decide whether to proceed, flag, or stop** | [`api/not-for-you.md`](api/not-for-you.md) — the one mechanical test |
| Recognise a type you met in a signature but should not create | [`api/referenced.md`](api/referenced.md) |
| | |
| Understand *what a period is*, or any framework vocabulary | [`concepts/06-definitions.md`](concepts/06-definitions.md) |
| Know which half of the work is yours | [`concepts/02-framework-model.md`](concepts/02-framework-model.md) |
| Decide which method a piece of logic belongs in | [`concepts/03-execution-stages.md`](concepts/03-execution-stages.md) |
| Tell raw input data from model parameter data | [`concepts/04-data-components.md`](concepts/04-data-components.md) |
| Choose a model type at the start of a project | [`concepts/05-model-types.md`](concepts/05-model-types.md) |
| Understand what the optimiser does with what your trigger returns | [`concepts/07-bca-model.md`](concepts/07-bca-model.md) |
| Wire up a domain model's setup, parameters and lookups | [`concepts/08-domain-model-setup.md`](concepts/08-domain-model-setup.md) — the heavyweight, ~5,300 words |
| Browse the concepts by reading order | [`concepts/README.md`](concepts/README.md) |

---

## The one rule this folder exists to support

> **Is the framework call you are about to write listed in [`api/`](api/)?**

**Yes** — proceed, and compose it with the documented patterns as freely as the problem needs.

**No** — stop. Do not guess a signature, do not infer it from a similar method, and do not copy one
out of a model found elsewhere. Draft the engineer a support request to `support@lonrix.com` saying
what was attempted, the exact error, the framework build stamp from [`api/README.md`](api/README.md)
and the model name.

The test is deliberately mechanical, because "is it on the list?" is something an agent can
actually decide, where "am I confident enough about this API?" is not. The full three-tier version —
including the middle case, *proceed and flag* — is in [`api/not-for-you.md`](api/not-for-you.md).

---

## Two things to know about the API reference

**It is generated from assembly metadata, not from the XML alone.** That distinction is load-bearing.
A C# XML documentation file contains only members somebody wrote a `///` comment for, and several of
the most important members in the framework have none — `TreatmentInstance`'s constructor among them.
Reading the metadata means every public member appears with its exact parameter order whether the
framework documented it or not. Where a member says *"No framework documentation for this member"*,
the **signature is still authoritative**; only the prose is missing.

**It is scoped, and the scope is a decision.** The reference covers the surface a domain model
actually touches, not the whole framework. That is not an oversight — an exhaustive reference nobody
can navigate is a reference nobody uses, and a list of a hundred types with no signal about which to
call is what leads an agent into framework internals believing it is still on the map. What is
missing from here is either framework internals or a gap worth reporting; see
[`api/not-for-you.md`](api/not-for-you.md).

---

## Regenerating

Both scripts live in the `cassandra_main` repository, not this one, and they write **into** this
repository.

```powershell
# Refresh the reference assemblies and XML docs first, if the framework has moved.
.\scripts\assistant\refresh-refs.ps1

.\scripts\assistant\generate-api-reference.ps1
.\scripts\assistant\sync-framework-concepts.ps1
```

Both are idempotent: running either twice produces no diff. Both fail loudly rather than skipping
quietly — a type missing from the assemblies, or a documentation page that has disappeared upstream,
stops the run with its name on screen. That is deliberate. A silent skip means this folder quietly
loses a page, nothing breaks, and nobody notices for months — there is simply one fewer thing the
agent knows, and it starts guessing in the gap.
