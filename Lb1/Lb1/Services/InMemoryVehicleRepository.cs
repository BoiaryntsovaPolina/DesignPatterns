using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class InMemoryVehicleRepository : IVehicleRepository
{
    private readonly List<Vehicle> _vehicles;

    public InMemoryVehicleRepository(IEnumerable<Vehicle> initialVehicles)
    {
        _vehicles = initialVehicles.ToList();
    }

    // "Доступне" авто - і вільне, і технічно справне одночасно
    public IReadOnlyList<Vehicle> GetAvailable() =>
        _vehicles.Where(v => v.IsAvailable && v.Condition == VehicleCondition.Operational).ToList();

    public IReadOnlyList<Vehicle> GetAll() => _vehicles.AsReadOnly();

    public void SetAvailability(Vehicle vehicle, bool isAvailable)
    {
        if (isAvailable) vehicle.MarkAvailable();
        else vehicle.MarkBusy();
    }

    // Технічний стан - окремо від зайнятості
    public void UpdateCondition(Vehicle vehicle, VehicleCondition condition) =>
        vehicle.SetCondition(condition);
}
