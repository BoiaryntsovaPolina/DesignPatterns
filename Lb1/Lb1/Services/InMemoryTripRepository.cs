using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class InMemoryTripRepository : ITripRepository
{
    private readonly List<Trip> _trips = new();

    public void Add(Trip trip) => _trips.Add(trip);

    public IReadOnlyList<Trip> GetAll() => _trips.AsReadOnly();

    // "Активний" = ще не Completed (тобто рейс, який зараз їде або зламався).
    public IReadOnlyList<Trip> GetActive() =>
        _trips.Where(t => t.Status is TripStatus.Assigned or TripStatus.InProgress or TripStatus.BrokenDown)
              .ToList();
}
