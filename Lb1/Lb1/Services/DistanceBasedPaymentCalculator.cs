using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class DistanceBasedPaymentCalculator : IPaymentCalculator
{
    private const decimal RatePerKm = 15m;
    private const decimal RatePerKgOfCargo = 0.5m;
    private const decimal ComplexityBonusPerPoint = 20m;


    public decimal CalculatePayment(Trip trip) =>
        (decimal)trip.Request.RouteDistanceKm * RatePerKm +
        (decimal)trip.Request.CargoWeightKg * RatePerKgOfCargo +
        trip.Request.DrivingComplexity * ComplexityBonusPerPoint;
}
