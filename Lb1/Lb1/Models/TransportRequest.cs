namespace AutobaseSystem.Models;


public record TransportRequest
{
    public int Id { get; private init; }
    public string Destination { get; private init; }
    public CargoType CargoType { get; private init; }
    public double CargoWeightKg { get; private init; }
    public double RouteDistanceKm { get; private init; }

    // Мінімальний стаж водія, потрібний саме для цього вантажу
    public int RequiredDriverExperienceYears { get; private init; }

    // 1..10: узагальнена оцінка складності маршруту (дорога, вантаж, дистанція тощо).
    public int DrivingComplexity { get; private init; }
    public DateTime CreatedAt { get; private init; } = DateTime.Now;

    public TransportRequest(
        int id,
        string destination,
        CargoType cargoType,
        double cargoWeightKg,
        double routeDistanceKm,
        int requiredDriverExperienceYears,
        int drivingComplexity)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "ID має бути > 0.");
        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("Пункт призначення обов'язковий.", nameof(destination));
        if (cargoWeightKg <= 0)
            throw new ArgumentOutOfRangeException(nameof(cargoWeightKg), "Вага > 0.");
        if (routeDistanceKm <= 0)
            throw new ArgumentOutOfRangeException(nameof(routeDistanceKm), "Дистанція > 0.");
        if (requiredDriverExperienceYears < 0)
            throw new ArgumentOutOfRangeException(nameof(requiredDriverExperienceYears), "Стаж >= 0.");
        if (drivingComplexity is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(drivingComplexity), "Складність 1..10.");

        Id = id;
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
