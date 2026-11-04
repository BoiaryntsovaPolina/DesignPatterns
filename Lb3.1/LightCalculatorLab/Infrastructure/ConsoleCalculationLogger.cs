using LightCalculatorLab.Interfaces;

namespace LightCalculatorLab.Infrastructure;

public sealed class ConsoleCalculationLogger : ICalculationLogger
{
    // За замовчуванням трасування увімкнено
    public bool IsEnabled { get; set; } = true;

    public void Log(string message)
    {
        if (!IsEnabled)
        {
            return;
        }

        Console.WriteLine($"   [trace] {message}");
    }
}
