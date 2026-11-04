namespace ClanBattle;

// Другий Mediator під тим самим інтерфейсом IBattleMediator.
// Ланцюг і команди ті самі, але замість зміни поля він лише записує НАМІРИ.
// Віртуальні позиції потрібні, щоб задні бачили клітини, які звільнили передні
public class PreviewMediator : IBattleMediator
{
    private readonly Battlefield _field;
    private readonly Dictionary<IWarrior, Position> _virtual = new();
    private readonly List<PreviewEntry> _entries = new();

    private readonly BattleCommand _command;

    public PreviewMediator(Battlefield field, BattleCommand command)
    {
        _field = field;
        _command = command;
    }

    public IReadOnlyList<PreviewEntry> Entries => _entries;

    // ---------- Вперед ----------
    public void RequestAdvance(IWarrior warrior)
    {
        if (!warrior.InBattle) return;

        if (!warrior.State.CanAdvance)
        {
            Add(new PreviewEntry(warrior, PreviewKind.Refused,
                $"{warrior.Name} поранений — наступати не може (допоможе «назад»)"));
            return;
        }

        if (TryVirtualStep(warrior, true, out Position from, out Position to, out string? reason, out IWarrior? swapped))
            Add(new PreviewEntry(warrior, PreviewKind.Move, $"{warrior.Name} → {to}: {How(swapped)}{ShrineNote(to)}", from, to));
        else
            Add(new PreviewEntry(warrior, PreviewKind.Blocked, $"{warrior.Name} не піде: {reason}"));
    }

    // ---------- Назад ----------
    public void RequestRetreat(IWarrior warrior)
    {
        if (!warrior.InBattle) return;

        bool heals = !ReferenceEquals(warrior.State.OnRetreat(), warrior.State);
        string healText = heals ? ", одужає" : "";

        if (TryVirtualStep(warrior, false, out Position from, out Position to, out string? reason, out IWarrior? swapped))
            Add(new PreviewEntry(warrior, PreviewKind.Move, $"{warrior.Name} → {to}: {How(swapped)}{healText}{ShrineNote(to)}", from, to));
        else if (heals)
            Add(new PreviewEntry(warrior, PreviewKind.Heal, $"{warrior.Name} не відступить ({reason}), але одужає"));
        else
            Add(new PreviewEntry(warrior, PreviewKind.Blocked, $"{warrior.Name} не піде: {reason}"));
    }

    // ---------- Битися ----------
    public void RequestAttack(IWarrior warrior)
    {
        if (!warrior.InBattle) return;

        IWarrior? target = _field.FindTarget(warrior, _command.Focus);

        if (target is null)
        {
            Add(new PreviewEntry(warrior, PreviewKind.NoTarget, $"{warrior.Name}: цілі немає — {NoTargetReason(warrior)}"));
            return;
        }

        int distance = Battlefield.Distance(warrior.Position, target.Position);
        int percent = (int)Math.Round(CombatRules.HitChance(warrior, target, _field) * 100);
        string shield = _field.Shielded.Contains(target) ? " (але «Кам'яна Стіна» відіб'є удар)" : "";

        // Пасивки цілі з фольклору — гравець бачить їх ще до пострілу
        string passive = target.Type switch
        {
            SquadType.Tanuki => ", може виявитись ілюзією",
            SquadType.Nekomata when !_field.NineLivesUsed.Contains(target) => ", має «Дев'ять життів»",
            _ => ""
        };

        string focus = _command.Focus == target.Position ? " [фокус]" : "";

        Add(new PreviewEntry(warrior, PreviewKind.Shoot,
            $"{warrior.Name} → {Who(target)}{focus}, відстань {distance}, шанс влучання {percent}% (+комбо){passive}{shield}",
            Target: target));
    }

