using LightCalculatorLab.Interfaces;

namespace LightCalculatorLab.Services;

// Сценарій програми: ввід -> розбір -> обчислення -> вивід.
// Команди: history, clear, trace, exit.
public sealed class CalculatorApp
{
    private const string ExitCommand = "exit";
    private const string HistoryCommand = "history";
    private const string ClearCommand = "clear";
    private const string TraceCommand = "trace";

    private readonly IUserInterface _ui;
    private readonly IExpressionParser _parser;
    private readonly ICalculationService _service;
    private readonly ICalculationHistory _history; // тут лише читаємо, пише декоратор
    private readonly ICalculationLogger _logger;   // для команди trace

    public CalculatorApp(
        IUserInterface ui,
        IExpressionParser parser,
        ICalculationService service,
        ICalculationHistory history,
        ICalculationLogger logger)
    {
        _ui = ui ?? throw new ArgumentNullException(nameof(ui));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _history = history ?? throw new ArgumentNullException(nameof(history));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Run()
    {
        ShowWelcome();

        while (true)
        {
            string? input = _ui.ReadLine("> ");

            if (input is null)
            {
                break; // кінець введення (Ctrl+Z)
            }

            input = input.Trim();

            if (input.Length == 0)
            {
                continue;
            }

            if (IsCommand(input, ExitCommand))
            {
                break;
            }

            if (IsCommand(input, HistoryCommand))
            {
                ShowHistory();
                continue;
            }

            if (IsCommand(input, ClearCommand))
            {
                _history.Clear();
                _ui.ShowMessage("Історію очищено.");
                continue;
            }

            if (IsCommand(input, TraceCommand))
            {
                ToggleTrace();
                continue;
            }

            ProcessInput(input);
        }

        _ui.ShowMessage("До побачення!");
    }

    private static bool IsCommand(string input, string command) =>
        input.Equals(command, StringComparison.OrdinalIgnoreCase);

    private void ShowWelcome()
    {
        _ui.ShowMessage("=== Калькулятор (Proxy + Decorator) ===");
        _ui.ShowMessage("Введіть вираз: операнд оператор операнд (через пробіли).");
        _ui.ShowMessage($"Доступні оператори: {_service.SupportedOperators}");
        _ui.ShowMessage("Операнд: число, ans (останній результат) або #N (результат запису N з історії).");
        _ui.ShowMessage("Приклади: 5 + 3   |   -2,5 * 4   |   ans * 2   |   #1 + #2");
        _ui.ShowMessage("Команди: history, clear, trace (показ, хто рахує), exit");
        _ui.ShowMessage(string.Empty);
    }

    private void ShowHistory()
    {
        if (_history.Entries.Count == 0)
        {
            _ui.ShowMessage("Історія порожня.");
            return;
        }

        for (int i = 0; i < _history.Entries.Count; i++)
        {
            _ui.ShowMessage($"{i + 1}) {_history.Entries[i]}");
        }
    }

    private void ToggleTrace()
    {
        _logger.IsEnabled = !_logger.IsEnabled;
        _ui.ShowMessage(_logger.IsEnabled ? "Трасування увімкнено." : "Трасування вимкнено.");
    }

    private void ProcessInput(string input)
    {
        // 1. Розбір тексту (валідація формату, підстановка ans та #N)
        if (!_parser.TryParse(input, out var request, out string error))
        {
            _ui.ShowError(error);
            return;
        }

        // 2. Чи знаємо такий оператор
        if (!_service.IsSupported(request.Operator))
        {
            _ui.ShowError(
                $"Оператор '{request.Operator}' не підтримується. " +
                $"Доступні: {_service.SupportedOperators}");
            return;
        }

        // 3. Обчислення і обробка помилок
        try
        {
            double result = _service.Calculate(request);

            // Переповнення: результат вийшов за межі double
            if (!double.IsFinite(result))
            {
                _ui.ShowError("Помилка: результат виходить за межі допустимого діапазону.");
                return;
            }

            _ui.ShowMessage($"{request.Left} {request.Operator} {request.Right} = {result}");
        }
        catch (DivideByZeroException ex)
        {
            _ui.ShowError($"Помилка: {ex.Message}");
        }
    }
}
