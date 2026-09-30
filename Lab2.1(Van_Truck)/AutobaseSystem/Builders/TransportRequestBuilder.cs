using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Builders;

// Builder: заявку збираємо покроково, а Build() бере ID у генератора, створює об'єкт і перевіряє його валідатором.
// Якщо якийсь крок пропущено (наприклад, не вказано пункт призначення) - валідатор відхилить заявку.
public class TransportRequestBuilder : ITransportRequestBuilder
{
    private readonly IIdGenerator _idGenerator;
    private readonly IValidator<TransportRequest> _validator;
    private string _destination = string.Empty;
    private CargoType _cargoType = CargoType.General;
    private double _cargoWeightKg;
    private double _routeDistanceKm;
    private int _requiredDriverExperienceYears;
    private int _drivingComplexity;
    public TransportRequestBuilder(IIdGenerator idGenerator, IValidator<TransportRequest> validator)
    {
        _idGenerator = idGenerator;
        _validator = validator;
    }

    // Кроки лише запам'ятовують значення в полях; "return this" повертає той самий білдер для наступного виклику в ланцюжку.
    public ITransportRequestBuilder WithDestination(string destination)
    {
        _destination = destination;
        return this;
    }

    public ITransportRequestBuilder WithCargo(CargoType cargoType, double weightKg)
    {
        _cargoType = cargoType;
        _cargoWeightKg = weightKg;
        return this;
    }

    public ITransportRequestBuilder WithRoute(double distanceKm, int drivingComplexity)
    {
        _routeDistanceKm = distanceKm;
        _drivingComplexity = drivingComplexity;
        return this;
    }

    public ITransportRequestBuilder WithRequiredExperience(int years)
    {
        _requiredDriverExperienceYears = years;
        return this;
    }

    // Фінальний крок: Id -> створення -> перевірка.
    // try/finally: Reset() виконається завжди - і при успіху, і якщо валідатор кинув виняток.
    public TransportRequest Build()
    {
        try
        {
            var request = new TransportRequest(
                _idGenerator.NextId<TransportRequest>(),
                _destination,
                _cargoType,
                _cargoWeightKg,
                _routeDistanceKm,
                _requiredDriverExperienceYears,
                _drivingComplexity);
            _validator.Validate(request);
            return request;
        }
        finally
        {
            Reset(); // билдер можна використовувати повторно, дані попередньої заявки не "просочуються" в наступну
        }
    }

    private void Reset()
    {
        _destination = string.Empty;
        _cargoType = CargoType.General;
        _cargoWeightKg = 0;
        _routeDistanceKm = 0;
        _requiredDriverExperienceYears = 0;
        _drivingComplexity = 0;
    }
}
