# Fixtures — deliberately broken domain models

Every model in this folder is wrong on purpose, in exactly one way.

They exist because **a check that has never been seen to fail has never been tested.** A rule can
read the wrong column, or look at a file that is not there, and report `OK` either way. Testing
only a healthy model would prove the output formatting and nothing else. So each rule
`jcass-dm check` applies has a model here that trips it, and
[`../src/JcassDm.Tests/CheckTests.cs`](../src/JcassDm.Tests/CheckTests.cs) asserts that it does.

**None of these is an example to copy.** If you want a model to start from, run
`jcass-dm scaffold`.

---

## What each one breaks

All nine start from the same base — `jcass-dm scaffold FixtureModel --element ModelElement
--from-sample` — and then have one thing done to them.

| Folder | What is wrong | Rule it trips |
|---|---|---|
| `healthy` | nothing | the control. The only one `check` should pass |
| `names-disagree` | the `.csproj` was renamed to `InheritedRoadModel.csproj`; the entry class is still `FixtureModel` | the four names |
| `parameter-not-written` | `par_obj` is on the `parameters` sheet and its line is gone from `SetParameterValues` | parameters vs C# |
| `treatment-not-in-bundle` | `TreatmentNames.Reseal = "reseal"` exists in C# with no row on the `treatments` sheet | treatments vs C# |
| `treatment-not-in-code` | the `treatments` sheet declares `overlay`; no C# mentions it | treatments vs C#, and reset arms |
| `missing-reset-arm` | `replace` is declared and triggered, and `Resetter` has no `case` for it | treatment reset arms |
| `two-csproj` | a second `.csproj` at the root | one `.csproj` at the root |
| `assembly-name-set` | `<AssemblyName>SomethingElse</AssemblyName>` in the `.csproj` | `<AssemblyName>` |
| `blank-budget-category` | `RMaint` has an empty `budget_category` cell | budget categories |

`names-disagree` does double duty: it is also **the `rename` test case**, because it is a model
whose four names genuinely disagree, which is the state `rename` exists to fix. See
[`../src/JcassDm.Tests/RenameTests.cs`](../src/JcassDm.Tests/RenameTests.cs).

---

## Two things about them worth knowing

**They have no `refs/`, so they do not compile.** That is deliberate and it is fine: `check` reads
C# as text rather than building it. Keeping them source-only holds each one to a few kilobytes in
a repository that already commits a 37 MB executable. A test that needs a fixture to build copies
it and drops `refs/` in first.

**They are snapshots, and they are allowed to go stale.** They test `check`'s rules, not the
scaffolder's templates. If the templates change, these do not need regenerating — a fixture that
still trips its rule is still doing its job.

---

## Adding one

Three steps, and the middle one is the point:

1. Copy `healthy`, and break one thing.
2. **Watch the new rule fail on it, then fix the fixture and watch it pass.** A fixture that has
   only ever been seen failing does not prove the rule can tell the difference.
3. Add it to `FixtureModels.All` in
   [`../src/JcassDm.Tests/FixtureModels.cs`](../src/JcassDm.Tests/FixtureModels.cs) and to the
   `[Theory]` in `CheckTests`. A test asserts that the list and the folders on disk match, so a
   fixture added without a test fails the build rather than sitting there uncovered.

Most breakages are a `sed` on a source file. Two needed more:

- `treatment-not-in-code` was made with
  `jcass-dm add-treatment <bundle> --name overlay --category overlay --budget-category renewals
  --description "Structural overlay"`.
- `blank-budget-category` needed the `budget_category` cell on row 4 of the `treatments` sheet
  cleared directly, because `jcass-dm` refuses to write an empty one — which is the behaviour the
  fixture is testing the *other* end of.
