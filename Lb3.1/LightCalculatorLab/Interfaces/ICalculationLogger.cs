namespace LightCalculatorLab.Interfaces;

public interface ICalculationLogger
{
    // Вмикається і вимикається командою trace
    bool IsEnabled { get; set; }
    void Log(string message);
}
