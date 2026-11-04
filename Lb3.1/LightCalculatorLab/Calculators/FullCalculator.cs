using LightCalculatorLab.Interfaces;

namespace LightCalculatorLab.Calculators;

// Справжній виконавець (RealSubject): загальна арифметика без особливих правил
public sealed class FullCalculator : ICalculator
{
    public double Add(double a, double b) => a + b;

    public double Subtract(double a, double b) => a - b;

    public double Multiply(double a, double b) => a * b;

    public double Divide(double a, double b)
    {
        // Для double ділення на 0 не кидає виняток (дає Infinity), тому перевіряємо явно
        if (b == 0)
        {
            throw new DivideByZeroException("Ділення на нуль неможливе.");
        }

        return a / b;
    }
}
