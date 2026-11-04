using LightCalculatorLab.Calculators;
using LightCalculatorLab.Infrastructure;
using LightCalculatorLab.Interfaces;
using LightCalculatorLab.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8; // щоб коректно показувалась кирилиця

// Composition Root: єдине місце з new, тут збираються всі залежності
IUserInterface ui = new ConsoleUserInterface();
ICalculationLogger logger = new ConsoleCalculationLogger();
ICalculationHistory history = new CalculationHistory(); // одне сховище на всіх

// Ланцюжок обгорток збираємо зсередини назовні:
ICalculator full = new FullCalculator();                                         // справжні обчислення
ICalculator tracedFull = new TracingCalculator(full, "Full виконує", logger);    // Decorator
ICalculator light = new LightCalculator(tracedFull);                             // Proxy
ICalculator tracedLight = new TracingCalculator(light, "Light отримав", logger); // Decorator
ICalculator withHistory = new HistoryCalculator(tracedLight, history);           // Decorator (зовні, щоб бачити все)

ICalculationService service = new CalculationService(withHistory);
IExpressionParser parser = new ExpressionParser(history);

new CalculatorApp(ui, parser, service, history, logger).Run();
