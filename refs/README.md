# refs/

The Juno Cassandra framework assemblies your model compiles against, plus their `.xml`
documentation files. **They are committed here on purpose.** Clone or unzip this repository, open
it in VS Code, and everything resolves — `DomainModelBase`, `TreatmentInstance`,
`ModelConfiguration` and the rest — with no download step and nothing to configure.

## They are reference assemblies, and they do not run

These are **reference assemblies**: the framework's complete public API with every method body
removed. The .NET SDK produces them as a normal part of building, and they are what the compiler
actually needs. Three consequences, and the third is the one to know about:

- **The compiler is satisfied.** Types, methods, properties, overloads and parameter names are all
  present, so your model builds exactly as it would against the real assemblies.
- **IntelliSense is complete.** Signatures come from the assembly, the descriptions and parameter
  documentation come from the `.xml` files beside it.
- **The runtime refuses to load them.** You cannot execute the framework on your own machine.

That last point is the design, not a defect. You **author** locally and you **run and debug** in
the web app's Debug Model page, where the real framework is, where the client's data is, and where
a breakpoint in your source will actually stop.

### What it looks like when you try

If you build something that attempts to run the framework locally, it compiles cleanly and then
fails the moment the assembly is loaded:

```
Unhandled exception. System.BadImageFormatException: Could not load file or assembly
'JCass_ModelCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'. Reference assemblies
cannot be loaded for execution. (0x80131058)
 ---> System.BadImageFormatException: Cannot load a reference assembly for execution.
```

Any framework assembly can appear in place of `JCass_ModelCore`. **This is expected.** Nothing is
corrupt, nothing is missing, and re-downloading will not change it. Build locally; run in the
browser.

### Be clear about what this is not

It is a **deterrent against casual local running, not a security control.** Anyone holding a set
of framework assemblies from elsewhere can host a domain model on their own machine. What actually
governs who may run Juno Cassandra is its licensing, not the shape of the files in this folder. It
is here so that this repository can be public and still let you compile — that is the whole of it.

## The `.xml` files are the most valuable thing here

Each `.xml` file carries the framework's own documentation: every public signature, what each
method is for, what each parameter means. That is what an AI coding assistant reads to learn an
API it has never seen and cannot otherwise discover, and it is what the generated reference in
[`../docs/framework/`](../docs/framework/) is built from.

They roughly triple the size of this folder and look like something a tidy-up would remove.
**Do not remove them.** A `refs/` folder with only the `.dll` files looks perfectly healthy and
quietly costs you every framework description you would have seen.

## Which framework build this is

[`FRAMEWORK-VERSION.txt`](FRAMEWORK-VERSION.txt) records it. The `ProductVersion` column carries
the framework's own **git commit SHA**, so two sets can be compared exactly rather than by date or
by guess.

```powershell
.\scripts\check-framework-version.ps1
```

### When it is stale, and why that bites

These are a snapshot, and snapshots go stale. Compiling against an older framework than the server
runs produces behaviour that differs between your machine and the real run, with **no error to
point at it** — a method whose behaviour has since changed, or one that did not exist yet.

The web app does not currently display the framework version it is running, so there is no screen
to compare this against. What the stamp buys you is a precise thing to *quote*: if your model
behaves in a way you cannot explain from your own code, email **support@lonrix.com** with this SHA
and the question is answerable in one reply.

Rule out the ordinary causes first, though. A version mismatch is the rarer explanation — your
model, the client's input data, or a lookup value that differs from the one you tested against are
all more likely.

**You do not refresh this folder yourself.** There is no script here for it and no set of DLLs to
go and find. A newer framework arrives with a newer release of this Assistant: you are told there
is one, you download it, and `refs/` is replaced along with everything else. Your own model lives
in its own folder beside this one and is never touched by that.

## On the web Debug Model page

You need none of this. That workspace stages its own `refs/` when you initialise it, and stages
rather more than this folder holds, because it includes NuGet dependencies and the framework host
itself.

**Do not include `refs/` in a zip you upload to the Debug Model page.** The upload filters it out,
and that is deliberate: overwriting part of the workspace's staged set with reference assemblies
would replace working framework assemblies with ones that cannot run.

## Getting a framework call that is not here

If you need something that is not in these assemblies, stop rather than guessing at a signature.
Email **support@lonrix.com** with what you were trying to do, the exact error, the commit SHA from
`FRAMEWORK-VERSION.txt`, and your model's name.
