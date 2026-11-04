using LightCalculatorLab.Interfaces;

namespace LightCalculatorLab.Calculators;

// Шаблон DECORATOR: повідомляє, що виклик пройшов через цю ланку.
// label дозволяє використати один клас у різних місцях ланцюжка.
public sealed class TracingCalculator : ICalculator
{
    private readonly ICalculator _inner;
    private readonly string _label;
    private readonly ICalculationLogger _logger;

    public TracingCalculator(ICalculator inner, string label, ICalculationLogger logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _label = label ?? throw new ArgumentNullException(nameof(label));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public double Add(double a, double b)
    {
        _logger.Log($"{_label}: {a} + {b}");
        return _inner.Add(a, b);
    }

    public double Subtract(double a, double b)
    {
        _logger.Log($"{_label}: {a} - {b}");
        return _inner.Subtract(a, b);
    }

    public double Multiply(double a, double b)
    {
        _logger.Log($"{_label}: {a} * {b}");
        return _inner.Multiply(a, b);
    }

    public double Divide(double a, double b)
    {
        _logger.Log($"{_label}: {a} / {b}");
        return _inner.Divide(a, b);
    }
}
