namespace ClanBattle;

public class MainMenu
{
    private readonly GameSettings _settings;

    public MainMenu(GameSettings settings) => _settings = settings;

    public void Run()
    {
        while (true)
        {
            Palette.ClearScreen();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Texts.Banner);
            Console.ResetColor();

            Console.WriteLine("    1. Нова гра");
            Console.WriteLine("    2. Налаштування");
            Console.WriteLine("    3. Як грати");
            Console.WriteLine("    4. Огляд кланів (ЛР 2–3: склад, Глава, Singleton)");
            Console.WriteLine(SaveGameFile.Exists
                ? "    5. Продовжити збережену гру"
                : "    5. Продовжити збережену гру (збережень ще немає)");
            Console.WriteLine("    0. Вихід");
            Console.Write("\n  Ваш вибір > ");

            string? choice = Console.ReadLine();
            if (choice is null) return;

            switch (choice.Trim())
            {
                case "1":
                    new GameUi(_settings).Play();
                    break;
                case "2":
                    SettingsMenu();
                    break;
                case "3":
                    Console.WriteLine(Texts.Help);
                    ConsoleHelper.Pause();
                    break;
                case "4":
                    Lab23Demo();
                    break;
                case "5":
                    ContinueSaved();
                    break;
                case "0":
                    return;
            }
        }
    }

    private void ContinueSaved()
    {
        if (!SaveGameFile.Exists)
        {
            Console.WriteLine("\n  Збереженої гри ще немає: вона створюється автоматично на початку ходу й при виході з бою.");
            ConsoleHelper.Pause();
            return;
        }

        LoadedGame? loaded = SaveGameFile.Load(out string? error);
        if (loaded is null)
        {
            Console.WriteLine($"\n  {error} Файл: {SaveGameFile.Path}");
            ConsoleHelper.Pause();
            return;
        }

        new GameUi(_settings, loaded).Play();
    }

    // Один пункт меню налаштувань: назва, поточне значення з варіантами, пояснення й дія зміни.
    // Меню збирається з таблиці, тому новий пункт — один рядок у BuildSettings
    private sealed record SettingItem(string Title, Func<string> Show, string Help, Action Change);

    // "[поточне] / інше": поточний варіант у дужках
    private static string Pick(int current, params string[] options) =>
        string.Join(" / ", options.Select((o, i) => i == current ? $"[{o}]" : o));

    private List<SettingItem> BuildSettings() => new()
    {
        new("Назва вашого клану", () => _settings.PlayerClanName,
            "Підпис у журналі й на панелях. На правила не впливає.",
            () => _settings.PlayerClanName = AskText("Нова назва вашого клану", _settings.PlayerClanName)),

        new("Назва ворожого клану", () => _settings.EnemyClanName,
            "Те саме для ворога.",
            () => _settings.EnemyClanName = AskText("Нова назва ворожого клану", _settings.EnemyClanName)),

        new("Вивід", () => Pick(_settings.GraphicMode ? 0 : 1, "псевдографіка", "простий текст"),
            "Псевдографіка: кольорова карта з підсвіткою. Простий текст: списки бійців з координатами (X; Y), без кольорів.",
            () => _settings.GraphicMode = !_settings.GraphicMode),

        new("Показ ланцюга наказів", () => Pick(_settings.ShowChain ? 0 : 1, "так", "ні"),
            "Показує, як наказ іде від воїна до воїна: Танукі-1✔ → Некомата-1· (✔ виконав, · пропустив). На гру не впливає.",
            () => _settings.ShowChain = !_settings.ShowChain),

        new("Складність", () => Pick(_settings.Difficulty == Difficulty.Easy ? 0 : 1, "легка", "складна"),
            $"Легка: ворог агресивний (рветься вперед, стріляє, святилищем не переймається); до світанку потрібно {GameConfig.ShrineGoalEasy} очок.\n" +
            $"      Складна: ворог обережний (відводить поранених, займає й тримає святилище); до світанку потрібно {GameConfig.ShrineGoalHard} очок.",
            () => _settings.Difficulty = _settings.Difficulty == Difficulty.Easy ? Difficulty.Hard : Difficulty.Easy),

        new("Святилище Інарі", () => Pick((int)_settings.Shrine, "змагання", "лише бонуси", "вимкнене"),
            "Змагання: очко дає більшість бійців на святилищі, а Інарі обирає переможця лише на світанку (Акт III): потрібен поріг очок і перевага.\n" +
            "      Лише бонуси: святилище лікує й додає Главі ярості, очок немає.\n" +
            "      Вимкнене: чисті правила лабораторних — перемога розгромом або кількістю бійців на 25-му ході.",
            () => _settings.Shrine = (ShrineMode)(((int)_settings.Shrine + 1) % 3)),

        new("Накази за хід", () => Pick(_settings.KiPerTurn == 2 ? 0 : 1, "два", "один"),
            "Два: кожен клан за хід віддає 2 накази (динамічніше).\n" +
            "      Один: по одному наказу, начальники кланів ходять по черзі — буквально як у завданні ЛР 5.",
            () => _settings.KiPerTurn = _settings.KiPerTurn == 2 ? 1 : 2),

        new("Seed (відтворення бою)", () => Pick(_settings.Seed is null ? 0 : 1, "випадковий", _settings.Seed?.ToString() ?? "число"),
            "Те саме число — та сама розстановка й ті самі «кубики». Зручно для захисту й повторів.",
            () =>
            {
                Console.Write("  Число (Enter — випадковий) > ");
                _settings.Seed = int.TryParse(Console.ReadLine()?.Trim(), out int seed) ? seed : null;
            }),

        new("Прогноз наказу", () => Pick(_settings.PreviewMode ? 0 : 1, "так", "ні"),
            "Так: перед виконанням карта показує, хто куди піде й кого вдарить; Enter — виконати, інше — скасувати.\n" +
            "      Ні: накази виконуються одразу.",
            () => _settings.PreviewMode = !_settings.PreviewMode),

        new("Керування", () => Pick(_settings.KeyMode ? 0 : 1, "клавіші", "текстові команди"),
            "Клавіші: цифра 1–4 (загін) + → ← Пробіл, без Enter; W A S D — курсор огляду й фокус-цілі.\n" +
            "      Текст: команда рядком, напр. 3f (Кіцуне б'ються) + Enter. Зручно, якщо термінал не приймає клавіші.",
            () => _settings.KeyMode = !_settings.KeyMode),
    };

    private void SettingsMenu()
    {
        List<SettingItem> items = BuildSettings();

        while (true)
        {
            Palette.ClearScreen();
            Console.WriteLine("\n  ═══ НАЛАШТУВАННЯ ═══   (номер — змінити, 0 — назад; зміни діють з наступної гри)\n");

            for (int i = 0; i < items.Count; i++)
            {
                Console.WriteLine($"    {i + 1}. {items[i].Title}:  {items[i].Show()}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"      {items[i].Help}");
                Console.ResetColor();
            }

            Console.WriteLine("    0. Назад");
            Console.Write("\n  Що змінити? > ");

            string? choice = Console.ReadLine();
            if (choice is null) return;

            if (choice.Trim() == "0") return;

            if (int.TryParse(choice.Trim(), out int n) && n >= 1 && n <= items.Count)
                items[n - 1].Change();
        }
    }

    private static string AskText(string prompt, string current)
    {
        Console.Write($"  {prompt} (Enter — лишити) > ");
        string? input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? current : input.Trim();
    }

    // Демонстрація ЛР 2–3 у новій грі: склад кланів, окремий вивід Глави, Singleton при зміні Глави
    private void Lab23Demo()
    {
        GameRandom.Reseed(_settings.Seed ?? Environment.TickCount);

        var factory = new ClanFactory(new RandomTraitsEnhancer());
        Clan clanA = factory.CreateClan(_settings.PlayerClanName, ClanSide.West);
        Clan clanB = factory.CreateClan(_settings.EnemyClanName, ClanSide.East);

        // Bridge: рендерер обираємо одним рядком, решта коду не змінюється
        IClanRenderer renderer = _settings.GraphicMode ? new GraphicClanRenderer() : new TextClanRenderer();
        ClanReport compositionReport = new ClanCompositionReport(renderer);
        ClanReport leaderReport = new LeaderReport(renderer);

        Palette.ClearScreen();
        foreach (Clan clan in new[] { clanA, clanB })
        {
            compositionReport.Print(clan);
            leaderReport.Print(clan);   // Глава виводиться окремо
        }

        // Демонстрація Singleton: один реєстр обслуговує обидва клани
        Console.WriteLine($"\n>>> Главу клану «{clanA.Name}» переможено в бою! Визначаємо спадкоємицю...\n");

        IWarrior? fallen = clanA.Leader;
        if (fallen is not null)
            fallen.State = OutOfBattleState.Instance;

        if (clanA.ChooseLeader())
            leaderReport.Print(clanA);
        else
            Console.WriteLine($"Клан «{clanA.Name}» розгромлено повністю — живих воїнів не лишилось.");

        ConsoleHelper.Pause();
    }
}