    // ---------- Здібність Глави ----------
    public void RequestLeaderAbility(IWarrior warrior)
    {
        if (!warrior.InBattle) return;

        IWarrior? leader = _field.ClanOf(warrior).Leader;
        if (leader is null) return;

        switch (leader.Type)
        {
            case SquadType.Kitsune:
                if (!ReferenceEquals(warrior.State.OnRetreat(), warrior.State))
                    Add(new PreviewEntry(warrior, PreviewKind.Heal, $"{warrior.Name} одужає («Покровительство Хвостів»)"));
                break;

            case SquadType.Nekomata:
                if (warrior.Type != SquadType.Nekomata) break;

                if (!warrior.State.CanAdvance)
                {
                    Add(new PreviewEntry(warrior, PreviewKind.Refused, $"{warrior.Name} поранений — стрибнути не може"));
                    break;
                }

                Position start = Pos(warrior);
                int steps = 0;
                string? why = null;
                for (int i = 0; i < 2; i++)
                {
                    if (!TryVirtualStep(warrior, true, out _, out _, out string? r, out _))
                    {
                        why = r;
                        break;
                    }
                    steps++;
                }

                if (steps > 0)
                    Add(new PreviewEntry(warrior, PreviewKind.Move, $"{warrior.Name} → {Pos(warrior)}: кроків {steps}", start, Pos(warrior)));
                else
                    Add(new PreviewEntry(warrior, PreviewKind.Blocked, $"{warrior.Name} не піде: {why}"));
                break;

            case SquadType.Tanuki:
                if (warrior.Type == SquadType.Tanuki)
                    Add(new PreviewEntry(warrior, PreviewKind.Shield, $"{warrior.Name} отримає «Кам'яну Стіну» до свого наступного ходу"));
                break;
        }
    }

    // ---------- Допоміжне ----------
    private void Add(PreviewEntry entry) => _entries.Add(entry);

    private Position Pos(IWarrior warrior) =>
        _virtual.TryGetValue(warrior, out Position p) ? p : warrior.Position;

    private IWarrior? OccupantAt(Position cell) =>
        _field.Clans.SelectMany(c => c.ActiveWarriors).FirstOrDefault(w => Pos(w) == cell);

    // Той самий крок, що й у BattleMediator, але на віртуальних позиціях
    private bool TryVirtualStep(IWarrior warrior, bool forward, out Position from, out Position to,
                                out string? reason, out IWarrior? swappedWith)
    {
        Clan clan = _field.ClanOf(warrior);
        from = Pos(warrior);
        to = new Position(from.X + clan.Facing * (forward ? 1 : -1), from.Y);
        reason = null;
        swappedWith = null;

        if (!_field.InBounds(to))
        {
            reason = "край поля";
            return false;
        }

        IWarrior? other = OccupantAt(to);
        if (other is not null)
        {
            bool ally = ReferenceEquals(_field.ClanOf(other), clan);

            if (ally && !_command.AppliesTo(other))
            {
                _virtual[other] = from;
                _virtual[warrior] = to;
                swappedWith = other;
                return true;
            }

            reason = ally
                ? $"на клітині {to} стоїть свій {Who(other)} (наказ лише цьому загону поміняє вас місцями)"
                : $"клітину {to} займає ворог {Who(other)}";
            return false;
        }

        _virtual[warrior] = to;
        return true;
    }

    private string How(IWarrior? swapped) =>
        swapped is null ? "вільно" : $"міняється місцями з {Who(swapped)}";

    private string NoTargetReason(IWarrior warrior)
    {
        int range = CombatRules.RangeOf(warrior.Type);
        int? nearest = _field.NearestEnemyDistance(warrior);

        if (nearest is null)
            return "ворогів на полі не лишилось";

        return $"найближчий ворог за {nearest.Value} кл., радіус удару {range}: треба підійти ще на {nearest.Value - range}";
    }

    private string ShrineNote(Position cell) => _field.IsShrine(cell) ? " — святилище Інарі!" : "";

    private string Who(IWarrior warrior) => $"[{_field.ClanOf(warrior).Tag}] {warrior.Name}";
}
