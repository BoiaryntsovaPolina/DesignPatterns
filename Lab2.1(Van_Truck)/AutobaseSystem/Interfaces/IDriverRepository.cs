using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

public interface IDriverRepository : IReadRepository<Driver>
{
    IReadOnlyList<Driver> GetAvailable();
    void SetAvailability(Driver driver, bool isAvailable);
}
