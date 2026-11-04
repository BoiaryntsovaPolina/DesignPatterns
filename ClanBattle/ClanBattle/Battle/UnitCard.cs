namespace ClanBattle;

// Текст картки бійця під курсором: тип, стан, радіус, міфологічна пасивка, особливості
public static class UnitCard
{
    public static string Passive(SquadType type) => type switch
    {
        SquadType.Tanuki => "Ілюзія: чверть ударів влучає в двійника з диму",
        SquadType.Nekomata => "Дев'ять життів: раз за бій переживає «вибуття»",
        SquadType.Kitsune => "Лисячий вогонь: +10% по цілях на святилищі Інарі",
        _ => ""
    };

    public static List<string> Lines(BattleSession session, Position cell)
    {
        var lines = new List<string>();
        IWarrior? warrior = session.Field.WarriorAt(cell);

        if (warrior is null)
        {
            string shrine = "святилище Інарі: " + ShrineModeNames.Rule(session);
            lines.Add($"Клітина {cell}: " + (session.Field.IsShrine(cell) ? shrine : "порожньо"));
            return lines;
        }

        Clan clan = session.Field.ClanOf(warrior);
        bool isLeader = ReferenceEquals(clan.Leader, warrior);

        lines.Add($"{warrior.Name} ({warrior.Type.ToUa()}), клан {clan.Tag}{(isLeader ? ", ГЛАВА" : "")}: {warrior.State.Name}, позиція {warrior.Position}");

        int chance = (int)Math.Round(CombatRules.BaseHitChance(warrior.Type) * 100);
        string lives = warrior.Type == SquadType.Nekomata && session.Field.NineLivesUsed.Contains(warrior)
            ? " (вже використано)"
            : "";
        lines.Add($"Радіус удару {CombatRules.RangeOf(warrior.Type)} кл. · базовий шанс {chance}% · {Passive(warrior.Type)}{lives}");

        WarriorTraits t = warrior.Traits;
        string traits = string.Join(", ", new[]
        {
            t.Color is { } c ? $"колір: {c.ToUa()}" : null,
            t.Height is { } h ? $"зріст: {h} см" : null,
            t.Clothing is { } cl ? $"одяг: {cl.ToUa()}" : null,
        }.Where(x => x is not null));
        lines.Add("Особливості: " + (traits.Length > 0 ? traits : "немає") + $" · зброя: {warrior.Weapon}");

        // Для ворога: скільки наших дістає до нього й підказка про фокус
        if (!ReferenceEquals(clan, session.Player))
        {
            int reach = session.Player.ActiveWarriors.Count(w => session.Field.CanReach(w, cell));
            lines.Add($"Дістають ваші бійці: {reach} з {session.Player.ActiveWarriors.Count()} · " +
                      "оберіть загін і Пробіл — фокус на цій цілі");
        }

        return lines;
    }
}
