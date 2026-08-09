using System;
using System.Collections.Generic;

namespace FixtureModel.Objects;

/// <summary>
/// Builds <see cref="ModelElement"/> instances from the dictionaries the framework hands to the
/// domain model.
///
/// <para><b>Every input column name in this project appears here and nowhere else.</b> That is
/// the point of the file: when the client's CSV gains a column, or somebody renames one, there is
/// exactly one place to change and one place to look. Scatter <c>numInputs["..."]</c> through the
/// trigger and the incrementer and a renamed column becomes an afternoon.</para>
///
/// <para><b>The two methods differ in where evolving state comes from, and forgetting the second
/// one is the classic bug.</b> At period zero the framework has no parameter history, so state is
/// read straight out of the raw input columns. From period one onward it is read out of the model
/// parameters this domain model itself wrote last period. Attributes that never change - name,
/// material, area - are read from the input columns in both. A field added to
/// <see cref="GetFromInputData"/> and not to <see cref="GetFromModelData"/> gives a model that
/// behaves correctly in period 0 and wrongly from period 1, with no error anywhere.</para>
///
/// <para>The framework rejects nulls in numeric columns, so a client CSV with blanks has to carry
/// a sentinel value instead. Latitude and longitude use <c>-999</c> by convention.</para>
/// </summary>
public static class ModelElementFactory
{
    /// <summary>
    /// Builds an element from raw input data only. Used during initialisation, before any model
    /// parameters exist.
    /// </summary>
    /// <param name="iElemIndex">Zero-based index of the element.</param>
    /// <param name="numInputs">Numeric raw input columns, keyed by column name.</param>
    /// <param name="textInputs">Text raw input columns, keyed by column name.</param>
    public static ModelElement GetFromInputData(
        int iElemIndex,
        Dictionary<string, double> numInputs,
        Dictionary<string, string> textInputs)
    {
        ModelElement element = new ModelElement
        {
            ElementIndex = iElemIndex,
            ElementName = textInputs["element_name"],
            MaterialType = textInputs["material"],
            AreaSquareMetre = numInputs["area_sqm"],
            ConditionRating = numInputs["cond_rating"],
            Age = Convert.ToInt32(numInputs["age"]),
        };

        element.SetObjectiveValue();
        return element;
    }

    /// <summary>
    /// Builds an element from the previous period's model parameters, plus the raw inputs that do
    /// not change over time. Used for every period after initialisation.
    /// </summary>
    /// <param name="iElemIndex">Zero-based index of the element.</param>
    /// <param name="numInputs">Numeric raw input columns, keyed by column name.</param>
    /// <param name="textInputs">Text raw input columns, keyed by column name.</param>
    /// <param name="numModelData">Numeric model parameters as at the previous period.</param>
    /// <param name="textModelData">Text model parameters as at the previous period. Unused in this model.</param>
    public static ModelElement GetFromModelData(
        int iElemIndex,
        Dictionary<string, double> numInputs,
        Dictionary<string, string> textInputs,
        Dictionary<string, double> numModelData,
        Dictionary<string, string> textModelData)
    {
        ModelElement element = new ModelElement
        {
            ElementIndex = iElemIndex,

            // Unchanging attributes: from the inputs, every period.
            ElementName = textInputs["element_name"],
            MaterialType = textInputs["material"],
            AreaSquareMetre = numInputs["area_sqm"],

            // Evolving state: from the parameters this model wrote last period, NOT from the
            // inputs. Reading condition from numInputs here is how a model quietly stops
            // deteriorating - every period would start again from the original survey value.
            //
            // Rounded because the framework stores parameters as floats, and the accumulated
            // representation error is otherwise visible right at a trigger boundary.
            ConditionRating = Math.Round(numModelData["par_cond_rating"], 5),
            Age = (int)numModelData["par_age"],
        };

        element.SetObjectiveValue();
        return element;
    }
}
