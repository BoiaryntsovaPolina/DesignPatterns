using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

// алгоритм підбору авто (за вантажопідйомністю)
public interface IVehicleSelector
{
    Vehicle? SelectVehicle(TransportRequest request, IReadOnlyList<Vehicle> availableVehicles);
}
