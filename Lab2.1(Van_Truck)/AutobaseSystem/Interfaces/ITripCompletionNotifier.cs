using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

// ISP: окремо від ITripStartNotifier / IBreakdownNotifier 
public interface ITripCompletionNotifier
{
    void NotifyTripCompleted(Trip trip);
}
