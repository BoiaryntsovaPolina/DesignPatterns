using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class RandomRequestGenerator : IRequestGenerator
{
    private static readonly string[] Destinations =
        { "Полтава", "Вінниця", "Львів", "Одеса", "Харків", "Дніпро", "Запоріжжя" };


    private readonly Random _random;
    private int _nextId = 1;

    public RandomRequestGenerator(Random random)
    {
        _random = random;
    }

    public TransportRequest? GenerateRequest()
    {
        var cargoType = (CargoType)_random.Next(0, Enum.GetValues<CargoType>().Length);

        return new TransportRequest(
            id: _nextId++,
            destination: Destinations[_random.Next(Destinations.Length)],
            cargoType: cargoType,
            cargoWeightKg: _random.Next(200, 12000),
            routeDistanceKm: _random.Next(50, 900),
            requiredDriverExperienceYears: GetRequiredExperience(cargoType),
            drivingComplexity: _random.Next(1, 11));
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