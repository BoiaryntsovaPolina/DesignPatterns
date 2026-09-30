using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

public interface IDriverSelector
{
    Driver? SelectDriver(TransportRequest request, IReadOnlyList<Driver> availableDrivers);
}
