using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

// Правила саме для мікроавтобусів
public class VanValidator : VehicleValidator
{
    // Легкий комерційний транспорт: до 3,5 т вантажу. Межу використовує і TruckValidator.
    public const double MaxVanCapacityKg = 3_500;

    protected override void ValidateFields(Vehicle vehicle)
    {
        base.ValidateFields(vehicle);
        if (vehicle.CapacityKg > MaxVanCapacityKg)
            throw new ArgumentOutOfRangeException(nameof(vehicle.CapacityKg),
                $"Мікроавтобус: вантажопідйомність не більше {MaxVanCapacityKg} кг (важче - це вантажівка).");
    }
}
