using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

// Builder: заявка має 7 полів, тому збираємо її покроково замість довгого конструктора.
// Кожен крок With* повертає сам білдер - так виходить ланцюжок викликів (fluent): .WithA().WithB().Build().
public interface ITransportRequestBuilder
{
    ITransportRequestBuilder WithDestination(string destination);
    ITransportRequestBuilder WithCargo(CargoType cargoType, double weightKg);
    ITransportRequestBuilder WithRoute(double distanceKm, int drivingComplexity);
    ITransportRequestBuilder WithRequiredExperience(int years);
    TransportRequest Build();
}
