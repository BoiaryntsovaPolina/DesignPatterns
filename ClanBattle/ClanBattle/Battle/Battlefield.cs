namespace ClanBattle;

// Поле бою: знає, хто де стоїть. Рішень не приймає — це справа медіатора
public class Battlefield
{
    private readonly List<Clan> _clans;

    public Battlefield(params Clan[] clans) => _clans = clans.ToList();

    public IReadOnlyList<Clan> Clans => _clans;
    public int Width => GameConfig.FieldWidth;
    public int Height => GameConfig.FieldHeight;

    // Воїни під "Кам'яною Стіною" (здібність Глави танукі): наступний обстріл по них не проходить
    public HashSet<IWarrior> Shielded { get; } = new();

    // Некомата, які вже використали «Дев'ять життів»
    public HashSet<IWarrior> NineLivesUsed { get; } = new();

    // Режим святилища (з налаштувань)
    public ShrineMode Shrine { get; set; } = ShrineMode.Contest;

    // Чи святилище в тумані (Акт II): видно на карті, але нічого не дає. Відповідь дає сесія (за актами)
    public Func<bool> VeilCheck { get; set; } = () => false;
    public bool IsVeiled => Shrine != ShrineMode.Off && VeilCheck();

    // Геометрія святилища: два центральні стовпці на всю висоту (для малювання). У режимі «вимкнене» його немає
    public bool IsShrine(Position p) =>
        Shrine != ShrineMode.Off && p.X >= Width / 2 - 1 && p.X <= Width / 2 && p.Y >= 0 && p.Y < Height;

    // Чи діє святилище в цій клітині просто зараз (лікування, очки, бонус Кіцуне)
    public bool ShrineWorks(Position p) => IsShrine(p) && !IsVeiled;

    public bool InBounds(Position p) => p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;

    public IWarrior? WarriorAt(Position p) =>
        _clans.SelectMany(c => c.ActiveWarriors).FirstOrDefault(w => w.Position == p);

    public Clan ClanOf(IWarrior warrior) =>
        _clans.First(c => c.AllWarriors.Contains(warrior));

    public Clan EnemyOf(Clan clan) =>
        _clans.First(c => !ReferenceEquals(c, clan));

    // Відстань між клітинами: крок по діагоналі теж рахується за 1 (квадратний радіус)
    public static int Distance(Position a, Position b) =>
        Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

    // Чи дістає воїн до клітини: у квадраті його радіуса, але не на власній клітині
    public bool CanReach(IWarrior warrior, Position cell)
    {
        int distance = Distance(warrior.Position, cell);
        return distance > 0 && distance <= CombatRules.RangeOf(warrior.Type);
    }

    // Ціль для пострілу. Якщо гравець вказав фокус (курсор на ворогові) і воїн до нього дістає — б'є його.
    // Інакше найближчий ворог у квадраті навколо воїна (будь-який бік);
    // при однаковій відстані перевага тому, хто попереду, потім тому, хто в ближчому ряду
    public IWarrior? FindTarget(IWarrior shooter, Position? focus = null)
    {
        Clan own = ClanOf(shooter);
        int range = CombatRules.RangeOf(shooter.Type);

        if (focus is { } cell && CanReach(shooter, cell))
        {
            IWarrior? focused = EnemyOf(own).ActiveWarriors.FirstOrDefault(e => e.Position == cell);
            if (focused is not null) return focused;
        }

        IWarrior? best = null;
        (int, int, int, int) bestKey = default;

        foreach (IWarrior enemy in EnemyOf(own).ActiveWarriors)
        {
            int distance = Distance(shooter.Position, enemy.Position);
            if (distance == 0 || distance > range) continue;

            int dx = enemy.Position.X - shooter.Position.X;
            int dy = enemy.Position.Y - shooter.Position.Y;
            int behind = dx * own.Facing < 0 ? 1 : 0;

            (int, int, int, int) key = (distance, behind, Math.Abs(dy), enemy.Position.Y);
            if (best is null || key.CompareTo(bestKey) < 0)
            {
                best = enemy;
                bestKey = key;
            }
        }

        return best;
    }

    // Клітини, які воїн може вразити: квадрат навколо нього радіусом дальності
    public HashSet<Position> RangeCells(IWarrior warrior)
    {
        int range = CombatRules.RangeOf(warrior.Type);
        var cells = new HashSet<Position>();

        for (int dx = -range; dx <= range; dx++)
            for (int dy = -range; dy <= range; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                var cell = new Position(warrior.Position.X + dx, warrior.Position.Y + dy);
                if (InBounds(cell))
                    cells.Add(cell);
            }

        return cells;
    }

    // Усі клітини, які клан зараз "простріляє"
    public HashSet<Position> ThreatCells(Clan clan)
    {
        var cells = new HashSet<Position>();
        foreach (IWarrior warrior in clan.ActiveWarriors)
            cells.UnionWith(RangeCells(warrior));
        return cells;
    }

    // Мої воїни, до яких ворог дістає просто зараз
    public IEnumerable<IWarrior> ThreatenedWarriors(Clan me)
    {
        HashSet<Position> threat = ThreatCells(EnemyOf(me));
        return me.ActiveWarriors.Where(w => threat.Contains(w.Position));
    }

    // Вороги, які просто зараз можуть уразити когось із моїх
    public IEnumerable<IWarrior> ThreateningWarriors(Clan me)
    {
        HashSet<Position> mine = me.ActiveWarriors.Select(w => w.Position).ToHashSet();
        return EnemyOf(me).ActiveWarriors.Where(e => RangeCells(e).Overlaps(mine));
    }

    // Відстань до найближчого ворога (null — ворогів не лишилось)
    public int? NearestEnemyDistance(IWarrior warrior)
    {
        Clan own = ClanOf(warrior);
        List<int> distances = EnemyOf(own).ActiveWarriors
            .Select(e => Distance(warrior.Position, e.Position))
            .ToList();

        return distances.Count == 0 ? null : distances.Min();
    }
}
