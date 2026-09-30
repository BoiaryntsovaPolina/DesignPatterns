using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

// Координує повний цикл рейсу (призначення -> поломка -> завершення).
public class AutobaseDispatcher
{
    private readonly IDriverRepository _driverRepo;
    private readonly IVehicleRepository _vehicleRepo;
    private readonly ITripRepository _tripRepo;
    private readonly IRequestRepository _requestRepo;
    private readonly ITripFactory _tripFactory;
    private readonly IDriverSelector _driverSelector;
    private readonly IVehicleSelector _vehicleSelector;
    private readonly IPaymentCalculator _paymentCalculator;
    private readonly IRepairService _repairService;
    private readonly IEnumerable<ITripStartNotifier> _startNotifiers;
    private readonly IEnumerable<ITripCompletionNotifier> _completionNotifiers;
    private readonly IEnumerable<IBreakdownNotifier> _breakdownNotifiers;


    public AutobaseDispatcher(
        IDriverRepository driverRepo,
        IVehicleRepository vehicleRepo,
        ITripRepository tripRepo,
        IRequestRepository requestRepo,
        ITripFactory tripFactory,
        IDriverSelector driverSelector,
        IVehicleSelector vehicleSelector,
        IPaymentCalculator paymentCalculator,
        IRepairService repairService,
        IEnumerable<ITripStartNotifier> startNotifiers,
        IEnumerable<ITripCompletionNotifier> completionNotifiers,
        IEnumerable<IBreakdownNotifier> breakdownNotifiers)
    {
        _driverRepo = driverRepo;
        _vehicleRepo = vehicleRepo;
        _tripRepo = tripRepo;
        _requestRepo = requestRepo;
        _tripFactory = tripFactory;
        _driverSelector = driverSelector;
        _vehicleSelector = vehicleSelector;
        _paymentCalculator = paymentCalculator;
        _repairService = repairService;
        _startNotifiers = startNotifiers;
        _completionNotifiers = completionNotifiers;
        _breakdownNotifiers = breakdownNotifiers;
    }

    // Заявка -> Рейс: підбираємо водія і авто, і лише якщо ОБИДВА знайшлися - створюємо рейс.
    public Trip? TryAssignTrip(TransportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        // Захист від повторного призначення: одну заявку не можна віддати в роботу двічі
        if (_tripRepo.GetAll().Any(t => t.Request.Equals(request)))
            throw new InvalidOperationException($"Заявку #{request.Id} уже призначено на рейс.");
        var driver = _driverSelector.SelectDriver(request, _driverRepo.GetAvailable());
        if (driver == null) return null; // немає підходящого водія - рейс не створюється
        var vehicle = _vehicleSelector.SelectVehicle(request, _vehicleRepo.GetAvailable());
        if (vehicle == null) return null; // те саме для авто
        var trip = _tripFactory.Create(request, driver, vehicle);

        _requestRepo.MarkAsAssigned(request);
        trip.Start(); // Assigned -> InProgress
        _driverRepo.SetAvailability(driver, false);   // водій і авто тепер зайняті
        _vehicleRepo.SetAvailability(vehicle, false);
        _tripRepo.Add(trip);
        // Кожен підписник отримує сповіщення (ConsoleNotifier).
        foreach (var notifier in _startNotifiers)
            notifier.NotifyTripStarted(trip);
        return trip;
    }

    // Поломка під час рейсу: перевести стан рейсу + подати заявку на ремонт авто
    public void ReportBreakdown(Trip trip)
    {
        trip.ReportBreakdown();
        _repairService.RequestRepair(trip);
        foreach (var notifier in _breakdownNotifiers)
            notifier.NotifyBreakdown(trip);
    }

    /* Завершення рейсу: спочатку рахуємо оплату і переводимо сам рейс у Completed,
     і лише потім: завершуємо ремонт (якщо авто ще "у ремонті"), нараховуємо заробіток,
     звільняємо і водія, і авто (вони знову доступні для нових заявок).*/
    public void CompleteTrip(Trip trip)
    {
        var payment = _paymentCalculator.CalculatePayment(trip); // лише розрахунок, стан нічого не змінює
        trip.Complete(payment); // перевіряє стан рейсу; при помилці далі нічого не виконується
        if (trip.Vehicle.Condition != VehicleCondition.Operational)
            _repairService.CompleteRepair(trip);
        trip.Driver.AddEarnings(payment);
        _driverRepo.SetAvailability(trip.Driver, true);
        _vehicleRepo.SetAvailability(trip.Vehicle, true);
        foreach (var notifier in _completionNotifiers)
            notifier.NotifyTripCompleted(trip);
    }


    public void ReportBreakdown(int tripId) => ReportBreakdown(GetTripOrThrow(tripId));
    public void CompleteTrip(int tripId) => CompleteTrip(GetTripOrThrow(tripId));

    private Trip GetTripOrThrow(int tripId) =>
        _tripRepo.GetById(tripId)
        ?? throw new KeyNotFoundException($"Рейс #{tripId} не знайдено.");
}
