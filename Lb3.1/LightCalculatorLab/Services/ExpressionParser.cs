using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using LightCalculatorLab.Interfaces;
using LightCalculatorLab.Models;

namespace LightCalculatorLab.Services;

// Розбирає рядок "операнд оператор операнд" (через пробіли).
// Операнд: число, ans (останній результат) або #N (результат запису N з історії).
public sealed class ExpressionParser : IExpressionParser
{
    private const string LastResultToken = "ans";
    private const char HistoryPrefix = '#';

    // Потрібна лише для читання результатів при розборі ans та #N
    private readonly ICalculationHistory _history;

    public ExpressionParser(ICalculationHistory history)
    {
        _history = history ?? throw new ArgumentNullException(nameof(history));
    }

    public bool TryParse(string input, [NotNullWhen(true)] out CalculationRequest? request, out string error)
    {
        request = null;
        error = string.Empty;

        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
        {
            error = "Невірний формат. Введіть: операнд оператор операнд (через пробіли), " +
                    "наприклад: 5 + 3 або ans * 2";
            return false;
        }

        if (!TryParseOperand(parts[0], out double left, out error))
        {
            return false;
        }

        if (parts[1].Length != 1)
        {
            error = $"'{parts[1]}' не є оператором.";
            return false;
        }

        if (!TryParseOperand(parts[2], out double right, out error))
        {
            return false;
        }

        request = new CalculationRequest(left, parts[1][0], right);
        return true;
    }

    private bool TryParseOperand(string text, out double value, out string error)
    {
        value = 0;
        error = string.Empty;

        // ans: результат останнього запису історії
        if (text.Equals(LastResultToken, StringComparison.OrdinalIgnoreCase))
        {
            if (_history.Entries.Count == 0)
            {
                error = "Немає попереднього результату: історія порожня.";
                return false;
            }

            value = _history.Entries[_history.Entries.Count - 1].Result;
            return true;
        }

        // #N: результат N-го запису (нумерація з 1, як у команді history)
        if (text.StartsWith(HistoryPrefix))
        {
            if (!int.TryParse(text.Substring(1), out int number))
            {
                error = $"'{text}' не є номером запису історії (очікується, наприклад, #1).";
                return false;
            }

            if (number < 1 || number > _history.Entries.Count)
            {
                error = $"В історії немає запису {text}.";
                return false;
            }

            value = _history.Entries[number - 1].Result;
            return true;
        }

        if (!TryParseNumber(text, out value))
        {
            error = $"'{text}' не є числом.";
            return false;
        }

        return true;
    }

    // Приймає і крапку, і кому; відхиляє Infinity та NaN
    private static bool TryParseNumber(string text, out double value)
    {
        string normalized = text.Replace(',', '.');

        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
               && double.IsFinite(value);
    }
}
