<!-- ------------------------------------------------------------------
     GENERATED FILE - DO NOT EDIT BY HAND.

     Mirrored from the Juno Cassandra documentation source:

       jcass_docs2\intro\domain_model_setup.qmd

     by cassandra_main\scripts\assistant\sync-framework-concepts.ps1

     The sync is ONE-WAY. Any edit made here is lost the next time that
     script runs, without warning and without a merge conflict. To change
     what this page says, change the .qmd in the jcass_docs2 repository
     and re-run the sync.
     ------------------------------------------------------------------ -->

> **Read this when:** The heavyweight. Read it when you are wiring a domain model's setup, parameters and lookups, rather than as an introduction.

# Domain Model Setup

## Overview

This document describes the structure of the template file that contains setup
information for your Domain Model. 

In Juno Cassandra, your Domain Model logic that controls Initialisation, Incrementing,
Treatment Triggering and Resetting needs to be contained in a compiled .dll file, based on a Visual Studio C# class library project. More information about the interface of this library can be found in other parts of this documentation. Lonrix can also provide you with a working example of such a project.

Although the logic for your domain model is encapsulated in the compiled .dll file, you still need to provide Cassandra with key information related to your model so that Cassandra can prepare the necessary structure and allocate memory to hold your data at runtime.

This information needs to be provided in your **Domain Model Setup file** which is an Excel file containing the following information:

* Input Headers - containing information about all the columns in your input data.

* Model Parameters - containing information about parameters that your model will use to forecast future network condition and make decisions along the way.

* Treatments - a definition of all treatments/interventions that your Domain Model uses.

* MCDA Setup - containing the parameters and weighting factors to use in a Multi-Criteria Decision Analysis (MCDA) model

* BCA Strategies - containing all treatment strategies that your Domain Model will use in a Benefit-Cost Analysis (BCA Model)

* Network Functions - containing information related to model parameters that require calculation over all elements in the network

Details on each of the above information sets are provided below. You can download an example template for specifying your Domain Model setup by clicking the button below.

<a href="../templates/domain_model_setup_template.xlsx" download class="download-btn">
  Download Domain Model Setup Template
</a>

> **Note**
>
> The above template file, as well as the examples shown below are provided as an example only. Cassandra allows you to design and specify your own Domain Model from scratch. Thus, if you are designing your own Domain Model, you will be looking to replace the data in the template with your own design. 
>
> However, the *structure* of the setup file (i.e. required sheet names and specified column names) should remain as shown in the examples.

---

<a id="input-headers"></a>

## Input Headers

Input headers specify all columns that the your Domain Model expects to be present in the input data as well as the data type expected for each column. 

Input headers must be specified using the template shown below, and must be contained on a sheet named **'input_headers'** (presume case-sensitive).

At runtime, Cassandra will scan your input file and report an error if:

* Any of the specified columns are not present in your input set

* Any column indicated as being numeric contains non-numeric or missing data

