namespace ClanBattle;

public enum BattleOutcome { None, PlayerWon, EnemyWon, Draw }

// Поточна партія: ходи, ки, накази. Це ж Originator патерну Memento
public class BattleSession
{
    public Battlefield Field { get; }
    public Clan Player { get; }
    public Clan Enemy { get; }
    public BattleLog Log { get; } = new();
    public ActTracker Acts { get; }
    public IWarrior? EnemyFirstLeader { get; }   // для мети Акту II
    public Difficulty Difficulty { get; }
    public BattleMediator Mediator { get; }

    private readonly Narrator _narrator = new();

    public int Turn { get; private set; } = 1;
    public Clan Active { get; private set; }
    public int KiLeft { get; private set; }
    public int CommandsUsed { get; private set; }

    public BattleOutcome Outcome { get; private set; } = BattleOutcome.None;
    public string EndReason { get; private set; } = "";

    // Скільки очок Благословення потрібно Інарі, щоб обрати клан (залежить від складності)
    public int ShrineGoal { get; }

    // Режим святилища (налаштування) і наказів за хід
    public ShrineMode Shrine => Field.Shrine;
    public int KiPerTurn { get; }

    // Мла (Акт II): святилище в тумані й нічого не дає
    public bool ShrineVeiled => Field.IsVeiled;

    public BattleSession(Clan player, Clan enemy, Difficulty difficulty = Difficulty.Easy,
        ShrineMode shrine = ShrineMode.Contest, int kiPerTurn = 2, IWarrior? enemyFirstLeader = null)
    {
        Difficulty = difficulty;
        ShrineGoal = difficulty == Difficulty.Hard ? GameConfig.ShrineGoalHard : GameConfig.ShrineGoalEasy;
        KiPerTurn = Math.Clamp(kiPerTurn, 1, 2);
        Player = player;
        Enemy = enemy;
        Acts = new ActTracker(ActPlan.For(shrine));
        Field = new Battlefield(player, enemy) { Shrine = shrine, VeilCheck = () => Acts.ShrineVeiled };
        Mediator = new BattleMediator(Field, Log);
        EnemyFirstLeader = enemyFirstLeader ?? enemy.Leader;

        Active = player;
        BeginTurn();
        Log.Add("Туман розсіюється. Битва починається!");
    }

    // ---------- Накази ----------

    // Чи можна віддати такий наказ зараз. null — можна; рядок — причина відмови
    public string? Validate(BattleCommand command)
    {
        if (Outcome != BattleOutcome.None) return "Бій уже завершено.";
        if (KiLeft <= 0) return "Ки вичерпано — хід закінчено.";

        if (command.Target is { } squad && !Active.ActiveWarriors.Any(w => w.Type == squad))
            return $"Загін «{squad.ToUa()}» уже не здатний битися.";

        if (command is AbilityCommand)
        {
            if (Active.Leader is null) return "У клану немає Глави.";
            if (Active.Rage < GameConfig.RageToUseAbility)
                return $"Ярість Глави ще не повна: {Active.Rage}/{GameConfig.RageToUseAbility} (росте від влучань і щоходу).";
        }

        if (!Active.ActiveWarriors.Any()) return "Немає кому виконувати наказ.";

        return null;
    }

    // Прогноз: той самий ланцюг, але з PreviewMediator — поле не змінюється
    public BattlePreview BuildPreview(BattleCommand command)
    {
        var mediator = new PreviewMediator(Field, command);
        ClanChainBuilder.Build(Active, mediator, command.RearFirst)?.Handle(command);
        return new BattlePreview(Active, command, mediator.Entries.ToList());
    }

