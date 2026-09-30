namespace AutobaseSystem.Models;

public class Trip : Entity
{
    public TransportRequest Request { get; private init; }
    public Driver Driver { get; private init; }
    public Vehicle Vehicle { get; private init; }
    public TripStatus Status { get; private set; } = TripStatus.Assigned;
    public decimal Payment { get; private set; } = 0m;


    internal Trip(int id, TransportRequest request, Driver driver, Vehicle vehicle) : base(id)
    {
        Request = request;
        Driver = driver;
        Vehicle = vehicle;
    }

    // Дозволені переходи: Assigned -> InProgress -> Completed; InProgress -> BrokenDown -> Completed.
    // Assigned -> InProgress
    internal void Start()
    {
        if (Status != TripStatus.Assigned)
            throw new InvalidOperationException($"Рейс #{Id}: розпочати можна лише призначений рейс (зараз: {Status}).");
        Status = TripStatus.InProgress;
    }

    // InProgress -> BrokenDown
    internal void ReportBreakdown()
    {
        if (Status != TripStatus.InProgress)
            throw new InvalidOperationException($"Рейс #{Id}: про поломку можна повідомити лише під час рейсу (зараз: {Status}).");
        Status = TripStatus.BrokenDown;
    }

    // InProgress або BrokenDown -> Completed
    internal void Complete(decimal payment)
    {
        if (Status is not (TripStatus.InProgress or TripStatus.BrokenDown))
            throw new InvalidOperationException($"Рейс #{Id}: завершити можна лише рейс у дорозі або після поломки (зараз: {Status}).");
        if (payment < 0)
            throw new ArgumentOutOfRangeException(nameof(payment), "Оплата не може бути від'ємною.");
        Payment = payment;
        Status = TripStatus.Completed;
    }

    public override string ToString() =>
        $"Рейс #{Id} [{Status}]: {Request.Destination} — {Driver.FullName} на {Vehicle.Model}";
}