| category | column_name | data_type | example | comment |
|---|---|---|---|---|
| idenfification | file_seg_name | text | Segment32 | Segment identifier |
| idenfification | file_section_id | number | 724 | SectionID |
| idenfification | file_section_name | text | BIRDNAME ROAD | Name of Section |
| idenfification | file_loc_from | number | 430 | Start metre |
| idenfification | file_loc_to | number | 1445 | End metre |
| idenfification | file_lane_name | text | All | lane code |
| quantity | file_length | number | 1015 | Length of segment in metres |
| quantity | file_area_m2 | number | 10332 | Square metre area |
| trigger | file_can_treat_flag | text | True | Can this segment be considered for treatment (change as needed based on client policy) |
| trigger | file_can_rehab_flag | text | False | Can this segment be considered for Rehab (client specific change as needed based on client policy) |
| trigger | file_ac_ok_flag | text | True | Is the pavement suitable for asphalt resurfacing (based on current delfection/remaining pavement life/condition - subject to client policy/thresholds) |
| surfacing | file_surf_class | text | cs | Surface Class (must be one of: 'cs' or 'ac', 'blocks','concrete','other') |
| trigger | file_next_surf | text | ac | What is the replacement surfacing type ('cs' or 'ac', 'blocks','concrete','other') (change as needed based on client policy) |
| trigger | file_earliest_treat_period | number | 1 | Specify the earliest possible modelling period that the first treatment may be triggered (flag for fine control of treatment selection on certain elements) |
| road | file_urban_rural | text | U | Urban/Rural Tag |
| road | file_onrc | text | secondary collector | ONRC Category |
| traffic | file_adt | number | 2463 | Average Daily Traffic |
| traffic | file_heavy_perc | number | 5 | Heavy Vehicle Percentage |
| traffic | file_no_of_bus_routes | number | 1 | Number of Bus Routes - Can be used in MCDA Model to lend greater weight to roads with more bus routes |
| traffic | file_traff_growth_perc | number | 2 | Traffic Growth Percent |
| surfacing | file_surf_date | text | 2003-02-13 00:00:00 | Surfacing Date dd/mm/yyyy (will be use to determine Surfacing Age using 'base_date' value in 'General' Lookup set) |
| surfacing | file_surf_function | text | 2 | Surface Function |
| surfacing | file_surf_material | text | RACK | Surfacing Material |
| surfacing | file_surf_life_expected | number | 10 | Surfacing Expected life from RAMM. Important factor that determines surface remaining life and plays a role in S-Curve factors for distresses |
| surfacing | file_surf_layer_no | number | 1 | Surfacing layer number |
| surfacing | file_surf_thick | number | 9 | Surfacing thickness, in mm |
| pavement | file_pave_date | text | 1976-02-08 00:00:00 | Pavement Construction Date dd/mm/yyyy (will be use to determine Pavement Age using 'base_date' value in 'General' Lookup set) |
| pavement | file_pave_remlife | number | 20 | Age based pavement remaining life |
| maint_fault | file_su_fault_qty | number | 0 | Surfacing Faults in Square Metres (open dispatches) -plays a role in calculation of Surfacing Distress Index |
| maint_fault | file_pa_fault_qty | number | 0 | Pavementy Faults in Square Metres (open dispatches) -plays a role in calculation of Pavement Distress Index |
| hsd | file_rough_survey_date | text | 2023-03-27 00:00:00 | Roughness Survey Date dd/mm/yyyy (use to determine Survey Age using 'base_date' value in 'General' Lookup set). Used in turn to determine if survey is outdated. |
| hsd | file_naasra_85 | number | 110 | Naasra 85th Percentile |
| hsd | file_rut_survey_date | text | 2023-03-27 00:00:00 | Rut Survey Date dd/mm/yyyy (use to determine Survey Age using 'base_date' value in 'General' Lookup set). Used in turn to determine if survey is outdated. |
| hsd | file_rut_lwpmean_85 | number | 4 | LWP Mean Rut 85th percentile |
| hsd | file_rut_rwpmean_85 | number | 4 | RWP Mean Rut 85th percentile |
| distress | file_cond_survey_date | text | 2023-03-18 00:00:00 | Condition survey Date dd/mm/yyyy (use to determine Survey Age using 'base_date' value in 'General' Lookup set). Used in turn to determine if survey is outdated. |
| distress | file_pct_allig | number | 0 | Alligator/Mesh Cracks percent of segment length |
| distress | file_pct_lt_crax | number | 1.06 | L&T Cracks percent of segment length |
| distress | file_pct_poth | number | 0.0043 | Potholes percent of segment length |
| distress | file_pct_scabb | number | 0.15 | Scabbing percent of segment length |
| distress | file_pct_flush | number | 0 | Flushing percent of segment length |
| distress | file_pct_shove | number | 0 | Shoving percent of segment length |
| distress | file_pct_edgebreak | number | 0 | Edge Breaks percent of segment length |

> **Important**
>
> The table shown above must have left-upper corner in cell A1 of the 'input_headers' sheet. You can use the cells below or to the right of the table but it is recommended that you rather make use of a separate sheet to keep any notes etc.
>
> Keep column names unchanged (case-sensitive) and do not change the order of columns.

<a id="model-parameters"></a>

## Model Parameters

The Model Parameters table represent the aspects of each network element that you want to forecast over each modelling period. Whereas the values in your input data always remain fixed, model parameters will change over time.

Model Parameters must be specified using the template shown below, and must be contained on a sheet named **'parameters'** (presume case-sensitive).

