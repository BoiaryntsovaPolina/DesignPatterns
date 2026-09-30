using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class InMemoryDriverRepository : InMemoryRepository<Driver>, IDriverRepository
{
    public InMemoryDriverRepository(IEnumerable<Driver> initialDrivers)
    {
        foreach (var driver in initialDrivers)
            Add(driver);
    }

    public IReadOnlyList<Driver> GetAvailable() =>
        GetAll().Where(d => d.IsAvailable).ToList();
    // Репозиторій не чіпає стан напряму
    public void SetAvailability(Driver driver, bool isAvailable)
    {
        if (isAvailable) driver.MarkAvailable();
        else driver.MarkBusy();
    }
}
