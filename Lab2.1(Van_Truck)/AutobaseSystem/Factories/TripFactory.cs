using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Factories;

// Проста фабрика (Simple Factory) за інтерфейсом: єдине місце, де створюється Trip.
// Рейс один, ієрархії продуктів немає, тому GoF-ієрархія творців тут не потрібна.
public class TripFactory : ITripFactory
{
    private readonly IIdGenerator _idGenerator;
    private readonly IValidator<Trip> _validator;

    public TripFactory(IIdGenerator idGenerator, IValidator<Trip> validator)
    {
        _idGenerator = idGenerator;
        _validator = validator;
    }

    public Trip Create(TransportRequest request, Driver driver, Vehicle vehicle)
    {
        var trip = new Trip(_idGenerator.NextId<Trip>(), request, driver, vehicle);
        _validator.Validate(trip);
        return trip;
    }
}
