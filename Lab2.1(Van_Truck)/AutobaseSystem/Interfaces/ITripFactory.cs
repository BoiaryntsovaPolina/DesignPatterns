using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

public interface ITripFactory
{
    Trip Create(TransportRequest request, Driver driver, Vehicle vehicle);
}
