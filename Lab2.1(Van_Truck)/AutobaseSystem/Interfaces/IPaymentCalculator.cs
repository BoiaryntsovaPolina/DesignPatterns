using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

public interface IPaymentCalculator
{
    decimal CalculatePayment(Trip trip);
}
