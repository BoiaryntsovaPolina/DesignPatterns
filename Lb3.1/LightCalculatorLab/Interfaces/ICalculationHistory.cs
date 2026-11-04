using LightCalculatorLab.Models;

namespace LightCalculatorLab.Interfaces;

// Декоратор пише в історію, а парсер і CalculatorApp тільки читають
public interface ICalculationHistory
{
    IReadOnlyList<HistoryEntry> Entries { get; }
    void Add(HistoryEntry entry);
    void Clear();
}
