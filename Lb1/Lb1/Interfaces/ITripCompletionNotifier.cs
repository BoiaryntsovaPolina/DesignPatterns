using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

// ISP: окремо від ITripStartNotifier / IBreakdownNotifier - див. ConsoleNotifier.
public interface ITripCompletionNotifier
{
    void NotifyTripCompleted(Trip trip);
}
