using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class DriverValidator : EntityValidator<Driver>
{
    public const int MaxFullNameLength = 100;
    public const int MaxExperienceYears = 60;
    protected override void ValidateFields(Driver driver)
    {
        if (string.IsNullOrWhiteSpace(driver.FullName))
            throw new ArgumentException("Ім'я обов'язкове.", nameof(driver.FullName));
        if (driver.FullName.Length > MaxFullNameLength)
            throw new ArgumentException($"Ім'я не довше {MaxFullNameLength} символів.", nameof(driver.FullName));
        if (driver.ExperienceYears is < 0 or > MaxExperienceYears)
            throw new ArgumentOutOfRangeException(nameof(driver.ExperienceYears), $"Стаж 0..{MaxExperienceYears}.");
    }
}
