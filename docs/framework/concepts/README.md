<!-- ------------------------------------------------------------------
     GENERATED FILE - DO NOT EDIT BY HAND.

     Mirrored from the Juno Cassandra documentation source:

       jcass_docs2\intro\ (manifest: scripts\assistant\framework-concepts.json)

     by cassandra_main\scripts\assistant\sync-framework-concepts.ps1

     The sync is ONE-WAY. Any edit made here is lost the next time that
     script runs, without warning and without a merge conflict. To change
     what this page says, change the .qmd in the jcass_docs2 repository
     and re-run the sync.
     ------------------------------------------------------------------ -->

# Framework concepts

A mirror of the public Juno Cassandra concept documentation, flattened to plain markdown.
Roughly 13,000 words in total, which is far too much to load by default.
**Read the row you need, not the set.**

| # | Page | Read it when |
|---|---|---|
| 1 | [Model levels: project, framework model, domain model](01-model-levels.md) | You need the vocabulary for what sits where — what a *project* is, and how the framework model and the domain model relate. |
| 2 | [The framework model — what it does, and where your code is called](02-framework-model.md) | You need to know which half of the work is yours. This is the framework/domain split, stage by stage. |
| 3 | [Model execution stages](03-execution-stages.md) | You are deciding which method a piece of logic belongs in. The stages map one-to-one onto the methods on `DomainModelBase`. |
| 4 | [Raw input data vs model parameter data](04-data-components.md) | You are confusing raw input columns with model parameters, or wondering why a value you set does not persist between periods. |
| 5 | [Model types — which base class to inherit](05-model-types.md) | You are starting a model and need to choose between an MCDA model, a BCA model and the Monte Carlo variants. |
| 6 | [Definitions — epochs, periods, and the decision-making vocabulary](06-definitions.md) | You are about to use the words *epoch*, *period*, *NPV* or *BCA* in code or in conversation with the engineer. Getting these wrong is quietly expensive. |
| 7 | [The Benefit-Cost Analysis model and strategy generation](07-bca-model.md) | Your model triggers strategies rather than individual treatments, or you need to understand what the optimiser will do with what your trigger produces. |
| 8 | [Domain model setup — the full contract](08-domain-model-setup.md) | The heavyweight. Read it when you are wiring a domain model's setup, parameters and lookups, rather than as an introduction. |

These pages describe the framework, not your model. For the framework **API** - the types
and method signatures you actually call - see [`../api/README.md`](../api/README.md).
