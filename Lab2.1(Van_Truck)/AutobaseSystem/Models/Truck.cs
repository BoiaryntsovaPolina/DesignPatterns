namespace AutobaseSystem.Models;

// Factory Method, роль ConcreteProduct: вантажівка (важкий вантаж, понад 3,5 т).
public sealed class Truck : Vehicle
{
    internal Truck(int id, string model, double capacityKg, int handlingComplexity)
        : base(id, model, capacityKg, handlingComplexity)
    {
    }

    public override string Kind => "Вантажівка";
}
