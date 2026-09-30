using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;
using AutobaseSystem.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Система \"Автобаза\" ===\n");


var drivers = new List<Driver>
{
    new Driver(1, "Іван Петренко", 1),
    new Driver(2, "Олена Ковальчук", 4),
    new Driver(3, "Сергій Бондар", 8),
    new Driver(4, "Марія Гончар", 6),
    new Driver(5, "Петро Ткаченко", 3)
};

var vehicles = new List<Vehicle>
{
    new Vehicle(1, "Renault Master", 1500, 3),
    new Vehicle(2, "MAN TGX", 15000, 8),
    new Vehicle(3, "Mercedes Sprinter", 3000, 4),
    new Vehicle(4, "Volvo FH", 20000, 9)
};

var requestRepo = new InMemoryRequestRepository();
var driverRepo = new InMemoryDriverRepository(drivers);
var vehicleRepo = new InMemoryVehicleRepository(vehicles);
var tripRepo = new InMemoryTripRepository();

var driverSelector = new ExperienceBasedDriverSelector();
var vehicleSelector = new CapacityOptimizedVehicleSelector();
var paymentCalculator = new DistanceBasedPaymentCalculator();
var repairService = new SimpleRepairService(vehicleRepo);

// Один об'єкт ConsoleNotifier підписаний одразу на всі 3 типи подій (старт/завершення/поломка).
var consoleNotifier = new ConsoleNotifier();
var startNotifiers = new List<ITripStartNotifier> { consoleNotifier };
var completionNotifiers = new List<ITripCompletionNotifier> { consoleNotifier };
var breakdownNotifiers = new List<IBreakdownNotifier> { consoleNotifier };

// послідовність заявок однакова при кожному запуску (зручно для демонстрації).
var random = new Random(42);
IRequestGenerator requestGenerator = new RandomRequestGenerator(random);

var dispatcher = new AutobaseDispatcher(
    driverRepo, vehicleRepo, tripRepo, driverSelector, vehicleSelector,
    paymentCalculator, repairService, startNotifiers, completionNotifiers, breakdownNotifiers);

const int requestsPerDay = 4;
var activeTrips = new List<Trip>();

Console.WriteLine("--- Надходження заявок та призначення рейсів ---\n");
for (int i = 0; i < requestsPerDay; i++)
{
    var request = requestGenerator.GenerateRequest();
    if (request is null)
    {

        Console.WriteLine("Нових заявок більше немає.");
        continue;
    }

    requestRepo.Add(request);

    var trip = dispatcher.TryAssignTrip(request);
    if (trip != null)
    {
        requestRepo.MarkAsAssigned(request);
        activeTrips.Add(trip);
        // 30% шанс поломки під час рейсу (умова завдання: "автомобіль може зламатися").
        if (random.NextDouble() < 0.3) dispatcher.ReportBreakdown(trip);
    }
    // якщо trip == null - не знайшлося водія/авто під заявку, вона лишається в requestRepo як pending

    if (activeTrips.Count > 0 && random.NextDouble() < 0.5)
    {
        var finishedTrip = activeTrips[0];
        activeTrips.RemoveAt(0);
        dispatcher.CompleteTrip(finishedTrip);
    }
}

Console.WriteLine("\n--- Завершення рейсів ---\n");
foreach (var trip in activeTrips)
    dispatcher.CompleteTrip(trip);

Console.WriteLine("\n--- Підсумок дня: заробіток водіїв ---");
foreach (var driver in driverRepo.GetAll().OrderByDescending(d => d.TotalEarnings))
    Console.WriteLine(driver);