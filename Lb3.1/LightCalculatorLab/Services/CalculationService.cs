using LightCalculatorLab.Interfaces;
using LightCalculatorLab.Models;

namespace LightCalculatorLab.Services;

// Клієнт шаблонів: за символом оператора вибирає метод калькулятора.
// Працює лише з ICalculator і не знає, що всередині ланцюжок обгорток.
public sealed class CalculationService : ICalculationService
{
    private readonly Dictionary<char, Func<double, double, double>> _operations;

    public CalculationService(ICalculator calculator)
    {
        if (calculator is null)
        {
            throw new ArgumentNullException(nameof(calculator));
        }

        // Нова операція додається одним рядком
        _operations = new Dictionary<char, Func<double, double, double>>
        {
            ['+'] = calculator.Add,
            ['-'] = calculator.Subtract,
            ['*'] = calculator.Multiply,
            ['/'] = calculator.Divide
        };
    }

    public string SupportedOperators => string.Join(" ", _operations.Keys);

    public bool IsSupported(char op) => _operations.ContainsKey(op);

    public double Calculate(CalculationRequest request)
    {
        if (!_operations.TryGetValue(request.Operator, out var operation))
        {
            throw new NotSupportedException($"Оператор '{request.Operator}' не підтримується.");
        }

        return operation(request.Left, request.Right);
    }
}
