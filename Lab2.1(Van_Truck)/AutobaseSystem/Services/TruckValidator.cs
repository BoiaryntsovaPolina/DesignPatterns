using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

// Правила саме для вантажівок
public class TruckValidator : VehicleValidator
{
    protected override void ValidateFields(Vehicle vehicle)
    {
        base.ValidateFields(vehicle); // спершу спільні перевірки
        if (vehicle.CapacityKg <= VanValidator.MaxVanCapacityKg)
            throw new ArgumentOutOfRangeException(nameof(vehicle.CapacityKg),
                $"Вантажівка: вантажопідйомність більша за {VanValidator.MaxVanCapacityKg} кг (легше - це мікроавтобус).");
    }
}
