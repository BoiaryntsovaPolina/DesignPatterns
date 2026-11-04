namespace LightCalculatorLab.Interfaces;

// Спільний інтерфейс усіх калькуляторів (основа Proxy і Decorator)
public interface ICalculator
{
    double Add(double a, double b);
    double Subtract(double a, double b);
    double Multiply(double a, double b);
    double Divide(double a, double b);
}
