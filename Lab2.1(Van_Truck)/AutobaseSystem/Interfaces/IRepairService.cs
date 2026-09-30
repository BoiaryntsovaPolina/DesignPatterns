using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

public interface IRepairService
{
    void RequestRepair(Trip trip);
    void CompleteRepair(Trip trip);
}
