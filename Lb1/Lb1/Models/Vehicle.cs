namespace AutobaseSystem.Models;


public class Vehicle
{
    public int Id { get; private init; }
    public string Model { get; private init; }
    public double CapacityKg { get; private init; }

    // Наскільки складно керувати саме цим авто (враховується непрямо через DrivingComplexity заявки).
    public int HandlingComplexity { get; private init; }
    public bool IsAvailable { get; private set; } = true;
    public VehicleCondition Condition { get; private set; } = VehicleCondition.Operational;

    public Vehicle(int id, string model, double capacityKg, int handlingComplexity)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Модель обов'язкова.", nameof(model));
        if (capacityKg <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacityKg), "Вантажопідйомність > 0.");
        if (handlingComplexity is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(handlingComplexity), "Складність 1..10.");

        Id = id;
        Model = model;
        CapacityKg = capacityKg;
        HandlingComplexity = handlingComplexity;
    }

    public void MarkBusy() => IsAvailable = false;

    public void MarkAvailable() => IsAvailable = true;

    // Викликається сервісом ремонту
    public void SetCondition(VehicleCondition condition) => Condition = condition;

    public bool Equals(Vehicle? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as Vehicle);

    public override int GetHashCode() => Id.GetHashCode();


    public override string ToString() =>
        $"Авто {Model} (вант. {CapacityKg} кг, скл. {HandlingComplexity}, " +
        $"{(IsAvailable ? "вільне" : "зайняте")}, стан: {Condition})";
}