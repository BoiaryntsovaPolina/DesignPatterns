using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

// Координує повний цикл рейсу (призначення -> поломка -> завершення).
public class AutobaseDispatcher
{

    private readonly IDriverRepository _driverRepo;
    private readonly IVehicleRepository _vehicleRepo;
    private readonly ITripRepository _tripRepo;
    private readonly IDriverSelector _driverSelector;
    private readonly IVehicleSelector _vehicleSelector;
    private readonly IPaymentCalculator _paymentCalculator;
    private readonly IRepairService _repairService;


    private readonly IEnumerable<ITripStartNotifier> _startNotifiers;
    private readonly IEnumerable<ITripCompletionNotifier> _completionNotifiers;
    private readonly IEnumerable<IBreakdownNotifier> _breakdownNotifiers;
    private int _nextTripId = 1;

    // DIP: усі залежності приймаються ззовні через конструктор
    public AutobaseDispatcher(
        IDriverRepository driverRepo,
        IVehicleRepository vehicleRepo,
        ITripRepository tripRepo,
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
        var driver = _driverSelector.SelectDriver(request, _driverRepo.GetAvailable());
        if (driver == null) return null; // немає підходящого водія - рейс не створюється

        var vehicle = _vehicleSelector.SelectVehicle(request, _vehicleRepo.GetAvailable());
        if (vehicle == null) return null; // те саме для авто

        var trip = new Trip(_nextTripId++, request, driver, vehicle);
        trip.Start();

        _driverRepo.SetAvailability(driver, false);
        _vehicleRepo.SetAvailability(vehicle, false);
        _tripRepo.Add(trip);

        foreach (var notifier in _startNotifiers)
            notifier.NotifyTripStarted(trip);

        return trip;
    }

    // Поломка під час рейсу: перевести стан рейсу + подати заявку на ремонт авто.
    public void ReportBreakdown(Trip trip)
    {
        trip.ReportBreakdown();
        _repairService.RequestRepair(trip);

        foreach (var notifier in _breakdownNotifiers)
            notifier.NotifyBreakdown(trip);
    }

    /* Завершення рейсу: якщо авто ще "у ремонті" - завершуємо ремонт, рахуємо оплату,
     звільняємо і водія, і авто (вони знову доступні для нових заявок).*/
    public void CompleteTrip(Trip trip)
    {
        if (trip.Vehicle.Condition != VehicleCondition.Operational)
            _repairService.CompleteRepair(trip);

        var payment = _paymentCalculator.CalculatePayment(trip);
        trip.Complete(payment);
        trip.Driver.AddEarnings(payment);

        _driverRepo.SetAvailability(trip.Driver, true);
        _vehicleRepo.SetAvailability(trip.Vehicle, true);

        foreach (var notifier in _completionNotifiers)
            notifier.NotifyTripCompleted(trip);
    }
}
