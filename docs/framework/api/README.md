<!-- ------------------------------------------------------------------
     GENERATED FILE - DO NOT EDIT BY HAND.

     Generated from the framework reference assemblies and their XML
     documentation in refs\, by:

       cassandra_main\scripts\assistant\generate-api-reference.ps1

     The sync is ONE-WAY. Any edit made here is lost the next time that
     script runs, without warning and without a merge conflict. To change
     what this page says, change the /// documentation comments in the
     framework source, or the scoped surface in
     cassandra_main\scripts\assistant\api-surface.json, and regenerate.
     ------------------------------------------------------------------ -->

# Framework API reference

The framework types a domain model actually touches, with **every overload and every**
**parameter in order**. Generated from the reference assemblies in `refs\` and their XML
documentation, so it describes the framework you are compiling against and not a
remembered version of it.

**Framework build:** Framework commit : 4219f2013cdd1c3fde8be4b822e09da24c087ba4

---

## How to use this

**If a framework call you want to make is not on this page, that is the signal to stop.**
It means you are about to invent an API. Do not guess a signature — write the engineer a
support request to `support@lonrix.com` saying what you were trying to do, and stop.
See [`not-for-you.md`](not-for-you.md).

**Prefer named arguments everywhere.** Several of these members take four or more
parameters of the same type. Passing the right values in the wrong order compiles
cleanly and produces a wrong model, which is the single most expensive mistake
available here.

---

## The types you use

| Type | Should a domain model use it? | Page |
|---|---|---|
| `IDomainModel` | **Yes — this is the contract.** The framework calls these methods on your model. | [`IDomainModel`](authoring/IDomainModel.md) |
| `DomainModelBase` | **Yes — your entry class inherits from this.** | [`DomainModelBase`](authoring/DomainModelBase.md) |
| `IDomainModelGroupingFunctions` | Only if your model groups elements (for example, treating a whole road section as one candidate). Optional. | [`IDomainModelGroupingFunctions`](authoring/IDomainModelGroupingFunctions.md) |
| `TreatmentInstance` | **Yes — you construct these.** It is what a trigger returns. | [`TreatmentInstance`](authoring/TreatmentInstance.md) |
| `TreatmentType` | You read these; you do not create them. They come from the model setup. | [`TreatmentType`](authoring/TreatmentType.md) |
| `StrategySetupInfo` | Only in models that generate multi-treatment strategies for benefit-cost optimisation. | [`StrategySetupInfo`](authoring/StrategySetupInfo.md) |
| `ModelBase` | **You call it, you never create it.** It is the framework model, reachable as the protected `model` field on `DomainModelBase`. | [`ModelBase`](authoring/ModelBase.md) |
| `ModelConfiguration` | You read it. Never write to it. | [`ModelConfiguration`](authoring/ModelConfiguration.md) |
| `Budget` | You read it — typically to ask whether a candidate can be afforded. The framework does the spending. | [`Budget`](authoring/Budget.md) |
| `ModelParameter` | You read these. They are the model parameter definitions from the setup. | [`ModelParameter`](authoring/ModelParameter.md) |
| `jcDataSet` | **Yes** — it is what a side-car CSV becomes when you read it at setup. | [`jcDataSet`](authoring/jcDataSet.md) |
| `CSVHelper` | Yes — this is how you read a `supporting/` CSV in `SetupInstance`. | [`CSVHelper`](authoring/CSVHelper.md) |
| `DistributionSimulator` | Yes, in Monte Carlo models — you build these at setup from a coefficients CSV. | [`DistributionSimulator`](authoring/DistributionSimulator.md) |
| `MarkovTransitionSimulator` | Yes, in Monte Carlo models that step condition through discrete states. | [`MarkovTransitionSimulator`](authoring/MarkovTransitionSimulator.md) |
| `PieceWiseLinearModel` | Yes — the usual way to express a relationship a modeller calibrates as a curve. | [`PieceWiseLinearModel`](authoring/PieceWiseLinearModel.md) |
| `PieceWiseLinearModelGeneric` | Yes — the same idea, built from explicit x/y lists rather than a setup code. | [`PieceWiseLinearModelGeneric`](authoring/PieceWiseLinearModelGeneric.md) |
| `LogisticModel` | Yes, where deterioration or a probability is expressed as a logistic curve. Coefficients come from a CSV, never from C#. | [`LogisticModel`](authoring/LogisticModel.md) |
| `LinearRegressionModel` | Yes, for a fitted linear relationship. Coefficients come from a CSV, never from C#. | [`LinearRegressionModel`](authoring/LinearRegressionModel.md) |
| `NormalGenerator` | Yes, in Monte Carlo models needing normally-distributed draws. | [`NormalGenerator`](authoring/NormalGenerator.md) |

## The types you will see but not call

These appear inside the signatures above. Recognise them; do not construct them.
One line each in [`referenced.md`](referenced.md).

## Everything else

Framework internals. [`not-for-you.md`](not-for-you.md) says what to do when you find
yourself wanting one.
