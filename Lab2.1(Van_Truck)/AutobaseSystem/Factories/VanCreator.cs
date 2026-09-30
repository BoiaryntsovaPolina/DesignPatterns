using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Factories;

// Factory Method, роль ConcreteCreator: створює мікроавтобуси.
public class VanCreator : VehicleCreator
{
    public VanCreator(IIdGenerator idGenerator, IValidator<Vehicle> validator)
        : base(idGenerator, validator)
    {
    }

    protected override Vehicle CreateVehicle(int id, string model, double capacityKg, int handlingComplexity) =>
        new Van(id, model, capacityKg, handlingComplexity);
}
