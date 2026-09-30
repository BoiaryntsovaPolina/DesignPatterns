using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class TripValidator : EntityValidator<Trip>
{
    protected override void ValidateFields(Trip trip)
    {
        // не можна створити рейс без запиту/водія/авто.
        if (trip.Request is null)
            throw new ArgumentNullException(nameof(trip.Request));
        if (trip.Driver is null)
            throw new ArgumentNullException(nameof(trip.Driver));
        if (trip.Vehicle is null)
            throw new ArgumentNullException(nameof(trip.Vehicle));
    }
}
