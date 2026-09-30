using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Factories;

// FACTORY METHOD (GoF), роль Creator.
// Творець описує ЗАГАЛЬНИЙ процес створення авто (Id -> об'єкт -> перевірка), а те, ЯКИЙ САМЕ клас авто
// створити, залишає підкласам через абстрактний фабричний метод CreateVehicle.
//
//   Creator         -> VehicleCreator (цей клас)
//   ConcreteCreator -> VanCreator, TruckCreator
//   Product         -> Vehicle
//   ConcreteProduct -> Van, Truck
//
// Program працює з VehicleCreator / IVehicleFactory і не знає, який продукт зроблять підкласи.
// Щоб додати новий тип авто (наприклад, рефрижератор), достатньо нового продукту і нового творця:
// цей клас, диспетчер та інші наявні класи не змінюються (принцип відкритості/закритості, OCP).
public abstract class VehicleCreator : IVehicleFactory
{
    private readonly IIdGenerator _idGenerator;
    private readonly IValidator<Vehicle> _validator;

    protected VehicleCreator(IIdGenerator idGenerator, IValidator<Vehicle> validator)
    {
        _idGenerator = idGenerator;
        _validator = validator;
    }

    public Vehicle Create(string model, double capacityKg, int handlingComplexity)
    {
        // Id беремо для типу Vehicle (а не Van/Truck): нумерація спільна для всіх авто.
        var id = _idGenerator.NextId<Vehicle>();

        //ФАБРИЧНИЙ МЕТОД: підклас вирішує, який саме клас створити.
        var vehicle = CreateVehicle(id, model, capacityKg, handlingComplexity);

        _validator.Validate(vehicle);
        return vehicle;
    }

    // Сам фабричний метод. protected: його викликає лише творець у Create, а клієнт - ні.
    protected abstract Vehicle CreateVehicle(int id, string model, double capacityKg, int handlingComplexity);
}