    // null — наказ виконано; рядок — причина відмови
    public string? TryIssue(BattleCommand command, List<string>? trace = null)
    {
        string? error = Validate(command);
        if (error is not null) return error;

        // Ланцюг збираємо на момент наказу: порядок залежить від поточного шикування
        CommandHandler? chain = ClanChainBuilder.Build(Active, Mediator, command.RearFirst);
        if (chain is null) return "Немає кому виконувати наказ.";

        Mediator.BeginCommand(command);

        if (command is AbilityCommand)
        {
            Log.Add($"[{Active.Tag}] Глава ({Active.Leader!.Name}) вимовляє слова сили...");
            Active.Rage = 0;
        }

        chain.Handle(command, trace);
        Mediator.EndCommand(Active, command);

        KiLeft--;
        CommandsUsed++;
        UpdateOutcome();
        _narrator.Check(this);
        Acts.Update(this);
        return null;
    }

    public void EndTurn()
    {
        if (Outcome != BattleOutcome.None) return;

        Active = ReferenceEquals(Active, Player) ? Enemy : Player;

        if (ReferenceEquals(Active, Player))
        {
            Turn++;

            if (Turn > GameConfig.MaxTurns)
            {
                ResolveByCount();
                return;
            }
        }

        BeginTurn();
    }

    private void BeginTurn()
    {
        KiLeft = Math.Max(1, KiPerTurn - Active.KiPenalty);
        Active.KiPenalty = 0;


        // Ярість Глави трохи росте щоходу, щоб здібність не лишалась недосяжною
        Active.AddRage(GameConfig.RagePerTurn);

        // Щит із минулого ходу спадає
        foreach (IWarrior warrior in Active.AllWarriors)
            Field.Shielded.Remove(warrior);

        CommandsUsed = 0;

        Acts.Update(this);   // акт міг змінитися за часом: від нього залежить туман і рішення Інарі
        CollectBlessing();
        CheckBlessingVictory();
        _narrator.Check(this);
        Acts.Update(this);
    }

    // Святилище: воїни на ньому одужують. Змагання: очко Благословення отримує клан із БІЛЬШОЮ кількістю
    // воїнів на святилищі (Кіцуне — посланці Інарі, тому дають 2). Лише бонуси: святилище додає Главі ярості.
    // У тумані (Акт II) і в режимі «вимкнене» нічого не діє
    private void CollectBlessing()
    {
        if (Field.Shrine == ShrineMode.Off || Field.IsVeiled) return;

        List<IWarrior> onShrine = Active.ActiveWarriors.Where(w => Field.IsShrine(w.Position)).ToList();
        if (onShrine.Count == 0) return;

        int healed = onShrine.Count(w => !ReferenceEquals(w.State, w.State.OnRetreat()));
        foreach (IWarrior warrior in onShrine)
            warrior.State = warrior.State.OnRetreat();

        string healNote = healed > 0 ? $" Святилище зцілило бійців: {healed}." : "";

        if (Field.Shrine == ShrineMode.BonusOnly)
        {
            Active.AddRage(1);
            Log.Add($"[{Active.Tag}] Святилище Інарі додає Главі ярості.{healNote}");
            return;
        }

        int rivals = Field.EnemyOf(Active).ActiveWarriors.Count(w => Field.IsShrine(w.Position));
        if (onShrine.Count <= rivals)
        {
            Log.Add($"[{Active.Tag}] Святилище оспорюється ({onShrine.Count} проти {rivals}): очок немає.{healNote}");
            return;
        }

        int points = onShrine.Any(w => w.Type == SquadType.Kitsune) ? 2 : 1;
        Active.Blessing += points;

        Log.Add($"[{Active.Tag}] Святилище Інарі благословляє клан «{Active.Name}»: +{points} " +
                $"({Active.Blessing}/{ShrineGoal})." +
                (points == 2 ? " Лисиці Кіцуне — посланці Інарі!" : "") + healNote);
    }

