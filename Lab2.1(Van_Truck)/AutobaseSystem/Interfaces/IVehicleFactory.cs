using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

// Контракт створення авто. Його реалізує абстрактний творець VehicleCreator (патерн Factory Method),
// а конкретний тип (Van / Truck) визначають підкласи - VanCreator і TruckCreator.
public interface IVehicleFactory
{
    Vehicle Create(string model, double capacityKg, int handlingComplexity);
}
