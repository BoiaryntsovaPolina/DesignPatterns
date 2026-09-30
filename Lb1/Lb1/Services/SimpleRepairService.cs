using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class SimpleRepairService : IRepairService
{
    private readonly IVehicleRepository _vehicleRepository;

    public SimpleRepairService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    // Ремонт міняє лише технічний стан авто, а не Trip.Status
    public void RequestRepair(Trip trip) =>
        _vehicleRepository.UpdateCondition(trip.Vehicle, VehicleCondition.UnderRepair);

    public void CompleteRepair(Trip trip) =>
        _vehicleRepository.UpdateCondition(trip.Vehicle, VehicleCondition.Operational);
}
