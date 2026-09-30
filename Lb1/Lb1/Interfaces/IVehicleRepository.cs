using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

public interface IVehicleRepository
{
    IReadOnlyList<Vehicle> GetAvailable();
    IReadOnlyList<Vehicle> GetAll();
    void SetAvailability(Vehicle vehicle, bool isAvailable);

    void UpdateCondition(Vehicle vehicle, VehicleCondition condition);
}
