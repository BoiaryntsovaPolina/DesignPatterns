namespace AutobaseSystem.Models;

public class TransportRequest : Entity
{
    public string Destination { get; private init; }
    public CargoType CargoType { get; private init; }
    public double CargoWeightKg { get; private init; }
    public double RouteDistanceKm { get; private init; }

    // Мінімальний стаж водія, потрібний саме для цього вантажу
    public int RequiredDriverExperienceYears { get; private init; }

    // 1..10: узагальнена оцінка складності маршруту (дорога, вантаж, дистанція тощо).
    public int DrivingComplexity { get; private init; }
    public DateTime CreatedAt { get; private init; } = DateTime.Now;


    internal TransportRequest(
        int id,
        string destination,
        CargoType cargoType,
        double cargoWeightKg,
        double routeDistanceKm,
        int requiredDriverExperienceYears,
        int drivingComplexity) : base(id)
    {
        Destination = destination;
        CargoType = cargoType;
        CargoWeightKg = cargoWeightKg;
        RouteDistanceKm = routeDistanceKm;
        RequiredDriverExperienceYears = requiredDriverExperienceYears;
        DrivingComplexity = drivingComplexity;
    }

    public override string ToString() =>
        $"Заявка #{Id}: {Destination}, вантаж {CargoType} ({CargoWeightKg} кг), " +
        $"дист. {RouteDistanceKm} км, стаж від {RequiredDriverExperienceYears} р.";
}
