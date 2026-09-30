using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

public interface ITripRepository
{
    void Add(Trip trip);
    IReadOnlyList<Trip> GetAll();
    IReadOnlyList<Trip> GetActive();
}
