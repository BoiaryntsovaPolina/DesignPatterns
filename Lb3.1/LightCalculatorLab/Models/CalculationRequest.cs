namespace LightCalculatorLab.Models;

// Незмінний контейнер даних: ліве число, оператор, праве число.
// record сам створює конструктор, ToString і порівняння за значенням.
public sealed record CalculationRequest(double Left, char Operator, double Right);
