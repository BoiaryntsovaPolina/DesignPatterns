namespace ClanBattle;

// Псевдографічний екран бою: кольорова карта, картка бійця, смужки здоров'я загонів, журнал
public class GraphicBattleRenderer : IBattleRenderer
{
    public void Clear() => Palette.ClearScreen();

    public void DrawTitle(string title)
    {
        string line = new string('═', title.Length + 8);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╔" + line + "╗");
        Console.WriteLine("║    " + title + "    ║");
        Console.WriteLine("╚" + line + "╝");
        Console.ResetColor();
    }

    public void DrawStatus(BattleSession session)
    {
        int ki = Math.Max(0, session.KiLeft);
        string kiBar = new string('●', ki) + new string('○', Math.Max(0, session.KiPerTurn - ki));

        Console.Write($" Хід {session.Turn}/{GameConfig.MaxTurns} · {session.Acts.Label}  │  Ходить: ");
        Console.ForegroundColor = ReferenceEquals(session.Active, session.Player) ? ConsoleColor.Cyan : ConsoleColor.Red;
        Console.Write($"{session.Active.Name} ({session.Active.Tag})");
        Console.ResetColor();
        Console.WriteLine($"  │  Ки: {kiBar}");

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(" " + session.Acts.StatusLine(session));
        Console.ResetColor();
    }

    public void DrawField(BattleSession session, BattleView view)
    {
        Palette.PrintGrid(MapOverlay.Build(session, view));

        // Екран тісний, тому легенда в один рядок, а порожніх рядків немає
        Console.ForegroundColor = ConsoleColor.DarkGray;
        string shrineLegend = session.Shrine == ShrineMode.Off ? ""
            : $" · {(session.ShrineVeiled ? "~~ святилище в тумані" : "[] святилище")}";
        Console.WriteLine($" T Танукі · N Некомата · K Кіцуне · * Глава · # щит · мала літера = поранений{shrineLegend}");
        Console.ResetColor();

        // Рядок 3: що означають кольори ЗАРАЗ (кольоровий фон = роль бійця; крапки = зона/шлях)
        if (view.Preview is not null)
        {
            Console.Write(" Прогноз: ");
            Chip("виконавець", ConsoleColor.DarkBlue);
            Chip("куди піде", ConsoleColor.DarkGreen);
            Chip("ціль", ConsoleColor.Red);
            Chip("не зможе", ConsoleColor.DarkRed);
            Chip("немає цілі", ConsoleColor.DarkGray);
            Console.WriteLine(" жовті + — лінія пострілу");
        }
        else if (view.SelectedKey is >= 1 and <= 4)
        {
            int total = session.Player.ActiveWarriors.Count(w => view.SelectedKey == 4 || w.Type.Key() == view.SelectedKey);
            int canHit = session.Player.ActiveWarriors.Count(w =>
                (view.SelectedKey == 4 || w.Type.Key() == view.SelectedKey) && session.Field.FindTarget(w) is not null);

            IWarrior? focus = MapOverlay.FocusOf(session, view);

            Console.Write(" Обрано: ");
            Chip("ваш загін", ConsoleColor.DarkBlue);
            if (focus is not null)
            {
                int onFocus = session.Player.ActiveWarriors.Count(w =>
                    (view.SelectedKey == 4 || w.Type.Key() == view.SelectedKey) && session.Field.CanReach(w, focus.Position));
                Chip("дістає фокус", ConsoleColor.Blue);
                Chip("ворог-фокус", ConsoleColor.Red);
                Console.WriteLine($" фокус {focus.Name}: дістають {onFocus} з {total}, решта б'є найближчого · Пробіл — битися");
            }
            else
            {
                Chip("ворог у зоні", ConsoleColor.Red);
                Console.WriteLine($" до ворога дістають {canHit} з {total} · Пробіл — битися (курсор на ворогові = фокус)");
            }
        }
        else if (view.Cursor is not null)
        {
            Console.Write(" Курсор: ");
            Chip("ворог у радіусі", ConsoleColor.Red);
            Console.WriteLine(" • — радіус удару воїна під курсором. Оберіть загін 1–4 — побачите, кого він дістає");
        }

        // Рядок 4: загроза (клавіша T)
        if (view.ShowThreat)
        {
            List<string> names = session.Field.ThreatenedWarriors(session.Player).Select(w => w.Name).ToList();
            Console.Write(" Загроза: ");
            Chip("ворог, що дістає вас", ConsoleColor.DarkMagenta);
            Chip("ваш боєць під ударом", ConsoleColor.DarkYellow);
            Console.WriteLine(names.Count > 0 ? $" ({names.Count})" : " нікого");
        }
    }

