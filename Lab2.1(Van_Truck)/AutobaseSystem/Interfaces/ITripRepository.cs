using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

public interface ITripRepository : IRepository<Trip>
{
    IReadOnlyList<Trip> GetActive();
}