    // Інарі обирає лише на світанку (Акт III): потрібен поріг очок і перевага над ворогом.
    // Тож добігти до святилища замало: його треба втримати до кінця ночі
    private void CheckBlessingVictory()
    {
        if (Field.Shrine != ShrineMode.Contest || Outcome != BattleOutcome.None || !Acts.ShrineDecides) return;

        if (Active.Blessing >= ShrineGoal && Active.Blessing > Field.EnemyOf(Active).Blessing)
        {
            Finish(ReferenceEquals(Active, Player) ? BattleOutcome.PlayerWon : BattleOutcome.EnemyWon,
                   $"Інарі обирає клан «{Active.Name}»: святилище визнало його гідним!");
        }
    }

    // ---------- Результат ----------
    private void UpdateOutcome()
    {
        bool playerDown = Player.IsDefeated;
        bool enemyDown = Enemy.IsDefeated;

        if (playerDown && enemyDown)
            Finish(BattleOutcome.Draw, "Обидва клани полягли на полі бою.");
        else if (enemyDown)
            Finish(BattleOutcome.PlayerWon, $"Клан «{Enemy.Name}» розгромлено!");
        else if (playerDown)
            Finish(BattleOutcome.EnemyWon, $"Клан «{Player.Name}» розгромлено...");
    }

    private void ResolveByCount()
    {
        int mine = Player.ActiveWarriors.Count();
        int theirs = Enemy.ActiveWarriors.Count();

        if (mine > theirs)
            Finish(BattleOutcome.PlayerWon, $"Світанок! Ваш клан має більше бійців у строю ({mine} проти {theirs}).");
        else if (theirs > mine)
            Finish(BattleOutcome.EnemyWon, $"Світанок! У ворога більше бійців у строю ({theirs} проти {mine}).");
        else
            Finish(BattleOutcome.Draw, $"Світанок! Сили рівні ({mine} на {theirs}).");
    }

    private void Finish(BattleOutcome outcome, string reason)
    {
        Outcome = outcome;
        EndReason = reason;
        Log.Add(reason);
        Acts.Update(this);   // підсумок актів, що перевіряються наприкінці
    }

    // ---------- Memento: Originator ----------
    public BattleMemento CreateMemento(string label) => new(
        label,
        Turn,
        Active,
        KiLeft,
        CommandsUsed,
        Field.Clans.SelectMany(c => c.AllWarriors)
            .Select(w => new WarriorSnapshot(w, w.Position, w.State)).ToList(),
        Field.Clans
            .Select(c => new ClanSnapshot(c, c.Leader, c.KiPenalty, c.Rage, c.Blessing)).ToList(),
        Field.Shielded.ToList(),
        Field.NineLivesUsed.ToList(),
        Acts.Snapshot(),
        _narrator.Told);

    public void Restore(BattleMemento memento)
    {
        Turn = memento.Turn;
        Active = memento.Active;
        KiLeft = memento.KiLeft;
        CommandsUsed = memento.CommandsUsed;
        Outcome = BattleOutcome.None;
        EndReason = "";

        foreach (WarriorSnapshot s in memento.Warriors)
        {
            s.Warrior.Position = s.Position;
            s.Warrior.State = s.State;
        }

        foreach (ClanSnapshot c in memento.Clans)
        {
            c.Clan.KiPenalty = c.KiPenalty;
            c.Clan.Rage = c.Rage;
            c.Clan.Blessing = c.Blessing;

            if (c.Leader is null)
                ClanLeader.GetInstance().ClearLeader(c.Clan);
            else
                ClanLeader.GetInstance().AssignLeader(c.Clan, c.Leader);
        }

        Field.Shielded.Clear();
        foreach (IWarrior warrior in memento.Shielded)
            Field.Shielded.Add(warrior);

        Field.NineLivesUsed.Clear();
        foreach (IWarrior warrior in memento.LivesUsed)
            Field.NineLivesUsed.Add(warrior);

        Acts.Restore(memento.Acts);
        _narrator.Restore(memento.Told);

        Log.Add($"Дзеркало Спогадів: час повернуто до «{memento.Label}».");
    }
}
