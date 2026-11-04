using LightCalculatorLab.Models;

namespace LightCalculatorLab.Interfaces;

public interface ICalculationService
{
    bool IsSupported(char op);
    string SupportedOperators { get; }
    double Calculate(CalculationRequest request);
}
