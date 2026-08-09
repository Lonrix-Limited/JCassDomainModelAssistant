using System;
using System.Collections.Generic;

namespace DomainModelSample.Objects;

/// <summary>
/// Builds <see cref="SampleElement"/> instances from the dictionaries the framework hands to the
/// domain model. Every entry point on <see cref="DomainModelSample"/> starts by calling one of
/// these two methods, which is why this is the file to edit when you add an input column or a
/// model parameter — the mapping lives in exactly one place.
///
/// <para>The two methods differ in where element state comes from. At period zero the framework
/// has no parameter history, so state is read straight out of the raw input columns. From period
/// one onward, state is read out of the model parameters the domain model itself wrote on the
/// previous period. Raw inputs that never change (name, material, area) are read from the input
/// columns in both cases.</para>
/// </summary>
public static class ElementFactory
{
    /// <summary>
    /// Builds an element from raw input data only. Used during initialisation, before any model
    /// parameters exist.
    /// </summary>
    /// <param name="iElemIndex">Zero-based index of the element.</param>
    /// <param name="numInputs">Numeric raw input columns, keyed by column name.</param>
    /// <param name="textInputs">Text raw input columns, keyed by column name.</param>
    public static SampleElement GetElementFromInputData(
        int iElemIndex,
        Dictionary<string, double> numInputs,
        Dictionary<string, string> textInputs)
    {
        SampleElement element = new SampleElement
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
    public static SampleElement GetElementFromModelData(
        int iElemIndex,
        Dictionary<string, double> numInputs,
        Dictionary<string, string> textInputs,
        Dictionary<string, double> numModelData,
        Dictionary<string, string> textModelData)
    {
        SampleElement element = new SampleElement
        {
            ElementIndex = iElemIndex,
            ElementName = textInputs["element_name"],
            MaterialType = textInputs["material"],
            AreaSquareMetre = numInputs["area_sqm"],

            // Rounded because the framework stores parameters as floats and the accumulated
            // representation error is otherwise visible in trigger comparisons at the boundary.
            ConditionRating = Math.Round(numModelData["par_cond_rating"], 5),
            Age = (int)numModelData["par_age"],
        };

        element.SetObjectiveValue();
        return element;
    }
}
