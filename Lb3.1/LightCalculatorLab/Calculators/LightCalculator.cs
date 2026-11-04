using LightCalculatorLab.Interfaces;

namespace LightCalculatorLab.Calculators;

// Шаблон PROXY: тривіальні випадки з нулем розв'язує сам,
// решту передає далі (у FullCalculator)
public sealed class LightCalculator : ICalculator
{
    private readonly ICalculator _inner;

    public LightCalculator(ICalculator inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public double Add(double a, double b)
    {
        if (IsZero(b)) return a;   // A + 0 = A
        if (IsZero(a)) return b;   // 0 + A = A

        return _inner.Add(a, b);   // складний випадок — делегуємо
    }

    public double Subtract(double a, double b)
    {
        if (IsZero(b)) return a;   // A - 0 = A
        if (IsZero(a)) return -b;  // 0 - A = -A

        return _inner.Subtract(a, b);
    }

    public double Multiply(double a, double b)
    {
        if (IsZero(a) || IsZero(b)) return 0;  // 0 * A = A * 0 = 0

        return _inner.Multiply(a, b);
    }

    public double Divide(double a, double b)
    {
        // Спочатку дільник, тому 0 / 0 теж дає виняток
        if (IsZero(b))
        {
            throw new DivideByZeroException("Ділення на нуль неможливе.");
        }

        if (IsZero(a)) return 0;   // 0 / B = 0

        return _inner.Divide(a, b);
    }

    // Єдине місце, де вирішується, що таке "нуль"
    private static bool IsZero(double value) => value == 0;
}
