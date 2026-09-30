namespace AutobaseSystem.Models;

// Factory Method, роль ConcreteProduct: мікроавтобус (легкий вантаж, до 3,5 т).
public sealed class Van : Vehicle
{
    internal Van(int id, string model, double capacityKg, int handlingComplexity)
        : base(id, model, capacityKg, handlingComplexity)
    {
    }

    public override string Kind => "Мікроавтобус";
}
