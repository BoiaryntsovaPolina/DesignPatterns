namespace ClanBattle;

// Ігровий цикл: показ екрана, введення наказів (текстом або клавішами), хід ворога, контрольні точки
public class GameUi
{
    private readonly GameSettings _settings;

    private BattleSession _session = null!;
    private CheckpointManager _checkpoints = null!;
    private IAiStrategy _ai = null!;
    private IBattleRenderer _renderer = null!;
    private BattleReport _report = null!;
    private KeyboardController _keys = null!;

    private bool _useKeys;        // керування клавішами (вимикається, якщо ввід перенаправлено)
    private string? _notice;      // повідомлення гравцю під екраном (помилки, підтвердження)
    private string? _lastTrace;   // слід останнього наказу по ланцюгу
    private bool _showThreat;     // підсвічувати загрозу від ворога (клавіша T)

    private readonly LoadedGame? _loaded;   // не null — продовжуємо збережену гру
    private int _autosavedTurn;              // хід, на якому востаннє писали автозбереження

    public GameUi(GameSettings settings, LoadedGame? loaded = null)
    {
        _settings = settings;
        _loaded = loaded;
    }

    public void Play()
    {
        Clan player;
        if (_loaded is null)
        {
            GameRandom.Reseed(_settings.Seed ?? Environment.TickCount);

            // Усі патерни зустрічаються тут: фабрика клану, ШІ-стратегія, Bridge-рендерер, Memento-опікун
            var clanFactory = new ClanFactory(new RandomTraitsEnhancer());
            player = clanFactory.CreateClan(_settings.PlayerClanName, ClanSide.West);
            Clan enemy = clanFactory.CreateClan(_settings.EnemyClanName, ClanSide.East);

            _session = new BattleSession(player, enemy, _settings.Difficulty, _settings.Shrine, _settings.KiPerTurn);
            _checkpoints = new CheckpointManager();
        }
        else
        {
            // Збережена гра: партія вже зібрана з файлу, «кубики» теж повернуто
            _session = _loaded.Session;
            player = _session.Player;
            _checkpoints = new CheckpointManager(_loaded.UndoCharges);
        }

        _ai = AiStrategies.For(_session.Difficulty);
        _renderer = _settings.GraphicMode ? new GraphicBattleRenderer() : new TextBattleRenderer();
        _report = new FullBattleReport(_renderer);
        _useKeys = _settings.KeyMode && !Console.IsInputRedirected;
        _keys = new KeyboardController(player.Leader?.Position ?? new Position(GameConfig.FieldWidth / 2 - 2, GameConfig.FieldHeight / 2));
        _notice = null;
        _lastTrace = null;
        _showThreat = false;

        Palette.ClearScreen();
        if (_loaded is null)
        {
            Console.WriteLine(Texts.Intro);
            Console.WriteLine($"  Seed цієї битви: {GameRandom.Seed}   (введіть його в налаштуваннях, щоб повторити бій)");
        }
        else
        {
            Console.WriteLine($"\n  Гру продовжено: {_loaded.Description}.");
            Console.WriteLine("  Контрольні слоти C/L почнуться заново: вони живуть лише в пам'яті.");
        }
        Console.WriteLine($"  Ворожий Глава: {_ai.Name}. Керування: {(_useKeys ? "клавіші (H — довідка)" : "текстові команди (H — довідка)")}");
        if (_settings.GraphicMode && Palette.Rows < 36)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  Вікно консолі низьке ({Palette.Rows} рядків): карту може бути видно не повністю.");
            Console.WriteLine("  Розгорніть вікно на весь екран (або зменште шрифт) — для гри зручно від 36 рядків.");
            Console.ResetColor();
        }
        ConsoleHelper.Pause(_loaded is null ? "\n  Enter — розпочати битву... " : "\n  Enter — продовжити битву... ");

        _checkpoints.PushTurnStart(_session.CreateMemento(_loaded is null ? "Початок ходу 1" : $"Продовження, хід {_session.Turn}"));
        _autosavedTurn = _loaded is null ? 0 : _session.Turn;

