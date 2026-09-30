using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class InMemoryDriverRepository : IDriverRepository
{
    private readonly List<Driver> _drivers;

    public InMemoryDriverRepository(IEnumerable<Driver> initialDrivers)
    {
        _drivers = initialDrivers.ToList();
    }


    public IReadOnlyList<Driver> GetAvailable() =>
        _drivers.Where(d => d.IsAvailable).ToList();


    public IReadOnlyList<Driver> GetAll() => _drivers.AsReadOnly();

    // Репозиторій не чіпає стан напряму
    public void SetAvailability(Driver driver, bool isAvailable)
    {
        if (isAvailable) driver.MarkAvailable();
        else driver.MarkBusy();
    }
}
