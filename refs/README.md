# refs/ — placeholder

**Filled by session S2.**

Will hold the Juno Cassandra framework **reference assemblies** and their `.xml` documentation
files, so that a model compiles and gives full IntelliSense straight after a clone.

Reference assemblies carry the public API with no method bodies. They compile and IntelliSense
resolves against them, and the runtime refuses to load them — which is exactly right here: you
author locally and you run and debug in the web app. Treat that as a deterrent against casual local
running, **not** as a security boundary; the real control is Juno Cassandra's licensing.

The `.xml` files are the most valuable thing in this folder. They are how an AI assistant learns
the framework API, and they are the input to the generated reference in
[`../docs/framework/`](../docs/framework/).

Until S2 lands this folder is empty, and the reference model can only be built on a machine that
already has a local set of framework assemblies. See
[`../reference-model/README.md`](../reference-model/README.md).
