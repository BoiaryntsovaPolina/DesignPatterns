using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

/* ISP: окремий вузький інтерфейс лише для сповіщення про поломку -
клас, якому потрібне лише воно, не змушений реалізовувати старт/завершення.*/
public interface IBreakdownNotifier
{
    void NotifyBreakdown(Trip trip);
}
