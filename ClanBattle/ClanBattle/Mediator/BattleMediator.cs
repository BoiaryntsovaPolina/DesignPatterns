namespace ClanBattle;

// Колеги медіатора: Battlefield (карта), Clan-и, BattleLog, воїни.
// Він же застосовує правила: зіткнення, межі, влучання, комбо, стани, пасивки, падіння Глави
public class BattleMediator : IBattleMediator
{
    private readonly Battlefield _field;
    private readonly BattleLog _log;

    // Лічильники одного наказу: щоб у журнал потрапив один зрозумілий підсумок, а не 7 однакових рядків
    private int _moved, _swapped, _blocked, _refused, _healed, _shielded, _shots, _hits, _noTarget;
    private int _combo, _bestCombo;     // поточне й найкраще комбо цього наказу
    private BattleCommand? _current;    // наказ, що виконується просто зараз

    public BattleMediator(Battlefield field, BattleLog log)
    {
        _field = field;
        _log = log;
    }

    public void BeginCommand(BattleCommand command)
    {
        _current = command;
        _moved = _swapped = _blocked = _refused = _healed = _shielded = _shots = _hits = _noTarget = 0;
        _combo = _bestCombo = 0;
    }

    public void EndCommand(Clan clan, BattleCommand command)
    {
        var parts = new List<string>();
        if (_moved > 0) parts.Add($"зрушили: {_moved}");
        if (_swapped > 0) parts.Add($"помінялись місцями зі своїми: {_swapped}");
        if (_blocked > 0) parts.Add($"шлях закрито: {_blocked}");
        if (_refused > 0) parts.Add($"поранені не наступають: {_refused}");
        if (_healed > 0) parts.Add($"одужали: {_healed}");
        if (_shielded > 0) parts.Add($"під щитом: {_shielded}");
        if (_shots > 0) parts.Add($"пострілів: {_shots}, влучань: {_hits}");
        if (_bestCombo >= 2) parts.Add($"найкраще комбо ×{_bestCombo}");
        if (_noTarget > 0) parts.Add($"без цілі: {_noTarget}");

        string summary = parts.Count > 0 ? string.Join(", ", parts) : "ніхто не відгукнувся";
        _log.Add($"[{clan.Tag}] {command.Describe()}: {summary}");
    }

    // ---------- Вперед ----------
    public void RequestAdvance(IWarrior warrior)
    {
        if (!warrior.InBattle) return;

        if (!warrior.State.CanAdvance)   // State вирішує, чи можна наступати
        {
            _refused++;
            return;
        }

        if (TryStep(warrior, forward: true)) _moved++; else _blocked++;
    }

    // ---------- Назад ----------
    public void RequestRetreat(IWarrior warrior)
    {
        if (!warrior.InBattle) return;

        // Відступ лікує пораненого (навіть якщо позаду немає місця)
        IWarriorState before = warrior.State;
        warrior.State = warrior.State.OnRetreat();
        if (!ReferenceEquals(before, warrior.State)) _healed++;

        if (TryStep(warrior, forward: false)) _moved++; else _blocked++;
    }

