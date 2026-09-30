using AutobaseSystem.Builders;
using AutobaseSystem.Factories;
using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;
using AutobaseSystem.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Система \"Автобаза\" ===\n");

// Патерн 1. SINGLETON: один генератор Id на всю програму. Id більше не задаються вручну.
IIdGenerator idGenerator = IdGenerator.Instance;

// Окремий шар перевірки даних
var driverValidator = new DriverValidator();
var requestValidator = new TransportRequestValidator();
var tripValidator = new TripValidator();

// Патерн 2. FACTORY METHOD (GoF): творці авто. Кожен ConcreteCreator знає, який клас створювати (Van / Truck)
VehicleCreator vanCreator = new VanCreator(idGenerator, new VanValidator());
VehicleCreator truckCreator = new TruckCreator(idGenerator, new TruckValidator());

// Водії та рейси - прості фабрики за інтерфейсом (продукт один, ієрархії немає).
var driverFactory = new DriverFactory(idGenerator, driverValidator);
var tripFactory = new TripFactory(idGenerator, tripValidator);

// Патерн 3. BUILDER: заявку збираємо покроково (використовується в RandomRequestGenerator).
var requestBuilder = new TransportRequestBuilder(idGenerator, requestValidator);

// Початкові дані: водіїв і авто створюємо лише через фабрики (Id + перевірка - всередині)
var drivers = new List<Driver>
{
    driverFactory.Create("Іван Петренко", 1),
    driverFactory.Create("Олена Ковальчук", 4),
    driverFactory.Create("Сергій Бондар", 8),
    driverFactory.Create("Марія Гончар", 6),
    driverFactory.Create("Петро Ткаченко", 3)
};

var vehicles = new List<Vehicle>
{
    vanCreator.Create("Renault Master", 1500, 3),
    truckCreator.Create("MAN TGX", 15000, 8),
    vanCreator.Create("Mercedes Sprinter", 3000, 4),
    truckCreator.Create("Volvo FH", 20000, 9)
};

// Репозиторії, стратегії підбору, розрахунок оплати, ремонт.
var requestRepo = new InMemoryRequestRepository();
var driverRepo = new InMemoryDriverRepository(drivers);
var vehicleRepo = new InMemoryVehicleRepository(vehicles);
var tripRepo = new InMemoryTripRepository();
var driverSelector = new ExperienceBasedDriverSelector();
var vehicleSelector = new CapacityOptimizedVehicleSelector();
var paymentCalculator = new DistanceBasedPaymentCalculator();
var repairService = new SimpleRepairService(vehicleRepo);

// Один об'єкт ConsoleNotifier підписаний одразу на всі 3 типи подій (старт / завершення / поломка).
var consoleNotifier = new ConsoleNotifier();
var startNotifiers = new List<ITripStartNotifier> { consoleNotifier };
var completionNotifiers = new List<ITripCompletionNotifier> { consoleNotifier };
var breakdownNotifiers = new List<IBreakdownNotifier> { consoleNotifier };

var random = new Random(42);
IRequestGenerator requestGenerator = new RandomRequestGenerator(random, requestBuilder);

var dispatcher = new AutobaseDispatcher(
    driverRepo, vehicleRepo, tripRepo, requestRepo, tripFactory, driverSelector, vehicleSelector,
    paymentCalculator, repairService, startNotifiers, completionNotifiers, breakdownNotifiers);

// Демонстрація Factory Method
Console.WriteLine("--- Автопарк (авто створені через Factory Method) ---");
foreach (var vehicle in vehicles)
    Console.WriteLine(vehicle); // Kind (Мікроавтобус / Вантажівка) - абстрактна властивість, її реалізує кожен продукт

// У кожного творця свої правила: мікроавтобус на 9 т відхиляє VanValidator.
Console.WriteLine();
try
{
    vanCreator.Create("Ford Transit", 9_000, 4);
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"[Перевірка] {ex.Message}");
}

// Сценарій дня
const int requestsPerDay = 4;

// Зберігаємо лише Id активних рейсів: диспетчер сам знаходить рейс у репозиторії за Id.
var activeTripIds = new List<int>();

Console.WriteLine("\n--- Надходження заявок та призначення рейсів ---\n");

for (int i = 0; i < requestsPerDay; i++)
{
    // Builder збирає заявку всередині генератора
    var request = requestGenerator.GenerateRequest();
    if (request is null)
    {
        Console.WriteLine("Нових заявок більше немає.");
        continue;
    }

    // Заявка має потрапити в репозиторій ДО призначення: позначити її призначеною диспетчер сам у TryAssignTrip.
    requestRepo.Add(request);

    var trip = dispatcher.TryAssignTrip(request);
    if (trip != null)
    {
        activeTripIds.Add(trip.Id);

        // 30% шанс поломки під час рейсу (умова завдання: "автомобіль може зламатися").
        if (random.NextDouble() < 0.3) dispatcher.ReportBreakdown(trip);
    }

    // Якщо trip == null - не знайшлося водія/авто під заявку, вона лишається в requestRepo як pending.
    // 50% шанс, що найстаріший активний рейс завершується.
    if (activeTripIds.Count > 0 && random.NextDouble() < 0.5)
    {
        var finishedTripId = activeTripIds[0];
        activeTripIds.RemoveAt(0);
        dispatcher.CompleteTrip(finishedTripId);
    }
}

Console.WriteLine("\n--- Завершення рейсів ---\n");
foreach (var tripId in activeTripIds)
    dispatcher.CompleteTrip(tripId);

Console.WriteLine("\n--- Підсумок дня: заробіток водіїв ---");
foreach (var driver in driverRepo.GetAll().OrderByDescending(d => d.TotalEarnings))
    Console.WriteLine(driver);
