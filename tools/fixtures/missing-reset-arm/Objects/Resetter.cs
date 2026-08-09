using System;
using JCass_ModelCore.Models;
using JCass_ModelCore.Treatments;

namespace FixtureModel.Objects;

/// <summary>
/// Stage 4b of 6: how an element changes when a treatment is applied to it.
///
/// <para>Called instead of <see cref="Incrementer"/>, for the elements the optimiser chose to
/// fund. Everything the forecast says about the benefit of spending money comes from this file,
/// which makes it the other half of the pair that decides whether a business case stacks up.</para>
///
/// <para><b>The <c>switch</c> is deliberately exhaustive with a throwing <c>default</c>.</b> A
/// treatment declared in the bundle, triggered by <see cref="TreatmentsTrigger"/>, and then not
/// handled here would be funded, charged to a budget, reported as delivered - and change nothing
/// about the element's condition. The forecast would show money spent for no benefit and nothing
/// would say why. Throwing turns that into a message on the first period it happens.
/// <c>jcass-dm check</c> looks for the same thing before you run.</para>
///
/// <para><b>Note the explicit empty case for routine maintenance.</b> It holds condition where it
/// is and does not reset age - it is a holding action, not a renewal. Writing that as its own
/// case rather than letting it fall through to the default records the intent.</para>
///
/// <para>Both reset values come from <c>lookups.xlsx</c>. "A repair halves the remaining defect"
/// and "a replacement leaves it at 10" are exactly the assumptions a modeller wants to test
/// against observed post-treatment surveys.</para>
/// </summary>
public class Resetter
{
    private readonly ModelBase _frameworkModel;
    private readonly FixtureModel _domainModel;

    /// <summary>
    /// Creates the resetter. Built once in <see cref="FixtureModel.SetupInstance"/>.
    /// </summary>
    /// <param name="frameworkModel">The framework model, for lookups and treatment types.</param>
    /// <param name="domainModel">This domain model, for its <see cref="Constants"/>.</param>
    public Resetter(ModelBase frameworkModel, FixtureModel domainModel)
    {
        _frameworkModel = frameworkModel ?? throw new ArgumentNullException(nameof(frameworkModel));
        _domainModel = domainModel ?? throw new ArgumentNullException(nameof(domainModel));
    }

    /// <summary>
    /// Applies the effect of a treatment to one element. Returns the same instance, mutated -
    /// the caller writes it straight back through the parameter sinks.
    /// </summary>
    /// <param name="element">The element that was treated.</param>
    /// <param name="period">Modelling period (1-based) the treatment was applied in.</param>
    /// <param name="treatment">The treatment the optimiser selected.</param>
    public ModelElement Reset(ModelElement element, int period, TreatmentInstance treatment)
    {
        Constants constants = _domainModel.Constants;

        switch (treatment.TreatmentName)
        {
            case TreatmentNames.Repair:
                // A repair improves the element by a factor; it does not make it new.
                element.ConditionRating *= constants.ConditionFactorAfterRepair;
                element.Age = 0;
                break;

            case TreatmentNames.RoutineMaintenance:
                // Holds condition where it is, and deliberately does not reset age.
                break;

            default:
                throw new Exception(
                    $"Treatment '{treatment.TreatmentName}' is not handled by Resetter.Reset(). " +
                    "Add a case for it here, or remove it from the treatments sheet of the bundle.");
        }

        element.SetObjectiveValue();
        return element;
    }
}
