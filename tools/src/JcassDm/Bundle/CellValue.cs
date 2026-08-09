using System;
using System.Globalization;

namespace JcassDm.Bundle;

/// <summary>
/// Turning a cell into text, and text into a cell value, deterministically.
///
/// <para>Determinism is the point. <c>dump</c> is only useful if two dumps of the same
/// workbook are byte-identical on two different machines, otherwise every diff is full of
/// noise and nobody trusts it. Everything here formats through
/// <see cref="CultureInfo.InvariantCulture"/> for that reason - see also the culture pin in
/// <see cref="Program"/>, which covers the formatting ClosedXML does on our behalf.</para>
/// </summary>
internal static class CellValue
{
    /// <summary>
    /// Renders a cell value for display and for comparison. Blank renders as an empty
    /// string; a number renders shortest-round-trip so 19.1 does not come back as
    /// 19.100000000000001.
    /// </summary>
    public static string Render(object? value)
    {
        return value switch
        {
            null => string.Empty,
            string s => s,
            double d => d.ToString("R", CultureInfo.InvariantCulture),
            bool b => b ? "TRUE" : "FALSE",
            DateTime dt => dt.TimeOfDay == TimeSpan.Zero
                ? dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };
    }

    /// <summary>
    /// Decides what to write into a cell for a value the caller supplied as text.
    ///
    /// <para>A value that looks like a number is written as a number, so the cell matches
    /// what a person typing into Excel would have produced, and so the framework's
    /// <c>Convert.ToDouble</c> reads it back without depending on the machine's locale.
    /// Everything else is written as text.</para>
    /// </summary>
    public static object ForCell(string text, bool numericIfPossible)
    {
        if (!numericIfPossible) return text;

        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
        {
            return number;
        }
        return text;
    }

    /// <summary>
    /// Whether an existing cell already holds the requested value. Numbers compare
    /// numerically so that 0 does not "differ from" 0.0, and everything else compares as
    /// rendered text, ordinally - because a treatment name that differs only in case is a
    /// different treatment as far as the framework's dictionaries are concerned.
    /// </summary>
    public static bool Matches(object? current, object requested)
    {
        if (requested is double requestedNumber)
        {
            if (current is double currentNumber) return currentNumber.Equals(requestedNumber);
            if (current is null) return false;

            return double.TryParse(Render(current), NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed)
                && parsed.Equals(requestedNumber);
        }

        return string.Equals(Render(current), Render(requested), StringComparison.Ordinal);
    }
}
