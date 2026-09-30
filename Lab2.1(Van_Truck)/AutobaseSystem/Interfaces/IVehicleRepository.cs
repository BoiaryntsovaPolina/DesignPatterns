using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

public interface IVehicleRepository : IReadRepository<Vehicle>
{
    IReadOnlyList<Vehicle> GetAvailable();
    void SetAvailability(Vehicle vehicle, bool isAvailable);
    void UpdateCondition(Vehicle vehicle, VehicleCondition condition);
}
