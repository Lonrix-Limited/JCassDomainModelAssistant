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

# Everything not in this reference

Everything in `JCass_*` that is not listed in this reference is framework internals: the model controller, the optimisers, the exporters, the setup loaders, the web worker's plumbing. None of it is secret and none of it is yours to call. The framework calls your domain model; your domain model does not reach back into the framework.

## The test

> **Is the framework call you are about to write listed in this reference?**

**Yes** — proceed. Compose it with the documented patterns as freely as the problem needs.

**No** — stop. Do not guess the signature, do not infer it from a similar method, and do
not copy it out of a model you found elsewhere. Write the engineer a support request for
`support@lonrix.com` containing:

- what you were trying to make the model do, in modelling terms;
- the framework call you wanted and why the reference does not cover it;
- the framework build stamp from [`README.md`](README.md);
- the model name.

This test is deliberately mechanical. "Is it on the list?" is something you can actually
decide, where "am I confident enough about this API?" is not.

It is **not** a ban on undocumented work. Most real modelling is composition rather than
exact match, and a rule that fires on every second edit gets switched off — at which
point it is not there for the case that mattered. If you are building something new out
of listed API, build it, and say plainly that it is not a canonical pattern and is worth
checking with Lonrix.

## What is on the list

The 19 types in [`README.md`](README.md), with all their members. Everything else in
`JCass_*` — model controllers, optimisers, exporters, setup loaders, the web worker's
plumbing — is framework internals. It is not hidden from you, and it is still not yours
to call: the framework calls your domain model, not the other way round.
