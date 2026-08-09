# sample-inputs/

A working set of the **client-side** input files a Juno Cassandra model run reads — the files that
live in a project's `inputs/` folder, alongside but separate from your domain model's own bundle.

| File | What it is |
|---|---|
| `model_input_data.csv` | The network: one row per element, 1000 generic assets with an age, a condition rating, a material and an area. |
| `lookups.xlsx` | Thresholds and unit rates. Every `lkp_`-prefixed sheet is read and merged into one flat table addressed by (`lookup_set_name`, `setting_key`). This is where tunable numbers belong. |
| `budgets.xlsx` | One column per budget category, one row per period. A treatment charged to a category with no column here is **never funded, silently**. |
| `configurations.xlsx` | Run settings — model type, number of periods, discount rate, which budget sheet to use. |
| `mcda_setups.xlsx` | Weights and objective types for MCDA optimisation. |
| `kpi_setups.xlsx` | KPI definitions for post-processing, plus a sheet documenting every aggregator code. |
| `goalseek_setup.xlsx` | Variable ranges and objectives for goal-seeking runs. |

They pair with [`../DomainModelSample/`](../DomainModelSample/): `lookups.xlsx` carries the
`repair_thresholds`, `replace_thresholds` and `unit_rates` sets that model reads at setup, and
`budgets.xlsx` carries the `repair` and `replace` columns its treatments are charged to.

### The extra lookup sets, and why they are here

`lookups.xlsx` carries four more sets than the reference model reads, in `lkp_project` and a
`lkp_rates` sheet: `maintenance_thresholds`, `deterioration_rates`, `replacement_rates` and
`rate_factors`, plus `repair_thresholds.cond_factor` and `replace_thresholds.cond_after`.

They exist for `jcass-dm scaffold --from-sample`. The walking skeleton carries the reference
model's logic but is the artefact the engineer **keeps**, so it reads every one of its numbers out
of this file rather than out of C# — including the ones the reference model deliberately leaves
hard-coded as counter-examples ([`../DomainModelSample/README.md`](../DomainModelSample/README.md)
§ 7). Shipping a labelled counter-example into somebody's own starting project would teach the
shape they then copy for their next twenty thresholds.

The reference model reads a subset of these and is unaffected by the additions.

`lkp_rates` is also worth a look for its own sake: it holds two sets addressed by *material* rather
than by setting name, which is the shape to copy when a rate varies by a domain attribute. Adding a
sixth material is then a spreadsheet row and nothing in C#.

## This is a one-way snapshot — it is never synced back

Copied on **2026-08-09** from
`JCass_UnitTests\test_data\cassandra-unit-test-web\inputs\` in the Juno Cassandra framework
repository.

**Nothing here is ever copied back to that source, and changes there do not flow to here.** The
copy is deliberate rather than lazy. The originals are live test fixtures: they get edited to make a
test cover a new case, and those edits are driven by what the test needs, not by what reads well as
an example. Wiring the two together in either direction would mean a test change silently rewriting
a client-facing sample, or a documentation tweak silently breaking a test.

So this snapshot is free to be edited for clarity, and it will go stale. If it ever disagrees with
what the web app produces, **the web app is right**. Report the drift to support@lonrix.com.

## No client data is in here

Generic assets named `A1` to `A1000`, made of plastic, concrete, metal, cast-iron or titanium. No
client, no network, no site and no real condition data is represented anywhere in these files.
