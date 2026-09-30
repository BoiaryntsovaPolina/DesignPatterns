using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Factories;

// Factory Method, роль ConcreteCreator: створює вантажівки.
public class TruckCreator : VehicleCreator
{
    public TruckCreator(IIdGenerator idGenerator, IValidator<Vehicle> validator)
        : base(idGenerator, validator)
    {
    }

    protected override Vehicle CreateVehicle(int id, string model, double capacityKg, int handlingComplexity) =>
        new Truck(id, model, capacityKg, handlingComplexity);
}
