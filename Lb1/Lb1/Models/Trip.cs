namespace AutobaseSystem.Models;


public class Trip
{
    public int Id { get; private init; }
    public TransportRequest Request { get; private init; }
    public Driver Driver { get; private init; }
    public Vehicle Vehicle { get; private init; }
    public TripStatus Status { get; private set; } = TripStatus.Assigned;
    public decimal Payment { get; private set; } = 0m;

    public Trip(int id, TransportRequest request, Driver driver, Vehicle vehicle)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        Id = id;
        // не можна створити рейс без запиту/водія/авто.
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        Vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
    }

    // Переходи станами - тільки через ці три методи
    public void Start() => Status = TripStatus.InProgress;

    public void ReportBreakdown() => Status = TripStatus.BrokenDown;

    public void Complete(decimal payment)
    {
        if (payment < 0)
            throw new ArgumentOutOfRangeException(nameof(payment), "Оплата не може бути від'ємною.");

        Payment = payment;
        Status = TripStatus.Completed;
    }


    public bool Equals(Trip? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as Trip);

    public override int GetHashCode() => Id.GetHashCode();


    public override string ToString() =>
        $"Рейс #{Id} [{Status}]: {Request.Destination} — {Driver.FullName} на {Vehicle.Model}";
}