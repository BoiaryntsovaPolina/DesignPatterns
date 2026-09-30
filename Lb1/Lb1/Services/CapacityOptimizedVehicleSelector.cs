using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class CapacityOptimizedVehicleSelector : IVehicleSelector
{
    /* "Оптимізація ваги": беремо лише авто, куди вантаж влізе (CapacityKg >= вага),
     і серед них - те, у якого залишок вантажопідйомності мінімальний */
    public Vehicle? SelectVehicle(TransportRequest request, IReadOnlyList<Vehicle> availableVehicles) =>
        availableVehicles
            .Where(v => v.CapacityKg >= request.CargoWeightKg)
            .OrderBy(v => v.CapacityKg - request.CargoWeightKg)
            .FirstOrDefault();
}
