namespace LightCalculatorLab.Models;

// Один запис історії: що рахували і який вийшов результат
public sealed record HistoryEntry(double Left, char Operator, double Right, double Result)
{
    // Вигляд, у якому запис показується користувачу: "3 + 4 = 7"
    public override string ToString() => $"{Left} {Operator} {Right} = {Result}";
}
