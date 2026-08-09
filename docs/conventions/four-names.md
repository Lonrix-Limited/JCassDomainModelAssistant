# The four-name rule

**Four strings have to be identical, and nothing tells you when they are not.**

| # | Where |
|---|---|
| 1 | The `.csproj` filename |
| 2 | The assembly name — **inherited from #1; leave `<AssemblyName>` unset** |
| 3 | The entry class, `public class YourModel : DomainModelBase` |
| 4 | `meta.main_dll` and `meta.main_class` in `domain_model_setup.xlsx` |

**The full explanation is written once, in
[`DomainModelSample/README.md` § 3](../../reference-model/DomainModelSample/README.md#3-the-four-name-rule--read-this-before-you-rename-anything).**
It covers why the rule exists — a normal run and a debug F5 run resolve the model's identity by two
different routes, and the routes only ever agree when all four match — and the exact error you get
when they do not. Read it rather than re-deriving it; a half-remembered version of this rule is how
models get half-renamed.

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