> **Important**
>
> To optimise running performance and minimise memory requirements at runtime, you should design your Domain Model so that it uses as few Model Parameters as possible. At runtime, the model needs to loop - for each modelling period - over all model elements, then all model parameters. By reducing the number of Model Parameters by 20%, your model will run at least 20% faster.
>
> As a general guideline, you should not use a Model Parameter to represent any facet of a network element unless it changes over time. Aspects that do not change over time, such as 'region', 'name', 'rainfall' etc. should be kept in the input data and not duplicated in Model Parameters. The Framework Model gives your Domain Model access to the input data at runtime, so you can easily find say the 'region' for a specific element if you need it for decision making, increments etc.
>
> Model parameters can be either Numeric ('number') or Text ('text'). Cassandra is configured to perform optimally with numeric parameters. While provision is made for text parameters, you should try and use numeric parameters wherever possible as this will greatly improve performance.
>
> For example, if you have a text parameter called 'priority' with possible values 'low', 'medium', 'high' or 'very high', you should consider converting this to a numeric parameter with a mapping of names to values. For example, 'low' = 0, 'medium' = 1, etc. This will greatly speed up running time of your model.
>
> As much as you can, you should try to design your model to avoid Text Parameters that may have an unlimited number of possible values, as these parameters have the greatest impact on model runtime. There are cases, however, where you cannot avoid using such parameters. Examples are when you use a text value to represent a State Probability Vector in Markov models (e.g. text parameter holding states like this: '0.20|0.35|0.25|0.1|0.05|0.05').

The table below shows an example of a Parameter Definition table:

