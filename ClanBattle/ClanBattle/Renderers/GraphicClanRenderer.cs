namespace ClanBattle;

// Псевдографіка огляду клану (ЛР 2–3): гравці накопичуються в сітці, карта друкується в EndClan
public class GraphicClanRenderer : IClanRenderer
{
    private MapCell[,] _grid = new MapCell[0, 0];
    private int _facing = 1;

    public void BeginClan(Clan clan)
    {
        Console.WriteLine($"\n===== КАРТА КЛАНУ: «{clan.Name}» (клан {clan.Tag}) =====");

        _facing = clan.Facing;
        _grid = new MapCell[GameConfig.FieldHeight, GameConfig.FieldWidth];
        for (int y = 0; y < GameConfig.FieldHeight; y++)
            for (int x = 0; x < GameConfig.FieldWidth; x++)
                _grid[y, x] = new MapCell(". ", ConsoleColor.DarkGray);
    }

    // Нічого не друкуємо: загони видно як колонки на карті
    public void BeginSquad(Squad squad) { }

    public void DrawWarrior(IWarrior warrior, bool isLeader)
    {
        if (!warrior.InBattle) return;

        _grid[warrior.Position.Y, warrior.Position.X] =
            new MapCell(Palette.Glyph(warrior, isLeader, shielded: false, _facing), Palette.ToConsole(warrior.Traits.Color));
    }

    public void EndClan()
    {
        Palette.PrintGrid(_grid);
        Console.WriteLine("T — Танукі, N — Некомата, K — Кіцуне, * — Глава, мала літера — поранений.");
        Console.WriteLine("Колір літери = колір гравця (Decorator). Кожен загін — окрема колонка.\n");
    }

    // Картка Глави в рамці (характеристики беремо з даних, а не з довгого GetInfo)
    public void DrawLeader(LeaderProfile profile)
    {
        IWarrior w = profile.Warrior;
        WarriorTraits t = w.Traits;

        string traits = string.Join(", ", new[]
        {
            t.Color is { } c ? $"колір: {c.ToUa()}" : null,
            t.Height is { } h ? $"зріст: {h} см" : null,
            t.Clothing is { } cl ? $"одяг: {cl.ToUa()}" : null,
        }.Where(x => x is not null));

        List<string> lines = new()
        {
            $"Титул:          {profile.Title}",
            $"Носій:          {w.Name}, позиція {w.Position}, стан: {w.State.Name}",
            $"Зброя:          {w.Weapon}",
            $"Пересування:    {w.MovementType}",
            $"Особливості:    {(traits.Length > 0 ? traits : "немає")}",
            $"Здібність:      {profile.Ability}",
        };

        int width = lines.Max(l => l.Length) + 1;

        Console.WriteLine("┌─ ГЛАВА КЛАНУ " + new string('─', Math.Max(0, width - 13)) + "┐");
        foreach (string line in lines)
            Console.WriteLine("│ " + line.PadRight(width - 1) + "│");
        Console.WriteLine("└" + new string('─', width) + "┘");
    }

    public void DrawNote(string text) => Console.WriteLine(text);
}
