namespace ClanBattle;

public static class ConsoleHelper
{
    public static void Pause(string message = "  Enter — далі... ")
    {
        Console.Write(message);
        Console.ReadLine();
    }

    public static bool Confirm(string question)
    {
        Console.Write($"  {question} (y/n) > ");
        string? answer = Console.ReadLine()?.Trim().ToLowerInvariant();
        return answer is "y" or "yes" or "н" or "так";   // «н» — клавіша Y в українській розкладці
    }
}
