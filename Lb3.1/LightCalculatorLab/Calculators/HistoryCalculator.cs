using LightCalculatorLab.Interfaces;
using LightCalculatorLab.Models;

namespace LightCalculatorLab.Calculators;

// Шаблон DECORATOR: додає запис результатів в історію, сам не рахує
public sealed class HistoryCalculator : ICalculator
{
    private readonly ICalculator _inner;
    private readonly ICalculationHistory _history;

    public HistoryCalculator(ICalculator inner, ICalculationHistory history)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _history = history ?? throw new ArgumentNullException(nameof(history));
    }

    public double Add(double a, double b) => Record(a, '+', b, _inner.Add(a, b));

    public double Subtract(double a, double b) => Record(a, '-', b, _inner.Subtract(a, b));

    public double Multiply(double a, double b) => Record(a, '*', b, _inner.Multiply(a, b));

    public double Divide(double a, double b) => Record(a, '/', b, _inner.Divide(a, b));

    // Спочатку виконується _inner.*, і лише потім цей метод.
    // Якщо _inner кинув виняток, сюди не дійдемо: помилки в історію не потрапляють.
    private double Record(double a, char op, double b, double result)
    {
        if (double.IsFinite(result)) // Infinity і NaN не записуємо
        {
            _history.Add(new HistoryEntry(a, op, b, result));
        }

        return result;
    }
}
