using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

// алгоритм підбору водія винесено за інтерфейс (OCP)
public interface IDriverSelector
{
    Driver? SelectDriver(TransportRequest request, IReadOnlyList<Driver> availableDrivers);
}
