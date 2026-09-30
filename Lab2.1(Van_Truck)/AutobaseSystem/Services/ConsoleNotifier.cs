using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class ConsoleNotifier : ITripStartNotifier, ITripCompletionNotifier, IBreakdownNotifier
{
    public void NotifyTripStarted(Trip trip) =>
        Console.WriteLine($"[СТАРТ] {trip}");
    public void NotifyTripCompleted(Trip trip) =>
        Console.WriteLine($"[ЗАВЕРШЕНО] {trip} — виплата водію: {trip.Payment:0.00} грн");
    public void NotifyBreakdown(Trip trip) =>
        Console.WriteLine($"[ПОЛОМКА] {trip} — водій {trip.Driver.FullName} подав заявку на ремонт");
}
