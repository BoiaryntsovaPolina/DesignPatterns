using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class RandomRequestGenerator : IRequestGenerator
{
    private static readonly string[] Destinations =
        { "Полтава", "Вінниця", "Львів", "Одеса", "Харків", "Дніпро", "Запоріжжя" };
    private readonly Random _random;
    private readonly ITransportRequestBuilder _requestBuilder;
    public RandomRequestGenerator(Random random, ITransportRequestBuilder requestBuilder)
    {
        _random = random;
        _requestBuilder = requestBuilder;
    }

    public TransportRequest? GenerateRequest()
    {
        var cargoType = (CargoType)_random.Next(0, Enum.GetValues<CargoType>().Length);
        
        return _requestBuilder
            .WithDestination(Destinations[_random.Next(Destinations.Length)])
            .WithCargo(cargoType, _random.Next(200, 12000))
            .WithRoute(_random.Next(50, 900), _random.Next(1, 11))
            .WithRequiredExperience(GetRequiredExperience(cargoType))
            .Build();
    }

    /* "тип вантажу -> мінімальний потрібний стаж". Небезпечний вантаж
     вимагає найбільше досвіду, звичайний - взагалі без вимог.*/
    private static int GetRequiredExperience(CargoType cargoType) => cargoType switch
    {
        CargoType.Hazardous => 5,
        CargoType.Heavy => 3,
        CargoType.Fragile => 2,
        CargoType.Perishable => 1,
        _ => 0
    };
}
