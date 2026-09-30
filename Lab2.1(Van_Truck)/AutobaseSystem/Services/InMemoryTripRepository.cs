using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class InMemoryTripRepository : InMemoryRepository<Trip>, ITripRepository
{
    public IReadOnlyList<Trip> GetActive() =>
        GetAll().Where(t => t.Status is TripStatus.Assigned or TripStatus.InProgress or TripStatus.BrokenDown)
                .ToList();
}
