# .claude/ — skills for Claude Code

Nine skills, and a permission allowlist. They are a **convenience layer** over the `jcass-dm` verbs
in [`../tools/`](../tools/) and reads of [`../docs/`](../docs/).

**A skill never holds knowledge that is not also in `docs/`.** The test is deliberately blunt:
delete this whole folder and the Assistant still works at full capability, with more typing.
Anything a skill does, an assistant that has never heard of skills must be able to do by reading
`docs/` and calling `jcass-dm`. That matters because Copilot and Cursor have no skill mechanism, and
a skill that knew something `docs/` did not would quietly make this a two-tier product.

So every page here is a routing table: *read this doc, run this verb, ask the engineer this.* If you
find yourself wanting to explain something in a skill, the explanation belongs in `docs/` and the
skill should link to it.

---

## The nine

| Skill | Wraps |
|---|---|
| `new-domain-model` | [`workflow/10-scaffold-and-build.md`](../docs/workflow/10-scaffold-and-build.md) · `scaffold`, `set-meta`, `check` |
| `adopt-existing-model` | [`workflow/05-adopt-an-existing-model.md`](../docs/workflow/05-adopt-an-existing-model.md) · `check`, `rename`, `dump` |
| `check-my-model` | [`conventions/silent-failures.md`](../docs/conventions/silent-failures.md) · `check` |
| `add-treatment` | [`workflow/30`](../docs/workflow/30-make-a-change.md#add-a-treatment) · [`patterns/treatment-instances.md`](../docs/patterns/treatment-instances.md), [`treatment-suitability-scoring.md`](../docs/patterns/treatment-suitability-scoring.md), [`candidate-strategies.md`](../docs/patterns/candidate-strategies.md) · `add-treatment` |
| `add-parameter` | [`workflow/30`](../docs/workflow/30-make-a-change.md#add-a-model-parameter) · `add-parameter`, `dump` |
| `add-input-column` | [`workflow/30`](../docs/workflow/30-make-a-change.md#add-an-input-column) · `add-input-header`, `dump` |
| `add-lookup-constant` | [`patterns/constants-from-lookups.md`](../docs/patterns/constants-from-lookups.md), [`setup-data-from-supporting-csv.md`](../docs/patterns/setup-data-from-supporting-csv.md) · `check --lookups` |
| `package-for-upload` | [`workflow/20-upload-and-debug.md`](../docs/workflow/20-upload-and-debug.md) · `package` |
| `draft-support-request` | [`support-request-template.md`](../docs/support-request-template.md) |

Three properties every one of them inherits, because they are properties of the Assistant rather
than of any skill:

- **It honours the verb.** *"Guide me through adding a treatment"* gets a lesson;
  *"add a treatment called reseal"* gets the change. A skill is never the way round the teaching —
  [`docs/00-start-here.md` § 2](../docs/00-start-here.md).
- **It scaffolds a stub and asks; it never supplies engineering judgement.** No invented
  deterioration rate, trigger threshold or unit rate. Where a number is required, the place for it
  gets built and the question gets asked.
- **The answer goes in `lookups.xlsx`, not in C#.** Being told a value is not permission to
  hard-code it — [`docs/conventions/where-numbers-live.md`](../docs/conventions/where-numbers-live.md).

And every one of them defers to
[`docs/conventions/when-to-stop.md`](../docs/conventions/when-to-stop.md) rather than restating it.
None of them has a "if that did not work, try this instead" branch: if a step cannot be completed as
written, that is a stop, and the exit is `draft-support-request`.

---

## `settings.json`

**Build and read-only commands are pre-allowed. Everything that writes prompts.**

Allowed without asking: `dotnet build`, `jcass-dm check`, `jcass-dm dump`, `jcass-dm version`, and
the two maintenance scripts in [`../scripts/`](../scripts/), which only report.

**Deliberately not allowed:** `scaffold`, `rename`, `package`, `set-meta` and the three `add-`
verbs. They write to the engineer's model, so the engineer approves each one. The prompt is the
point, not friction to be tuned away.

**Publish is not in the allowlist and cannot be.** There is no `jcass-dm` publish verb — publishing
is a browser button behind an admin grant a modeller cannot give themselves, and it overwrites the
one version the client runs. The `deny` list blocks `curl`, `wget` and the PowerShell web cmdlets so
that the skills cannot reach the web API directly either; nothing in building a domain model needs
them. [`docs/workflow/40-publish.md`](../docs/workflow/40-publish.md).
