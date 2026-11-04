using System.Diagnostics.CodeAnalysis;
using LightCalculatorLab.Models;

namespace LightCalculatorLab.Interfaces;

public interface IExpressionParser
{
    // [NotNullWhen(true)]: якщо метод повернув true, request гарантовано не null
    bool TryParse(string input, [NotNullWhen(true)] out CalculationRequest? request, out string error);
}
