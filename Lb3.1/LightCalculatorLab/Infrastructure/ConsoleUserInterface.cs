using LightCalculatorLab.Interfaces;

namespace LightCalculatorLab.Infrastructure;

public sealed class ConsoleUserInterface : IUserInterface
{
    public void ShowMessage(string message) => Console.WriteLine(message);

    public void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public string? ReadLine(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }
}
