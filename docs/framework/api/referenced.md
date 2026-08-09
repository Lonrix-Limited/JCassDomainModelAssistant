<!-- ------------------------------------------------------------------
     GENERATED FILE - DO NOT EDIT BY HAND.

     Generated from the framework reference assemblies and their XML
     documentation in refs\, by:

       cassandra_main\scripts\assistant\generate-api-reference.ps1

     The sync is ONE-WAY. Any edit made here is lost the next time that
     script runs, without warning and without a merge conflict. To change
     what this page says, change the /// documentation comments in the
     framework source, or the scoped surface in
     cassandra_main\scripts\assistant\api-surface.json, and regenerate.
     ------------------------------------------------------------------ -->

# Types you will see, but should not construct

These turn up inside the signatures in [`README.md`](README.md) — as a parameter type, a
return type, or something the framework hands you. You need to recognise the name. You
do not need to create one, and creating one yourself is usually a sign of having taken a
wrong turn.

| Type | Namespace | What it is |
|---|---|---|
| `TreatmentSet` | `JCass_ModelCore.Treatments` | The framework's collection of treatments selected in a period. Read-only, from your point of view. |
| `TreatmentStrategy` | `JCass_ModelCore.Treatments` | A sequence of treatments over time, assembled by the framework's strategy generator. |
| `ModelParameterData` | `JCass_ModelCore.ModelObjects` | The framework's per-element parameter storage across periods. |
| `JCColumn` | `JCass_ModelCore.ModelObjects` | A column definition in the model's input data. |
| `InputSet` | `JCass_ModelCore.ModelObjects` | A loaded set of model input data. |
| `NetworkStatistic` | `JCass_ModelCore.ModelObjects` | A network-level summary the framework calculates each period. |
| `enumDataType` | `JCass_Data.Objects` | The data type of a column - numeric or text. |
