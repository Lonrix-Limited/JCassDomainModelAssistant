# The four-name rule

**Four strings have to be identical.** `jcass-dm check` tells you on your machine when they are not,
and on the web Debug Model page **Check bundle** tells you, and publish refuses.

| # | Where |
|---|---|
| 1 | The `.csproj` filename |
| 2 | The assembly name — **inherited from #1; leave `<AssemblyName>` unset** |
| 3 | The entry class, `public class YourModel : DomainModelBase` |
| 4 | `meta.main_dll` and `meta.main_class` in `domain_model_setup.xlsx` |

**Exactly identical, with two precisions the web app enforces:**

- **`main_dll` must match `<ProjectName>.dll` letter for letter, case included.** The server runs
  Linux and resolves it as a file path, so `toymodel.dll` and `ToyModel.dll` are different files. A
  case-only difference is refused at publish — it used to publish and then fail every run.
- **`main_class` is the bare project name, with no namespace.** `ToyModel`, never
  `MyRoads.ToyModel`. (Letter case does not matter to the server here, but keep it identical anyway
  — the rule is four identical strings, and `jcass-dm check` holds you to that.)

**The full explanation is written once, in
[`DomainModelSample/README.md` § 3](../../reference-model/DomainModelSample/README.md#3-the-four-name-rule--read-this-before-you-rename-anything).**
It covers why the rule exists — a normal run and a debug F5 run resolve the model's identity by two
different routes, and the routes only ever agree when all four match. Read it rather than
re-deriving it; a half-remembered version of this rule is how models get half-renamed.

### Which disagreement is caught where

The two routes mean a mismatch fails in a different place depending on **which** names disagree:

| What disagrees | F5 on the Debug Model page | Check bundle | Publish | `jcass-dm check` |
|---|---|---|---|---|
| The entry class (#3) against the `.csproj` (#1), or `<AssemblyName>` set | **Fails** — *"Domain Model class 'X' was not found in the specified .dll"* | Cannot see it — it does not read C# | **Not caught** — publish compares the spreadsheet with the project name, never the C#. Do not publish a model F5 cannot run | **Reports it** |
| The `meta` sheet (#4) against the `.csproj` (#1) | **Runs**, with a `[DebugMode] Overriding DM DLL name` warning | **Fails** — *"Bundle main_dll + main_class match the C# project"* | **Refuses** — nothing published | **Reports it** |

The second row is the one that used to hurt: F5 is green on every run, so the mismatch sat there
until the first normal run after publish. Check bundle now catches it as soon as the edited workbook
is uploaded. **Treat a Check bundle failure on this check as a stop for publishing**, never as noise.

---

## What you actually do about it

**Never rename by hand.** Four edits, one of them two cells inside a binary spreadsheet, is exactly
how the names drift apart. There is a verb, and it changes all four together or changes nothing:

```powershell
.\tools\jcass-dm.exe rename NewModelName --project ..\TheirModel
```

**A new model should never need renaming at all.** `scaffold` writes all four from the single name
you give it, so they cannot start out disagreeing:

```powershell
.\tools\jcass-dm.exe scaffold MyRoadModel --from-sample --output ..\MyRoadModel
```

**An inherited model very often does need it** — names that already disagree are one of the most
common things `check` finds. Run `check` first, always:

```powershell
.\tools\jcass-dm.exe check --project ..\TheirModel
```

The `the four names` rule reports which of the four disagree and what they currently read.

---

## After a rename — carry it to the web app

`rename` fixes your machine. The server still holds the old source, the old workbook, and in an older
workspace a copy of the old DLL. **Two upload buttons and a possible reset**, in this order — taking
`StarterModel` → `ToyModel` as the example:

1. **Upload the renamed source.** `jcass-dm package`, then **Upload zip** on the Debug Model page,
   and say yes to wiping `domain_model_source/` — otherwise `StarterModel.csproj` stays beside
   `ToyModel.csproj` and the workspace has two projects
   ([`silent-failures.md` § 7](silent-failures.md#7-two-csproj-files-at-the-project-root)).
2. **If the bundle file list still shows `StarterModel.dll`, click Reset debug bundle.** Older
   workspaces were seeded with a copy of the published model DLL; newer ones are not, and Initialize
   and Reset no longer copy it (third-party DLLs are still copied). A leftover old DLL has no build
   twin, so publish would ship it beside the new one. Reset empties `debug_domain_model/` and
   re-seeds it from the live version — **which also puts the live version's workbook back**, so:
3. **Upload the edited `domain_model_setup.xlsx` with Upload bundle file(s) — after the reset, never
   before.** The workbook inside your source zip does not count; a run reads only the one in
   `debug_domain_model/`.
4. **Check bundle.** *"Bundle main_dll + main_class match the C# project"* must pass.
5. **F5, then publish** when the engineer asks for it — [`../workflow/40-publish.md`](../workflow/40-publish.md).

Upload bundle file(s) never deletes anything, and uploading `ToyModel.dll` while `StarterModel.dll`
is selected adds a file rather than swapping one. Reset debug bundle is the only way to remove a
bundle file. Both buttons are browser steps — walk the engineer through them; they are not yours to
run.

---

## Two things that are *not* one of the four

**The namespace.** `rename` leaves it alone unless you ask. Nothing resolves a domain model by
namespace, so a stale one is confusing rather than broken. Pass `--namespace` if the engineer wants
it tidied.

**The element class name** — `RoadSegment`, `Bridge`, whatever an asset is called in their network.
That is an ordinary class name, chosen freely, and `scaffold --element` sets it. Only the four above
are resolved by string from outside the project.

---

## Related

- `<AssemblyName>` set, and the F5 failure it produces:
  [`silent-failures.md` § 6](silent-failures.md#6-assemblyname-set-in-the-csproj)
- Two `.csproj` files at the root, the other identity failure:
  [`silent-failures.md` § 7](silent-failures.md#7-two-csproj-files-at-the-project-root)
