using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

// Спільні правила для будь-якого авто
public class VehicleValidator : EntityValidator<Vehicle>
{
    public const int MaxModelLength = 100;
    public const double MaxCapacityKg = 100_000;
    protected override void ValidateFields(Vehicle vehicle)
    {
        if (string.IsNullOrWhiteSpace(vehicle.Model))
            throw new ArgumentException("Модель обов'язкова.", nameof(vehicle.Model));
        if (vehicle.Model.Length > MaxModelLength)
            throw new ArgumentException($"Модель не довша {MaxModelLength} символів.", nameof(vehicle.Model));

        if (!double.IsFinite(vehicle.CapacityKg) || vehicle.CapacityKg is <= 0 or > MaxCapacityKg)
            throw new ArgumentOutOfRangeException(nameof(vehicle.CapacityKg), $"Вантажопідйомність > 0 і не більше {MaxCapacityKg} кг.");
        if (vehicle.HandlingComplexity is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(vehicle.HandlingComplexity), "Складність 1..10.");
    }
}
