using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Factories;

// Проста фабрика (Simple Factory) за інтерфейсом: єдине місце, де створюється Driver.
// водій один, ієрархії продуктів немає
// Factory Method (GoF) - VehicleCreator (Van / Truck).
public class DriverFactory : IDriverFactory
{
    private readonly IIdGenerator _idGenerator;
    private readonly IValidator<Driver> _validator;

    public DriverFactory(IIdGenerator idGenerator, IValidator<Driver> validator)
    {
        _idGenerator = idGenerator;
        _validator = validator;
    }

    public Driver Create(string fullName, int experienceYears)
    {
        var driver = new Driver(_idGenerator.NextId<Driver>(), fullName, experienceYears);
        _validator.Validate(driver);
        return driver;
    }
}