        while (true)
        {
            while (_session.Outcome == BattleOutcome.None)
            {
                if (!ReferenceEquals(_session.Active, _session.Player))
                {
                    EnemyTurn();
                    continue;
                }

                if (!PlayerStep())
                    return;   // гравець вийшов із бою
            }

            SaveGameFile.Delete();   // завершений бій продовжувати нема сенсу
            ShowResult();

            // Після невдачі Дзеркало Спогадів ще може врятувати (питання саме чекає на відповідь)
            if (_session.Outcome != BattleOutcome.PlayerWon && HasRewind())
            {
                if (AskRewind())
                    continue;   // час повернуто — бій триває
                return;
            }

            // Інакше просто чекаємо, щоб меню не стерло екран із результатом
            ConsoleHelper.Pause("\n  Enter — у головне меню... ");
            return;
        }
    }

    // ---------- Хід гравця ----------

    // false — гравець вийшов із бою
    private bool PlayerStep()
    {
        // Автозбереження на початку кожного ходу гравця
        if (_session.CommandsUsed == 0 && _session.Turn != _autosavedTurn)
        {
            _autosavedTurn = _session.Turn;
            _notice ??= SaveGameFile.Save(_session, _checkpoints.UndoCharges);
        }

        ParsedInput? input = _useKeys ? ReadKeyInput() : ReadTextInput();
        if (input is null)
        {
            SaveGameFile.Save(_session, _checkpoints.UndoCharges);
            return false;
        }

        switch (input.Kind)
        {
            case InputKind.Command:
                ProposeCommand(WithFocus(input.Command!, input.Focus));
                break;

            case InputKind.NeedSquad:
                BattleCommand? chosen = AskSquad(input.Action);
                if (chosen is null) _notice = "Наказ скасовано.";
                else ProposeCommand(chosen);
                break;

            case InputKind.SelectAtCursor:
                SelectUnderCursor();
                break;

            case InputKind.Save:
                SaveCheckpoint();
                break;

            case InputKind.Load:
                LoadCheckpoint();
                break;

            case InputKind.Undo:
                UndoTurn();
                break;

            case InputKind.Info:
                ShowClanInfo();
                break;

            case InputKind.Threat:
                _showThreat = !_showThreat;
                _notice = _showThreat
                    ? "Загрозу показано: пурпур — вороги, що вас дістають; жовтогарячий — ваші бійці під ударом. T — вимкнути."
                    : "Загрозу сховано.";
                break;

            case InputKind.Help:
                Palette.ClearScreen();
                Console.WriteLine(Texts.Help);
                ConsoleHelper.Pause();
                break;

            case InputKind.EndTurn:
                EndPlayerTurn();
                break;

            case InputKind.Quit:
                if (ConsoleHelper.Confirm("Покинути бій і повернутись у меню? (гру буде збережено)"))
                {
                    string? failure = SaveGameFile.Save(_session, _checkpoints.UndoCharges);
                    if (failure is not null) Console.WriteLine($"  {failure}");
                    else Console.WriteLine("  Гру збережено: «Продовжити» в головному меню.");
                    ConsoleHelper.Pause();
                    return false;
                }
                break;

            default:
                _notice = "Не зрозумів. Наказ = номер загону + дія, напр. 3f. H — довідка.";
                break;
        }

        // Ки вичерпано: даємо побачити результат останнього наказу й передаємо хід ворогу
        if (_session.Outcome == BattleOutcome.None
            && ReferenceEquals(_session.Active, _session.Player)
            && _session.KiLeft <= 0)
        {
            _report.Show(_session, _lastTrace, CurrentView());
            ConsoleHelper.Pause("\n  Накази вичерпано. Enter — хід ворога... ");
            EndPlayerTurn();
        }

        return true;
    }

    // Фокус-ціль: «битися» з курсором на ворогові — усі, хто дістає, б'ють саме його
    private BattleCommand WithFocus(BattleCommand command, Position? cell)
    {
        if (command is FightCommand fight && cell is { } c
            && _session.Field.WarriorAt(c) is { } who && ReferenceEquals(_session.Field.ClanOf(who), _session.Enemy))
        {
            return new FightCommand(fight.Target, c);
        }

        return command;
    }

    // Що малювати на карті зараз: у режимі клавіш — ще й курсор і вибраний загін
    private BattleView CurrentView() => _useKeys
        ? new BattleView(null, _showThreat, _keys.Cursor, _keys.SelectedKey)
        : new BattleView(null, _showThreat);

    private void PrintNoticeAndHint()
    {
        if (_notice is not null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ! " + _notice);
            Console.ResetColor();
            _notice = null;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Порада: " + HintProvider.For(_session, _useKeys));
        Console.ResetColor();
    }

    // Текстовий режим: "3f", "4w", "a"... + Enter
    private ParsedInput? ReadTextInput()
    {
        _report.Show(_session, _lastTrace, CurrentView());
        PrintNoticeAndHint();

        Console.WriteLine($"\n  Наказів: {_session.KiLeft} │ Відкатів часу: {_checkpoints.UndoCharges} │ " +
                          "3f — Кіцуне б'ються, 4w — усі вперед, A — здібність, T — загроза, H — довідка");
        Console.Write("  Ваш наказ > ");

        string? line = Console.ReadLine();
        return line is null ? null : InputParser.Parse(line);
    }

    // Режим клавіш: кожне натискання перемальовує екран, поки не буде наказу
    private ParsedInput? ReadKeyInput()
    {
        while (true)
        {
            _report.Show(_session, _lastTrace, CurrentView());
            PrintNoticeAndHint();

            Console.WriteLine($"  Наказів: {_session.KiLeft} │ Відкатів: {_checkpoints.UndoCharges} │ Обрано: {_keys.SelectedText}");
            Console.WriteLine("  1-4 загін · W A S D курсор · →/← вперед/назад · Пробіл бій (курсор на ворогові = фокус) · X здібність");
            Console.WriteLine("  C/L збереж./завант. · U відкат · T загроза · I клан · E кінець ходу · H довідка · Q вихід");

            ConsoleKeyInfo key;
            try
            {
                key = Console.ReadKey(true);
            }
            catch (InvalidOperationException)
            {
                _useKeys = false;   // ввід перенаправлено — переходимо на текстовий режим
                return ReadTextInput();
            }

            ParsedInput? result = _keys.Handle(key);
            if (_keys.Notice is not null)
                _notice = _keys.Notice;

            if (result is not null)
                return result;
        }
    }

    // Enter на своєму бійці обирає його загін; на ворогові — нагадує, що обирати можна лише своїх
    private void SelectUnderCursor()
    {
        IWarrior? under = _session.Field.WarriorAt(_keys.Cursor);

        if (under is null)
            _notice = "Тут нікого немає. Enter на своєму бійці обирає його загін.";
        else if (_session.Player.AllWarriors.Contains(under))
        {
            _keys.Select(under.Type.Key());
            _notice = $"Обрано загін: {under.Type.ToUa()}.";
        }
        else
            _notice = "Це ворог: його картка внизу. Обирати можна лише свої загони (щоб бити його — фокус: курсор на ньому + Пробіл).";
    }

    private BattleCommand? AskSquad(char action)
    {
        Console.Write("  Який загін? 1 — Танукі, 2 — Некомата, 3 — Кіцуне, 4 — усі > ");
        string? answer = Console.ReadLine()?.Trim();

        if (!string.IsNullOrEmpty(answer) && InputParser.TryParseSquad(answer[0], out SquadType? target))
            return CommandFactory.Create(action, target);

        return null;
    }

    // Прогноз -> підтвердження -> виконання. Той самий наказ іде тим самим ланцюгом,
    // але спершу з PreviewMediator (нічого не змінює), а після підтвердження — зі справжнім BattleMediator
    private void ProposeCommand(BattleCommand command)
    {
        string? error = _session.Validate(command);
        if (error is not null)
        {
            _notice = error;
            return;
        }

        if (!_settings.PreviewMode)
        {
            ExecuteCommand(command);
            return;
        }

        BattlePreview preview = _session.BuildPreview(command);
        _report.Show(_session, null, new BattleView(preview, _showThreat));

        bool confirmed;
        if (_useKeys)
        {
            Console.Write("\n  Enter — виконати наказ, будь-яка інша клавіша — скасувати (ки не витрачаються) > ");
            confirmed = Console.ReadKey(true).Key == ConsoleKey.Enter;
        }
        else
        {
            Console.Write("\n  Enter — виконати наказ, будь-що інше — скасувати (ки не витрачаються) > ");
            string? answer = Console.ReadLine();
            confirmed = answer is not null && answer.Trim().Length == 0;
        }

        if (confirmed)
            ExecuteCommand(command);
        else
            _notice = "Наказ скасовано, ки не витрачено.";
    }

    private void ExecuteCommand(BattleCommand command)
    {
        var trace = _settings.ShowChain ? new List<string>() : null;
        string? error = _session.TryIssue(command, trace);

        if (error is not null)
        {
            _notice = error;
            return;
        }

        _lastTrace = trace is null ? null : FormatTrace(command, trace);
    }

    private static string FormatTrace(BattleCommand command, List<string> trace) =>
        $"Ланцюг наказу «{command.Describe()}»:\n {string.Join(" → ", trace)}\n (✔ виконав, · пропустив — не його загін)";

    private void EndPlayerTurn()
    {
        _lastTrace = null;
        _session.EndTurn();
    }

    // ---------- Хід ворога ----------

    private void EnemyTurn()
    {
        _lastTrace = null;
        var orders = new List<string>();

        // Після кожного наказу ворога перемальовуємо карту: видно, куди він ходить
        void Redraw()
        {
            _report.Show(_session, null, BattleView.Empty);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n  ▶ Хід ворога: {_session.Enemy.Name}");
            Console.ResetColor();
            foreach (string line in orders) Console.WriteLine(line);
        }

        Redraw();

        while (_session.Outcome == BattleOutcome.None && _session.KiLeft > 0)
        {
            BattleCommand? command = _ai.Choose(_session, _session.Enemy);
            if (command is null)
            {
                orders.Add("    Ворог тримає позицію.");
                Redraw();
                break;
            }

            var trace = _settings.ShowChain ? new List<string>() : null;

            if (_session.TryIssue(command, trace) is not null)
                break;   // ШІ не повинен помилятись, але цикл не зависне

            orders.Add($"    Наказ ворога: {command.Describe()}");
            if (trace is not null)
                orders.Add("      " + string.Join(" → ", trace));

            Redraw();
            Thread.Sleep(500);
        }

        if (_session.Outcome != BattleOutcome.None)
            return;

        _session.EndTurn();

        if (_session.Outcome == BattleOutcome.None && ReferenceEquals(_session.Active, _session.Player))
            _checkpoints.PushTurnStart(_session.CreateMemento($"Початок ходу {_session.Turn}"));

        ConsoleHelper.Pause(_session.Outcome == BattleOutcome.None
            ? "\n  Enter — ваш хід... "
            : "\n  Enter — підсумок бою... ");
    }

    // ---------- Дзеркало Спогадів (Memento) ----------

    private void SaveCheckpoint()
    {
        int? slot = AskSlot("Зберегти у слот");
        if (slot is null)
        {
            _notice = "Збереження скасовано.";
            return;
        }

        string label = $"Хід {_session.Turn}, ки {_session.KiLeft} " +
                       $"(у строю {_session.Player.ActiveWarriors.Count()}:{_session.Enemy.ActiveWarriors.Count()})";

        _checkpoints.Save(slot.Value, _session.CreateMemento(label));
        _session.Log.Add($"Дзеркало Спогадів запам'ятало мить (слот {slot.Value + 1}).");
        _notice = $"Контрольну точку збережено у слот {slot.Value + 1}.";
    }

    private void LoadCheckpoint()
    {
        int? slot = AskSlot("Завантажити зі слота");
        if (slot is null)
        {
            _notice = "Завантаження скасовано.";
            return;
        }

        BattleMemento? memento = _checkpoints.Load(slot.Value);
        if (memento is null)
        {
            _notice = "Цей слот порожній.";
            return;
        }

        _session.Restore(memento);
        _checkpoints.ResetTimeline(memento);
        _lastTrace = null;
        _notice = $"Час повернуто до «{memento.Label}».";
    }

    private void UndoTurn()
    {
        BattleMemento? memento = _checkpoints.Undo(_session.CommandsUsed > 0);
        if (memento is null)
        {
            _notice = "Дзеркало мовчить: відкатів не лишилось, або повертати нічого.";
            return;
        }

        _session.Restore(memento);
        _lastTrace = null;
        _notice = $"Час повернуто до «{memento.Label}». Відкатів лишилось: {_checkpoints.UndoCharges}.";
    }

    // Одна цифра: у режимі клавіш — без Enter, у текстовому — з Enter
    private char? ReadDigit()
    {
        if (_useKeys)
        {
            char key = Console.ReadKey(true).KeyChar;
            Console.WriteLine();
            return char.IsDigit(key) ? key : null;
        }

        string? line = Console.ReadLine()?.Trim();
        return !string.IsNullOrEmpty(line) && char.IsDigit(line[0]) ? line[0] : null;
    }

    private int? AskSlot(string prompt)
    {
        Console.WriteLine();
        for (int i = 0; i < GameConfig.MaxCheckpoints; i++)
            Console.WriteLine($"    [{i + 1}] {_checkpoints.Slots[i]?.Label ?? "— порожньо —"}");

        Console.Write($"  {prompt} (1–{GameConfig.MaxCheckpoints}, інше — скасувати) > ");

        if (ReadDigit() is { } digit)
        {
            int n = digit - '0';
            if (n >= 1 && n <= GameConfig.MaxCheckpoints)
                return n - 1;
        }

        return null;
    }

    private bool HasRewind() =>
        _checkpoints.Slots.Any(s => s is not null) || _checkpoints.UndoCharges > 0;

    // Після невдачі: останній шанс — Дзеркало Спогадів. true — бій продовжується
    private bool AskRewind()
    {
        Console.Write("\n  Дзеркало Спогадів ще світиться. L + Enter — завантажити точку, U + Enter — відкат ходу, Enter — у головне меню > ");

        switch (InputParser.Parse(Console.ReadLine()).Kind)
        {
            case InputKind.Load:
                LoadCheckpoint();
                break;
            case InputKind.Undo:
                UndoTurn();
                break;
        }

        return _session.Outcome == BattleOutcome.None;
    }

    // ---------- Огляд клану (Bridge з ЛР 2–3) ----------

    private void ShowClanInfo()
    {
        Console.Write("  Який клан показати? 1 — ваш, 2 — ворожий > ");
        Clan clan = ReadDigit() == '2' ? _session.Enemy : _session.Player;

        IClanRenderer renderer = _settings.GraphicMode ? new GraphicClanRenderer() : new TextClanRenderer();
        new ClanCompositionReport(renderer).Print(clan);
        new LeaderReport(renderer).Print(clan);

        ConsoleHelper.Pause();
    }

    // ---------- Фінал ----------

    private void ShowResult()
    {
        _report.Show(_session, null);

        (string title, ConsoleColor color) = _session.Outcome switch
        {
            BattleOutcome.PlayerWon => ("ПЕРЕМОГА! Святилище Інарі відчиняє двері вашому клану.", ConsoleColor.Green),
            BattleOutcome.EnemyWon => ("ПОРАЗКА. Святилище лишається зачиненим для вас.", ConsoleColor.Red),
            _ => ("НІЧИЯ. Світанок застав обидва клани на полі.", ConsoleColor.Yellow)
        };

        Console.ForegroundColor = color;
        Console.WriteLine("\n  ╔══════════════════════════════════════════════════════╗");
        Console.WriteLine("    " + title);
        Console.WriteLine("    " + _session.EndReason);
        Console.WriteLine($"    Ходів: {Math.Min(_session.Turn, GameConfig.MaxTurns)} │ У строю: {_session.Player.ActiveWarriors.Count()} проти {_session.Enemy.ActiveWarriors.Count()} │ Seed: {GameRandom.Seed}");
        Console.WriteLine("  ╚══════════════════════════════════════════════════════╝");
        Console.ResetColor();

        // Акти ночі: що вдалося, що ні
        Console.WriteLine();
        for (int i = 0; i < _session.Acts.Acts.Count; i++)
        {
            Act act = _session.Acts.Acts[i];
            string mark = _session.Acts.StatusOf(i) switch
            {
                ActStatus.Completed => "★",
                ActStatus.Failed => "✗",
                _ => "—"
            };
            Console.WriteLine($"    {mark} {act.Title}: {act.Objective}");
        }

        Console.WriteLine($"\n    Виконано актів: {_session.Acts.Stars} із {_session.Acts.Acts.Count}");

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n  " + Epilogue.For(_session.Outcome, _session.Acts.Stars));
        Console.ResetColor();
    }
}