    // ---------- Битися ----------
    public void RequestAttack(IWarrior warrior)
    {
        if (!warrior.InBattle) return;

        IWarrior? target = _field.FindTarget(warrior, _current?.Focus);   // фокус із наказу або найближчий
        if (target is null)
        {
            _noTarget++;
            return;
        }

        _shots++;
        Clan attackerClan = _field.ClanOf(warrior);
        Clan victimClan = _field.ClanOf(target);
        string attacker = Who(warrior);
        string victim = Who(target);

        if (_field.Shielded.Contains(target))
        {
            _log.Add($"{attacker} → {victim}: «Кам'яна Стіна» відбила удар.");
            _combo = 0;
            return;
        }

        // Комбо: кожен влучний удар підряд у цьому наказі підсилює наступний — тому ПОРЯДОК у ланцюгу важливий
        if (GameRandom.NextDouble() >= CombatRules.HitChance(warrior, target, _field, _combo))
        {
            _log.Add($"{attacker} → {victim}: промах.");
            _combo = 0;
            return;
        }

        // Пасивка Танукі «Ілюзія»: удар влучає в двійника з диму
        if (target.Type == SquadType.Tanuki && GameRandom.NextDouble() < CombatRules.IllusionChance)
        {
            _log.Add($"{attacker} → {victim}: влучив в ілюзію — двійник розтанув димом, Танукі цілий.");
            _combo = 0;
            return;
        }

        _hits++;
        _combo++;
        _bestCombo = Math.Max(_bestCombo, _combo);

        IWarriorState next = target.State.OnHit();
        bool nineLives = false;

        // Пасивка Некомата «Дев'ять життів»: один раз за бій переживає "вибуття"
        if (!next.InBattle && target.Type == SquadType.Nekomata && _field.NineLivesUsed.Add(target))
        {
            next = target.State;
            nineLives = true;
        }

        target.State = next;

        // Ярість Глави росте від влучань (комбо — швидше) і від отриманих ударів
        attackerClan.AddRage(_combo >= 3 ? 2 : 1);
        victimClan.AddRage(1);

        string combo = _combo >= 2 ? $" КОМБО ×{_combo}!" : "";

        if (nineLives)
        {
            _log.Add($"{attacker} → {victim}: влучання!{combo} Але «Дев'ять життів»: {victim} ще тримається.");
        }
        else if (target.InBattle)
        {
            _log.Add($"{attacker} → {victim}: влучання!{combo} Ціль поранена.");
        }
        else
        {
            _log.Add($"{attacker} → {victim}: влучання!{combo} {victim} вибуває з бою.");
            OnWarriorDown(victimClan, target);
        }
    }

    // ---------- Здібність Глави ----------
    public void RequestLeaderAbility(IWarrior warrior)
    {
        if (!warrior.InBattle) return;

        IWarrior? leader = _field.ClanOf(warrior).Leader;
        if (leader is null) return;

        switch (leader.Type)
        {
            case SquadType.Kitsune:      // «Покровительство Хвостів»: лікує весь клан
                IWarriorState before = warrior.State;
                warrior.State = warrior.State.OnRetreat();
                if (!ReferenceEquals(before, warrior.State)) _healed++;
                break;

            case SquadType.Nekomata:    // «Тіньовий Стрибок»: загін Некомата швидко наступає
                if (warrior.Type == SquadType.Nekomata)
                {
                    RequestAdvance(warrior);
                    RequestAdvance(warrior);
                }
                break;

            case SquadType.Tanuki:      // «Кам'яна Стіна»: загін Танукі невразливий до наступного обстрілу
                if (warrior.Type == SquadType.Tanuki && _field.Shielded.Add(warrior))
                    _shielded++;
                break;
        }
    }

    // ---------- Допоміжне ----------
    private bool TryStep(IWarrior warrior, bool forward)
    {
        Clan clan = _field.ClanOf(warrior);
        int dx = clan.Facing * (forward ? 1 : -1);
        var target = new Position(warrior.Position.X + dx, warrior.Position.Y);

        if (!_field.InBounds(target))
            return false;

        IWarrior? other = _field.WarriorAt(target);

        if (other is null)
        {
            warrior.Position = target;
            return true;
        }

        // Свій з іншого загону (він не отримав цього наказу) — міняємось місцями.
        // Так Кіцуне можна висунути вперед, а наказ усім загонам місцями не міняє нікого
        if (ReferenceEquals(_field.ClanOf(other), clan) && _current is not null && !_current.AppliesTo(other))
        {
            other.Position = warrior.Position;
            warrior.Position = target;
            _swapped++;
            return true;
        }

        return false;
    }

    // Якщо вибула Глава — обираємо нову, клан у сум'ятті втрачає 1 ки (Singleton ClanLeader)
    private void OnWarriorDown(Clan clan, IWarrior fallen)
    {
        if (!ReferenceEquals(clan.Leader, fallen)) return;

        if (clan.ChooseLeader())
        {
            clan.KiPenalty = 1;
            _log.Add($"Глава клану «{clan.Name}» впала! Нова Глава — {Who(clan.Leader!)}. Клан у сум'ятті: −1 ки на наступному ході.");
        }
        else
        {
            _log.Add($"Глава клану «{clan.Name}» впала, а обирати вже нікого.");
        }
    }

    private string Who(IWarrior warrior) => $"[{_field.ClanOf(warrior).Tag}] {warrior.Name}";
}
