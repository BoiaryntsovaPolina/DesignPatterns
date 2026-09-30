using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

public interface ITripStartNotifier
{
    void NotifyTripStarted(Trip trip);
}
