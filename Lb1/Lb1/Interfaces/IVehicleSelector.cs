using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

// алгоритм підбору авто (за вантажопідйомністю) - окремо від логіки диспетчера.
public interface IVehicleSelector
{
    Vehicle? SelectVehicle(TransportRequest request, IReadOnlyList<Vehicle> availableVehicles);
}