    // Кольорова «плашка» легенди: той самий фон, що й на карті
    private static void Chip(string text, ConsoleColor back)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = back;
        Console.Write($" {text} ");
        Console.ResetColor();
        Console.Write(" ");
    }

    public void DrawCard(BattleSession session, BattleView view)
    {
        if (view.Cursor is not { } cursor) return;

        List<string> lines = UnitCard.Lines(session, cursor);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" ▸ " + lines[0]);
        Console.ResetColor();

        for (int i = 1; i < lines.Count; i++)
            Console.WriteLine("   " + lines[i]);
    }

    public void DrawPreview(BattlePreview preview)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($" ── Прогноз наказу «{preview.Command.Describe()}» (ще НЕ виконано) ──");
        Console.ResetColor();
        Console.WriteLine("  " + preview.Summary());

        List<PreviewEntry> lines = preview.DetailLines().ToList();
        foreach (PreviewEntry entry in lines.Take(12))
            Console.WriteLine("   • " + entry.Text);

        if (lines.Count > 12)
            Console.WriteLine($"   … і ще {lines.Count - 12} (їх видно на карті)");
    }

    // Панель клану: 3 рядки (назва + Глава, ярість + благословення, загони) — щоб екран був нижчим
    public void DrawClanPanel(BattleSession session, Clan clan)
    {
        bool mine = ReferenceEquals(clan, session.Player);
        ConsoleColor tone = mine ? ConsoleColor.Cyan : ConsoleColor.Red;
        int alive = clan.ActiveWarriors.Count();
        int total = clan.AllWarriors.Count();

        IWarrior? leader = clan.Leader;
        string leaderText = leader is null ? "немає" : $"{leader.Name} — {LeaderProfile.For(leader).Title}";

        string arrow = clan.Facing > 0 ? "→" : "←";
        Console.ForegroundColor = tone;
        Console.Write($" ▌ Клан {clan.Tag} {arrow} «{clan.Name}» — у строю {alive}/{total}");
        Console.ResetColor();
        Console.WriteLine($" │ Глава: {leaderText}");

        int rageMax = GameConfig.RageToUseAbility;
        string rage = new string('■', Math.Min(clan.Rage, rageMax)) + new string('□', Math.Max(0, rageMax - clan.Rage));
        string ready = clan.Rage >= rageMax ? " ◄ ЗДІБНІСТЬ ГОТОВА" : "";

        // Очки Благословення є лише в режимі «змагання»
        string blessing = "";
        if (session.Shrine == ShrineMode.Contest)
        {
            int need = session.ShrineGoal;
            blessing = $" │ Благосл. {clan.Blessing}/{need}";
        }

        // Другий рядок: ярість, благословення й загони (екран тісний, тому все в одному рядку)
        Console.Write($"    Ярість {rage} {clan.Rage}/{rageMax}{ready}{blessing} │ ");
        foreach (Squad squad in clan.Squads.OrderBy(sq => sq.Type))
        {
            string bar = string.Concat(squad.Members.Select(m => !m.InBattle ? '░' : m.State.CanAdvance ? '█' : '▒'));
            Console.Write($"[{squad.Type.Key()}] ");
            Console.ForegroundColor = tone;
            Console.Write(bar);
            Console.ResetColor();
            Console.Write($" {squad.Active.Count()}/{squad.Members.Count}  ");
        }
        Console.WriteLine();
    }

    public void DrawLog(IReadOnlyList<string> lines)
    {
        Console.WriteLine(" ── Журнал бою ─────────────────────────────────");
        foreach (string line in lines)
        {
            // Рядки клану А — блакитні, клану Б — червоні, сюжет — фіолетові, решта (події) — сірі
            Console.ForegroundColor = line.StartsWith("~ ") ? ConsoleColor.Magenta
                                    : line.StartsWith("[А]") ? ConsoleColor.Cyan
                                    : line.StartsWith("[Б]") ? ConsoleColor.Red
                                    : ConsoleColor.Gray;
            Console.WriteLine("  " + line);
            Console.ResetColor();
        }
    }

    public void DrawNote(string text)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(" " + text);
        Console.ResetColor();
    }
}
