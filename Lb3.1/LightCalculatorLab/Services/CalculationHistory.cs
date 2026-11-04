using LightCalculatorLab.Interfaces;
using LightCalculatorLab.Models;

namespace LightCalculatorLab.Services;

// Історія в пам'яті (зникає після закриття програми)
public sealed class CalculationHistory : ICalculationHistory
{
    private readonly List<HistoryEntry> _entries = new();

    // Лише для читання, щоб ззовні список не можна було змінити
    public IReadOnlyList<HistoryEntry> Entries => _entries.AsReadOnly();

    public void Add(HistoryEntry entry) => _entries.Add(entry);

    public void Clear() => _entries.Clear();
}
