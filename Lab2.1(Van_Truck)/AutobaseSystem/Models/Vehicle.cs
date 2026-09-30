namespace AutobaseSystem.Models;

// Factory Method, роль Product: спільна база всіх авто. Конкретні продукти - Van і Truck.
public abstract class Vehicle : Entity
{
    public string Model { get; private init; }
    public double CapacityKg { get; private init; }

    // Наскільки складно керувати саме цим авто (1..10)
    public int HandlingComplexity { get; private init; }
    public bool IsAvailable { get; private set; } = true;
    public VehicleCondition Condition { get; private set; } = VehicleCondition.Operational;

    public abstract string Kind { get; }

  
    protected Vehicle(int id, string model, double capacityKg, int handlingComplexity) : base(id)
    {
        Model = model;
        CapacityKg = capacityKg;
        HandlingComplexity = handlingComplexity;
    }


    internal void MarkBusy() => IsAvailable = false;
    internal void MarkAvailable() => IsAvailable = true;


    internal void SetCondition(VehicleCondition condition) => Condition = condition;

    public override string ToString() =>
        $"{Kind} {Model} (вант. {CapacityKg} кг, скл. {HandlingComplexity}, " +
        $"{(IsAvailable ? "вільне" : "зайняте")}, стан: {Condition})";
}
