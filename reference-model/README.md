# reference-model/

A complete, working domain model, plus a set of inputs it runs against. This is the **worked
reference** — something to read and compare against, not the thing you start from.

| | |
|---|---|
| [`DomainModelSample/`](DomainModelSample/) | A deliberately small working model. Its own [`README.md`](DomainModelSample/README.md) is the best documentation in this repository until `docs/` is written. |
| [`sample-inputs/`](sample-inputs/) | A snapshot of the client-side input files the model reads. See its [`README.md`](sample-inputs/README.md). |

## Do not start by renaming this

It is tempting, and it is the wrong move. Renaming a model means changing four separate strings
that must stay identical — the `.csproj` filename, the assembly name, the entry class and the
bundle's `meta` sheet — and getting one wrong produces a failure that only appears when you press
F5. That is the highest-frequency way to break a new model.

`jcass-dm scaffold` (session S6) emits a correctly-named project instead, so the mismatch cannot
happen. Read `DomainModelSample/README.md` section 3 for why the four names matter.

## Two things in here are wrong on purpose

`TreatmentTrigger.RoutineMaintenanceConditionGreaterThan` and the per-material deterioration and
cost rates in `SampleElement` are hard-coded in C#. **Do not copy that.** They are counter-examples,
kept as the contrast that makes the rule visible: tunable numbers belong in `inputs/lookups.xlsx`,
where a modeller can change them and re-run without a developer, a rebuild and a republish.
`DomainModelSample/README.md` section 7 sets moving them as the reader's first two exercises.

## Building it

Nothing to fetch — the framework assemblies are committed in `DomainModelSample/refs/`. From the
repository root:

```powershell
dotnet build reference-model/DomainModelSample/DomainModelSample.csproj -c Debug --no-incremental
```

Expect `0 Warning(s), 0 Error(s)`.

**Building it is as far as you get locally, and that is by design.** Those are *reference*
assemblies — full public API, no method bodies — so they compile and give you complete IntelliSense
and the runtime refuses to load them. Models run and are debugged in the web app's Debug Model
page. [`../refs/README.md`](../refs/README.md) explains it, including what the failure looks like
if you try.