| parameter_name | data_type | minimum | maximum | decimals | comment |
|---|---|---|---|---|---|
| para_adt | number | 0 | 100000 | 1 | Average Daily Traffic |
| para_hcv | number | 0 | 50000 | 1 | Percentage heavy vehicles (ranges from 0 to 100) |
| para_pave_age | number | 0 | 200 | 1 | Pavement age, in years |
| para_pave_remlife | number | -100 | 100 | 1 | Pavement Remaining Life, in years |
| para_pave_life_ach | number | 0 | 200 | 1 | Pavement Life Achieved (pavement age as a percentage of life (age plus remaining life)) |
| para_hcv_risk | number | 1 | 25 | 1 | Pavement Risk factor based on traffic loading and pavement life achieved |
| para_surf_mat | text |  |  |  | Surface Material Code |
| para_surf_class | text |  |  |  | Surface Class ('cs' or 'ac', 'blocks','concrete','other') |
| para_surf_cs_flag | number | 0 | 1 | 0 | When Surface Class is 'cs', then 1, else 0 |
| para_surf_cs_or_ac_flag | number | 0 | 1 | 0 | When Surface Class is 'cs' or 'ac', then 1, else 0 |
| para_surf_road_type | text |  |  |  | Concatenate Road Type and Surface Class for use in various lookups |
| para_surf_thick | number | 0 | 500 | 1 | Surface thickness in mm |
| para_surf_layers | number | 0 | 20 | 1 | Number of Surface Layers |
| para_surf_func | text |  |  |  | Surface Function (First coat = '1', Second Coat = "2', Reseal = 'R') |
| para_surf_exp_life | number | 0 | 200 | 1 | Surface Expected Life, in years (initially from RAMM, then based on experience after treatment) |
| para_surf_age | number | 0 | 999 | 1 | Surface age, in years |
| para_surf_life_ach | number | 0 | 500 | 1 | Surface Life Achieved (surface age as a percentage of Expected Life) |
| para_surf_remain_life | number | -100 | 100 | 1 | Surface Remaining Life, in years |
| para_flush_pct | number | 0 | 100 | 1 | flushing percentage |
| para_flush_info | text |  |  |  | Concatenated value for calibrated or reset values for aadi\|iv\| t100 |
| para_edgeb_pct | number | 0 | 100 | 1 | edge break percentage |
| para_edgeb_info | text |  |  |  | Concatenated value for calibrated or reset values for aadi\|iv\| t100 |
| para_scabb_pct | number | 0 | 100 | 1 | scabbing percentage |
| para_scabb_info | text |  |  |  | Concatenated value for calibrated or reset values for aadi\|iv\| t100 |
| para_lt_cracks_pct | number | 0 | 100 | 1 | lt_cracks percentage |
| para_lt_cracks_info | text |  |  |  | Concatenated value for calibrated or reset values for aadi\|iv\| t100 |
| para_mesh_cracks_pct | number | 0 | 100 | 1 | meash_cracks percentage |
| para_mesh_cracks_info | text |  |  |  | Concatenated value for calibrated or reset values for aadi\|iv\| t100 |
| para_shove_pct | number | 0 | 100 | 1 | shove percentage |
| para_shove_info | text |  |  |  | Concatenated value for calibrated or reset values for aadi\|iv\| t100 |
| para_poth_pct | number | 0 | 100 | 3 | poth percentage (allow more decimals) |
| para_poth_info | text |  |  |  | Concatenated value for calibrated or reset values for aadi\|iv\| t100 |
| para_rut_increm | number | 0 | 2.5 | 2 | Estimated Rut increment |
| para_rut | number | 0 | 50 | 1 | Rut depth |
| para_naasra_increm | number | 0 | 15 | 1 | Estimated Naasra Increment |
| para_naasra | number | 0 | 350 | 1 | Naasra |
| para_sdi | number | 0 | 100 | 2 | Surface distress index |
| para_pdi | number | 0 | 100 | 2 | Pavement distress index |
| para_obj_distress | number | 0 | 100 | 2 | Objective function component - weighted sum of PDI and SDI |
| para_obj_rsl | number | 0 | 100 | 2 | Objective function component - remaining surface life |
| para_obj_rutting | number | 0 | 100 | 2 | Objective function component - rutting |
| para_obj_naasra | number | 0 | 100 | 2 | Objective function component - roughness (NAASRA) |
| para_obj_o | number | 0 | 100 | 2 | Raw objective function |
| para_obj | number | 0 | 1000 | 2 | Objective function - raw objective function weighted by road type |
| para_obj_auc | number | 0 | 9999999 | 1 | Objective multiplied by treatment area, to scale for AUC calcs in BCA models |
| para_maint_cost_perkm | number | 0 | 99000000 | 0 | Maintenance Cost Tracker |
| para_csl_status | text |  |  |  | Canditate Selection Logic. Trigger logic outcome. If 'ok' then triggered = true. Otherwise triggered = false and value contains reason |
| para_csl_flag | number | 0 | 1 | 0 | Canditate Selection Logic. If 1 then triggered = true. If 0 then not triggered |
| para_is_treated_flag | number | 0 | 1 | 0 | Flag to indicate if the model has applied a treatment |
| para_treat_count | number | 0 | 999 | 0 | Number of Treatments applied |
| para_pdi_rank | number | 0 | 101 | 2 | Network Functions. Rank of raw Pavement Distress Index |
| para_rut_rank | number | 0 | 101 | 2 | Network Functions. Rank of Rut Depth |
| para_sdi_rank | number | 0 | 101 | 2 | Network Functions. Rank of raw Surface Distress Index |
| para_sla_rank | number | 0 | 101 | 2 | Network Functions. Rank of Surface Life Achieved |

> **Important**
>
> The table shown above must have left-upper corner in cell A1 of the 'input_headers' sheet. You can use the cells below or to the right of the table but it is recommended that you rather make use of a separate sheet to keep any notes etc.
>
> Keep column names unchanged (case-sensitive) and do not change the order of columns.

<a id="model-treatments"></a>

## Treatments

The treatments you want your model to trigger and apply need to be defined on the *'treatments'* sheet. An example of a set of defined treatments is shown in the table below. As shown, you need to specify the following properties for each treatment:

* **Name** (column 'treatment_name') - a short descriptive name for the treatment

* **Category** (column 'category') - name for the **Engineering* Category for a treatment (see note below.

* **Budget Category** (column 'budget_category') - Default **Budget** Category for the treatment (see note below)

* **Description** (column 'description') - Short optional description for the treatment

* **Unit Rate** (column 'unit_rate') - Unit rate to apply to the specified treatment quantity. Currency is up to you, and the quantity used will also be determined by your Domain Model logic. You should code your Domain Model to ensure congruency between your Unit Rate and the quantity for the treatment calculated at runtime.

* **Comments** (column 'comments') - Short optional comment on the treatment

> **Note**
>
> **Engineering Category vs Budget Category**
>
> You can apply two categories to a treatment. One is the engineering category, which should relate to the engineering purpose for the treatment. For example, the category 'Rehabilitation' clearly shows that the engineering intent for the treatment is to rehabilitate the element. You can only assign one engineering category to a treatment.
>
> By contrast, the Budget Category specifies which budget or budgets the costs for the treatment should be allocated to. It is the Budget Category that determines which budget constraint(s) the treatment will relate to. The budget category assigned to each treatment in the treatments template is the **default** budget category.
>
> Whereas you can only assign one engineering category to a treatment, your Domain Model can assign the costs for a treatment to more than one budget category. For example, if the treatment is a Rehabilitation, your model may assign a certain proportion (say, for resurfacing costs) to a 'Preservation' budget and another portion to a 'Repairs' budget category. In this case, your Domain Model will override the default budget category assigned in this table.
>
> Treatment costs split into different budget categories are output in the '**Spending**' output file. You can think of this table as holding *line items* related to a treatment. The total treatment costs (where costs assigned to different budgets are collapsed into a single item), are held in the '**Treatments**' output file.

**Example of Treatments Definition Template:**

| treatment_name | category | budget_category | description | unit_rate | comments |
|---|---|---|---|---|---|
| PreSeal | HeavyMaint | Pre-Repairs | Holding/Preseal Repairs as Holding action on Chipseal | 70 | none |
| ChipSeal_S | Preserve | Resurfacing | Chipseal surfacing with minimal repairs | 15 | none |
| ChipSeal_P | Preserve | Resurfacing | Chipseal surfacing with minimal repairs | 15 | none |
| ChipSeal_H | Holding | Resurfacing | Chipseal surfacing on Preseal Repairs | 15 | none |
| ThinAC_P | Preserve | Resurfacing | Thin AC overlay/inlay with no or minimal repairs | 65 | none |
| ThinAC_H | Holding | Resurfacing | Thin AC overlay/inlay with Heavy Maintenance repairs | 1 | none |
| HMaint_AC | HeavyMaint | Pre-Repairs | Heavy Maintenance on AC Section | 120 | Heavy maintenance/inlay/repair on AC |
| Rehab_CS_RL | Rehab_CS | Rehab | Rehab on Chipseal - Rural, Light Intensity | 120 | low volume and access |
| Rehab_CS_RM | Rehab_CS | Rehab | Rehab on Chipseal - Rural, Medium Intensity | 190 | secondary collectors |
| Rehab_CS_RH | Rehab_CS | Rehab | Rehab on Chipseal - Rural, Heavy Intensity | 260 | primary collectors and arterials |
| Rehab_CS_UL | Rehab_CS | Rehab | Rehab on Chipseal - Urban, Light Intensity | 150 | low volume and access |
| Rehab_CS_UM | Rehab_CS | Rehab | Rehab on Chipseal - Urban, Medium Intensity | 220 | secondary collectors |
| Rehab_CS_UH | Rehab_CS | Rehab | Rehab on Chipseal - Urban, Heavy Intensity | 290 | primary collectors and arterials |
| Rehab_AC_RL | Rehab_AC | Rehab | Rehab on AC - Rural, Light Intensity | 160 | low volume and access |
| Rehab_AC_RM | Rehab_AC | Rehab | Rehab on AC - Rural, Medium Intensity | 230 | secondary collectors |
| Rehab_AC_RH | Rehab_AC | Rehab | Rehab on AC - Rural, Heavy Intensity | 300 | primary collectors and arterials |
| Rehab_AC_UL | Rehab_AC | Rehab | Rehab on AC - Urban, Light Intensity | 190 | low volume and access |
| Rehab_AC_UM | Rehab_AC | Rehab | Rehab on AC - Urban, Medium Intensity | 260 | secondary collectors |
| Rehab_AC_UH | Rehab_AC | Rehab | Rehab on AC - Urban, Heavy Intensity | 330 | primary collectors and arterials |
| RMaint | Maintenance | Maintenance | Routine maintenance | 1 | Cost will be calculated at run-time based on distresses, hsd rough & rutting, traffic |
| BlockRep | Birthday | BlockRep | Block paving repairs treated when at end of expected life | 100 | set the file_surf_life_expected to a large value - 999 years - to disable treatment trigger |
| ConcRep | Birthday | ConcRep | Concrete paving repairs treated when at end of expected life | 100 | set the file_surf_life_expected to a large value - 999 years - to disable treatment trigger |
| Xtreat | Birthday | Xtreat | Other repairs treated when at end of expected life | 100 | set the file_surf_life_expected to a large value - 999 years - to disable treatment trigger |

> **Important**
>
> The table shown above must have left-upper corner in cell A1 of the 'treatments' sheet. You can use the cells below or to the right of the table but it is recommended that you rather make use of a separate sheet to keep any notes etc.
>
> Keep column names unchanged (case-sensitive) and do not change the order of columns.

## MCDA Setup

The parameters and weights you want to use for your Multi-Criteria Decision Analysis (MCDA) model needs to be specified on the **'mcda_setup'** sheet. The parameters and weights specified on this sheet define which parameters your MCDA model must use for treatment optimisation. 

There are three classes of parameters that you can apply in an MCDA setup:

* Parameters related to Treatment Suitability Scores and Treatment Cost. These two parameters are mandatory, and are indicated with source type 'mcda_ranking'.

* Parameters relating to model parameters (changing over time). You can add any model parameters that you want to take into account in the MCDA model here, by specifying source type 'model_param'.

* Parameters relating to input data (does not change over time). You can input parameters to the MCDA model by specifying them as having source type 'raw_data'.

In addition to specifying the source type (column 'source_type' - see below), you also need to specify which model parameter or input data column to use in the MCDA model. 

If the source type is 'model_param', then the value specified under the 'key' column should be the name of a model parameter (e.g. 'para_adt'). If the source type is 'raw_data', the value specified under the 'key' column should be the name of an input data column (e.g. 'file_length'). **Always presume key values are case sensitive** - that is, you should enter parameter names or input data column names exactly as they are specified in the setup (see above).

In addition to specifying the source for each parameter, you need to assign a relative weight to each parameter. These weights do not have to add up to 1. Cassandra will automatically adjust the weights automatically so that their relative values are retained and add up to 1. **It is perhaps best to think of these weights as 'importance scores' and then assign a value from 0 to 100**.

In the *'obj_type'* column, you need to specify the type of objective by using a 1 (if higher is better), or -1 (if lower is better). For example, the 'mcda_ranking' parameter 'treatment_cost' relates to the cost of treatment, which we want to minimise (lower is better). Thus, this value will have a 'obj_type' of -1.

Similarly, if you want to include a priority based on Traffic (ADT), you can have a value with source 'model_param' mapping to parameter 'para_adt' and having an objective type of 1, indicating that elements with higher traffic will get a higher priority in the optimisation scheme.

> **Note**
>
> The cost that is used in the MCDA optimisation will be automatically scaled using a scaling value in the input data, as specified by setting *'Cost Scaling Parameter'* in the Model Configuration - the `configurations.xlsx` workbook in your project's `inputs` folder, which carries one row per setting. Typically, the value used for scaling will be length or area for each element. The scaled cost is calculated as the cost divided by the value in the scaling parameter. Thus, if the raw cost is calculated as 100 and the scaling parameter is 'length', then for a length value of 50 the scaled cost will be 50.
> **If you do not want to apply cost scaling, then set the value for *'Cost Scaling Parameter'* in the Model Configuration to 'none'.**

**Example of MCDA Setup Template:**

| source_type | key | weight | obj_type |
|---|---|---|---|
| mcda_ranking | treatment_suitability_score | 10.38 | 1 |
| mcda_ranking | treatment_cost | 39.63 | -1 |
| model_param | para_adt | 16.29 | 1 |
| raw_data | file_no_of_bus_routes | 16.41 | 1 |
| model_param | para_surf_life_ach | 5.72 | 1 |
| model_param | para_treat_count | 29.3 | -1 |
| raw_data | file_length | 49.45 | 1 |

> **Important**
>
> The table shown above must have left-upper corner in cell A1 of the 'mcda_setup' sheet. You can use the cells below or to the right of the table but it is recommended that you rather make use of a separate sheet to keep any notes etc.
>
> Keep column names unchanged (case-sensitive) and do not change the order of columns.

## Network Functions

On the sheet **'network_functions'** you can define model parameters that are not calculated at element level by your Domain Model, but rather relative to the rest of the network. 

Most model parameters you define in the 'parameters' sheet will be calculated by your Domain Model and applies at element level. For example, a model parameter such as 'para_rut_increment' that models the Rut Increment for each element is strictly dependent on what happens on each element. That is, it is not dependent on what happens to other elements on the network.

Thus, for the rut increment example, the value of rut increment on an element will be dependent only on the characteristics of that element such as traffic, mesh-cracks, etc.

However, in some instances, you may want to know how the element compares relative to the rest of the network. An example may be the Percentage Rank of Rut depth. This cannot be calculated in the Domain Model because the domain model only has access to each element **one at a time**. 

The purpose of Network Functions is to make available some functionality for calculating aspects of an element as it relates to the rest of the network. To define a parameter that depends on Network Functions, you need to follow these broad steps:

1. Define the parameter in your 'parameters' sheet
2. Add a row to represent the parameter to the table on the 'network_functions' sheet
3. Define which other parameter the network-function parameter is dependent on (this is the 'input_parameter').
4. Define what network function should be used to calculate the value for your network-function parameter.

**An Example:** 
You want to add a parameter that reports the percentage rank of the element's rut depth relative to the rest of the network. You define the new parameter as *'para_rut_rank'*. The value assigned to *'para_rut_rank'* will depend on the value of *'para_rut'* (rut depth) for the element relative to the rut depth values on all other elements. Thus, your input parameter is *'para_rut'* and your network-function parameter is *'para_rut_rank'*.

Next you need to specify which of the available network functions is to be used for calculating the value of *'para_rut_rank'* from the values of *'para_rut'*. You choose *'percent_rank_approx'* meaning you will be using the approximate percentage rank calculation (see below).

> **Important**
>
> Because parameters calculated using Network Functions are always dependent on other parameters, **they should always be specified last in your Model Parameters table**. In the unlikely case that one Network Function parameter depends on another, you should specify the input parameter before the parameter that depends on it.

### Available Network Functions

The network functions currently (October 2025) available in Cassandra are:

* **Z-Score**: This is the [Standard Score](https://en.wikipedia.org/wiki/Standard_score) that indicates how many standard deviations the element's input value is below (negative) or above (positive) the mean of the total network distribution for that input value. Specify code **'z_score'** (case sensitive) in the 'function_type' column to use this function.

* **Percentage Rank**: this is the Percentage Rank of the input parameter relative to the values for the rest of the network, similar to [Excel's PERCENTRANK function](https://support.microsoft.com/en-au/office/percentrank-function-f1b5836c-9619-4847-9fc9-080ec9024442). Specify code **'percent_rank'** (case sensitive) in the 'function_type' column to use this function. This method uses a lazy lookup so it is SLOWER than
the approximate method below (by a factor of more than 100) but this method gives an exact match with Excel.

* **Approximate Percentage Rank**: this is the approximate Percentage Rank of the input parameter relative to the values for the rest of the network, similar to [Excel's PERCENTRANK function](https://support.microsoft.com/en-au/office/percentrank-function-f1b5836c-9619-4847-9fc9-080ec9024442). This method uses Binary Search so it is significantly
FASTER than the other method (see above) by a factor of about 100. However, when there are many repeat values it does not match Excel exactly. Specify code **'percent_rank_approx'** (case sensitive) in the 'function_type' column to use this function.

If you need additional Network Functions similar to the above for your model, contact Lonrix.

**Example of Network Functions Template:**

| input_parameter | function_type | output_parameter | comment |
|---|---|---|---|
| para_surf_life_ach | percent_rank_approx | para_sla_rank | Percentage rank for pan_surf_life_ach |
| para_rut | percent_rank_approx | para_rut_rank | Percentage rank for pan_rut |
| para_sdi | percent_rank_approx | para_sdi_rank | Percentage rank for pan_sdi |
| para_pdi | percent_rank_approx | para_pdi_rank | Percentage rank for pan_pdi |

> **Important**
>
> The table shown above must have left-upper corner in cell A1 of the 'network_functions' sheet. You can use the cells below or to the right of the table but it is recommended that you rather make use of a separate sheet to keep any notes etc.
>
> Keep column names unchanged (case-sensitive) and do not change the order of columns.
