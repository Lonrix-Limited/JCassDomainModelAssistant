# examples/

Small, focused, **compiling** examples of individual patterns — one idea each, in contrast to
[`../reference-model/DomainModelSample/`](../reference-model/DomainModelSample/), which is a whole
small model.

**Start at [`../docs/patterns/`](../docs/patterns/README.md).** These files are the code the pattern
pages quote; the pages carry the reasoning, and reading a file here without its page gives you the
shape and none of the *why*.

---

## `ExamplesLibrary/`

One `.cs` file per pattern, plus a small `Shared/` folder holding the element class and the host
model the examples operate on.

```powershell
dotnet build .\examples\ExamplesLibrary\ExamplesLibrary.csproj -c Debug --no-incremental
```

**It is built in CI, with warnings as errors.** That is the whole reason it exists as a project
rather than as code blocks in markdown: an example that does not compile rots, and a rotted example
is worse than no example because it gets quoted with confidence. A framework change that invalidates
a documented pattern fails here rather than in a client's model.

### It is a library, not a template

**Do not copy this folder and rename it.** Nothing here is named for a real model, nothing here
implements a complete one, and `PipeNetworkModel` throws from five of its seven methods on purpose.

To start a model:

```powershell
.\tools\jcass-dm.exe scaffold MyModel --from-sample --output ..\MyModel
```

See [`../docs/workflow/10-scaffold-and-build.md`](../docs/workflow/10-scaffold-and-build.md).

### The domain is fictional, and deliberately

Everything here is a buried water main network — pipe segments, condition grades, break rates,
relining. The working models these patterns were mined from carry client IP: calibrated
coefficients, business rules, sometimes client names. **The shape travels; the numbers do not.**
Every threshold and rate in every example is read from `lookups.xlsx` through the `Constants`
pattern, so there is no calibration here to leak and none to copy by mistake.

---

## Why a `.csproj` is safe here

The Assistant is **never uploaded** to the Debug Model page. Only the engineer's own model folder is,
and it is uploaded whole, rooted at its single `.csproj`. Since the model is always a sibling folder
and never inside this repository, the `.csproj` files under `examples/` and `reference-model/` can
never end up in an upload. [`../docs/conventions/naming-and-folders.md`](../docs/conventions/naming-and-folders.md).
