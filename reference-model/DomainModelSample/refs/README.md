# refs\ — the reference model's copy

Byte-identical to [`../../../refs/`](../../../refs/), which is where the explanation lives. Read
that one.

The short version: these are **reference assemblies** — the framework's full public API with no
method bodies, plus the `.xml` documentation files that carry its descriptions. They compile, they
give complete IntelliSense, and the runtime refuses to load them. You author locally; you run and
debug in the web app.

## Why there are two copies

The project references framework assemblies with a wildcard, `<Reference Include="refs\*.dll">`,
resolved **relative to the `.csproj`**. That has to stay that way: the web Debug Model workspace
stages its own framework assemblies into exactly that folder, so the same `.csproj` builds
unchanged on your machine and on the server. A project reaching up to a shared folder somewhere
above it would build locally and break the moment it was uploaded.

So every project that compiles against the framework carries its own `refs\`, seeded from the one
at the repository root. Identical bytes, duplicated on purpose.

## Do not edit anything in here

Not the assemblies, not the `.xml` files, and not
[`FRAMEWORK-VERSION.txt`](FRAMEWORK-VERSION.txt) — it is generated. Never mix assemblies from two
framework releases in one folder; the wildcard picks up whatever is present, so a leftover from an
older release is compiled against rather than ignored.
