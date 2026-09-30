using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class InMemoryVehicleRepository : InMemoryRepository<Vehicle>, IVehicleRepository
{
    public InMemoryVehicleRepository(IEnumerable<Vehicle> initialVehicles)
    {
        foreach (var vehicle in initialVehicles)
            Add(vehicle);
    }

    // "Доступне" авто - і вільне, і технічно справне одночасно
    public IReadOnlyList<Vehicle> GetAvailable() =>
        GetAll().Where(v => v.IsAvailable && v.Condition == VehicleCondition.Operational).ToList();
    public void SetAvailability(Vehicle vehicle, bool isAvailable)
    {
        if (isAvailable) vehicle.MarkAvailable();
        else vehicle.MarkBusy();
    }

    // Технічний стан - окремо від зайнятості
    public void UpdateCondition(Vehicle vehicle, VehicleCondition condition) =>
        vehicle.SetCondition(condition);
}
