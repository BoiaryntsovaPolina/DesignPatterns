namespace ClanBattle;

// Простий текст: без кольорів і карти, кожен воїн — з координатами
public class TextBattleRenderer : IBattleRenderer
{
    public void Clear() => Palette.ClearScreen();

    public void DrawTitle(string title) => Console.WriteLine("=== " + title + " ===");

    public void DrawStatus(BattleSession session)
    {
        Console.WriteLine($"Хід {session.Turn}/{GameConfig.MaxTurns} ({session.Acts.Label}{(session.ShrineVeiled ? ", святилище в тумані" : "")}). " +
                          $"Ходить: {session.Active.Name} ({session.Active.Tag}). Ки: {session.KiLeft}");
        Console.WriteLine(session.Acts.StatusLine(session));
    }

    public void DrawField(BattleSession session, BattleView view)
    {
        Console.WriteLine($"\nПоле {session.Field.Width}x{session.Field.Height}, координати (X; Y). Клан А іде вправо, клан Б — вліво.");
        string shrine = session.Shrine == ShrineMode.Off
            ? ""
            : $" Святилище Інарі: X {session.Field.Width / 2 - 1}–{session.Field.Width / 2}, усі ряди ({ShrineModeNames.Rule(session)}).";
        Console.WriteLine("* — Глава, ~ — поранений." + shrine);

        foreach (Clan clan in session.Field.Clans)
        {
            Console.WriteLine($"\nКлан {clan.Tag} «{clan.Name}»:");
            IWarrior? leader = clan.Leader;

            foreach (Squad squad in clan.Squads.OrderBy(s => s.Type))
            {
                string members = string.Join(" ", squad.Members.Select(m => Token(m, ReferenceEquals(m, leader))));
                Console.WriteLine($"  [{squad.Type.Key()}] {squad.Type.ToUa(),-9} {squad.Active.Count()}/{squad.Members.Count}: {members}");
            }
        }

        if (view.ShowThreat)
        {
            List<string> names = session.Field.ThreatenedWarriors(session.Player).Select(w => w.Name).ToList();
            Console.WriteLine("\nЗагроза: " + (names.Count > 0 ? "ворог дістає до " + string.Join(", ", names) : "нікого не досягають"));
        }
    }

    public void DrawCard(BattleSession session, BattleView view)
    {
        if (view.Cursor is not { } cursor) return;

        Console.WriteLine();
        foreach (string line in UnitCard.Lines(session, cursor))
            Console.WriteLine(line);
    }

    public void DrawPreview(BattlePreview preview)
    {
        Console.WriteLine($"\n--- Прогноз наказу «{preview.Command.Describe()}» (ще НЕ виконано) ---");
        Console.WriteLine(preview.Summary());

        foreach (PreviewEntry entry in preview.DetailLines())
            Console.WriteLine(" - " + entry.Text);
    }

    public void DrawClanPanel(BattleSession session, Clan clan)
    {
        IWarrior? leader = clan.Leader;
        Console.WriteLine($"Клан {clan.Tag}: у строю {clan.ActiveWarriors.Count()}/{clan.AllWarriors.Count()}, " +
                          $"Глава: {(leader?.Name ?? "немає")}, ярість: {clan.Rage}/{GameConfig.RageToUseAbility}" +
                          (session.Shrine == ShrineMode.Contest ? ", " : "") +
                          (session.Shrine == ShrineMode.Contest ? $"благословення: {clan.Blessing}/{session.ShrineGoal}" : ""));
    }

    public void DrawLog(IReadOnlyList<string> lines)
    {
        Console.WriteLine("\n--- Журнал бою ---");
        foreach (string line in lines)
            Console.WriteLine(line);
    }

    public void DrawNote(string text) => Console.WriteLine("\n" + text);

    private static string Token(IWarrior warrior, bool isLeader)
    {
        if (!warrior.InBattle)
            return $"{warrior.Name}[вибув]";

        string leader = isLeader ? "*" : "";
        string wounded = warrior.State.CanAdvance ? "" : "~";
        return $"{leader}{warrior.Name}{wounded}({warrior.Position.X};{warrior.Position.Y})";
    }
}
