using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class TransportRequestValidator : EntityValidator<TransportRequest>
{
    public const int MaxDestinationLength = 100;
    public const double MaxCargoWeightKg = 100_000;
    public const double MaxRouteDistanceKm = 20_000;
    protected override void ValidateFields(TransportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Destination))
            throw new ArgumentException("Пункт призначення обов'язковий.", nameof(request.Destination));
        if (request.Destination.Length > MaxDestinationLength)
            throw new ArgumentException($"Пункт призначення не довший {MaxDestinationLength} символів.", nameof(request.Destination));

        if (!Enum.IsDefined(request.CargoType))
            throw new ArgumentOutOfRangeException(nameof(request.CargoType), "Невідомий тип вантажу.");
 
        if (!double.IsFinite(request.CargoWeightKg) || request.CargoWeightKg is <= 0 or > MaxCargoWeightKg)
            throw new ArgumentOutOfRangeException(nameof(request.CargoWeightKg), $"Вага > 0 і не більше {MaxCargoWeightKg} кг.");
        if (!double.IsFinite(request.RouteDistanceKm) || request.RouteDistanceKm is <= 0 or > MaxRouteDistanceKm)
            throw new ArgumentOutOfRangeException(nameof(request.RouteDistanceKm), $"Дистанція > 0 і не більше {MaxRouteDistanceKm} км.");
        if (request.RequiredDriverExperienceYears is < 0 or > DriverValidator.MaxExperienceYears)
            throw new ArgumentOutOfRangeException(nameof(request.RequiredDriverExperienceYears),
                $"Стаж 0..{DriverValidator.MaxExperienceYears}.");
        if (request.DrivingComplexity is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(request.DrivingComplexity), "Складність 1..10.");
    }
}
