using System.Text.Json;

namespace ClanBattle;

// Збереження в файл — це серіалізований Memento. Гра, ланцюг і медіатори про файл нічого не знають:
// BattleSession лише віддає знімок (CreateMemento) і приймає його назад (Restore)

public sealed record WarriorDto(string Name, int X, int Y, string State, string? Color, int? Height, string? Clothing);

public sealed record SquadDto(string Type, List<WarriorDto> Members);

public sealed record ClanDto(string Name, string Side, int KiPenalty, int Rage, int Blessing,
    string? Leader, List<SquadDto> Squads);

public sealed record SaveDto(
    int Version,
    string SavedAt,
    string Difficulty,
    string Shrine,
    int KiPerTurn,
    int Seed,
    long Draws,
    int Turn,
    string ActiveSide,
    int KiLeft,
    int CommandsUsed,
    int UndoCharges,
    string? EnemyFirstLeader,
    List<string> Shielded,
    List<string> NineLivesUsed,
    List<string> ActStatus,
    int ActIndex,
    int ActStartTurn,
    List<string> Told,
    List<ClanDto> Clans);

// Те, що повертає завантаження: готова партія + скільки відкатів лишилось
public sealed record LoadedGame(BattleSession Session, int UndoCharges, string Description);

public static class SaveGameFile
{
    private const int Version = 2;

    // Один файл, що перезаписується при кожному збереженні
    public static string Path { get; } = System.IO.Path.Combine(AppContext.BaseDirectory, "savegame.json");

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping   // кирилиця читабельна
    };

    public static bool Exists => File.Exists(Path);

    public static void Delete()
    {
        try { File.Delete(Path); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    // Пише у тимчасовий файл і підміняє основний: збій посеред запису не зіпсує старе збереження.
    // Повертає null, якщо все гаразд, або текст помилки
    public static string? Save(BattleSession session, int undoCharges)
    {
        try
        {
            string json = JsonSerializer.Serialize(ToDto(session, undoCharges), Options);
            string temp = Path + ".tmp";
            File.WriteAllText(temp, json);
            File.Move(temp, Path, overwrite: true);
            return null;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return $"Не вдалося зберегти гру: {e.Message}";
        }
    }

    // Читає й перевіряє файл. Пошкоджений файл не валить гру: повертається опис помилки
    public static LoadedGame? Load(out string? error)
    {
        error = null;
        try
        {
            SaveDto dto = JsonSerializer.Deserialize<SaveDto>(File.ReadAllText(Path))
                          ?? throw new InvalidDataException("порожній файл");
            if (dto.Version != Version)
                throw new InvalidDataException($"версія файлу {dto.Version}, очікувалась {Version}");

            return FromDto(dto);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException
                                      or InvalidDataException or KeyNotFoundException or ArgumentException
                                      or FormatException or InvalidOperationException)
        {
            error = $"Збереження не вдалося прочитати ({e.Message}).";
            return null;
        }
    }

    // ---------- Memento -> DTO ----------

    private static SaveDto ToDto(BattleSession s, int undoCharges)
    {
        BattleMemento m = s.CreateMemento("Збереження");

        // Воїна в файлі позначаємо парою «сторона + ім'я» (ім'я унікальне в межах клану)
        string Id(IWarrior w) => $"{SideOf(m, w)}:{w.Name}";

        var clans = m.Clans.Select(c => new ClanDto(
            c.Clan.Name,
            c.Clan.Side.ToString(),
            c.KiPenalty, c.Rage, c.Blessing,
            c.Leader?.Name,
            c.Clan.Squads.Select(sq => new SquadDto(
                sq.Type.ToString(),
                sq.Members.Select(w =>
                {
                    WarriorSnapshot snap = m.Warriors.First(x => ReferenceEquals(x.Warrior, w));
                    WarriorTraits t = w.Traits;
                    return new WarriorDto(w.Name, snap.Position.X, snap.Position.Y, StateKey(snap.State),
                        t.Color?.ToString(), t.Height, t.Clothing?.ToString());
                }).ToList())).ToList())).ToList();

        return new SaveDto(
            Version, DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            s.Difficulty.ToString(), s.Shrine.ToString(), s.KiPerTurn,
            GameRandom.Seed, GameRandom.Draws,
            m.Turn, m.Active.Side.ToString(), m.KiLeft, m.CommandsUsed, undoCharges,
            s.EnemyFirstLeader is null ? null : Id(s.EnemyFirstLeader),
            m.Shielded.Select(Id).ToList(),
            m.LivesUsed.Select(Id).ToList(),
            m.Acts.Status.Select(x => x.ToString()).ToList(), m.Acts.Index, m.Acts.StartTurn,
            m.Told.ToList(),
            clans);
    }

    private static string SideOf(BattleMemento m, IWarrior w) =>
        m.Clans.First(c => c.Clan.AllWarriors.Any(x => ReferenceEquals(x, w))).Clan.Side.ToString();

    private static string StateKey(IWarriorState state) =>
        ReferenceEquals(state, OutOfBattleState.Instance) ? "Out" :
        ReferenceEquals(state, WoundedState.Instance) ? "Wounded" : "Healthy";

    private static IWarriorState StateOf(string key) => key switch
    {
        "Out" => OutOfBattleState.Instance,
        "Wounded" => WoundedState.Instance,
        "Healthy" => HealthyState.Instance,
        _ => throw new InvalidDataException($"невідомий стан «{key}»")
    };

    // ---------- DTO -> гра ----------

    private static LoadedGame FromDto(SaveDto d)
    {
        var byId = new Dictionary<string, IWarrior>();
        var clans = new Dictionary<ClanSide, Clan>();

        foreach (ClanDto c in d.Clans)
        {
            ClanSide side = Enum.Parse<ClanSide>(c.Side);
            var squads = new List<Squad>();

            foreach (SquadDto sq in c.Squads)
            {
                SquadType type = Enum.Parse<SquadType>(sq.Type);
                var members = new List<IWarrior>();

                foreach (WarriorDto w in sq.Members)
                {
                    IWarrior warrior = Rebuild(type, w);
                    members.Add(warrior);
                    byId[$"{c.Side}:{w.Name}"] = warrior;
                }

                squads.Add(new Squad(type, members));
            }

            clans[side] = new Clan(c.Name, side, squads);
        }

        Clan player = clans[ClanSide.West];
        Clan enemy = clans[ClanSide.East];

        IWarrior Find(string id) =>
            byId.TryGetValue(id, out IWarrior? w) ? w : throw new InvalidDataException($"воїна «{id}» немає в складі");

        var session = new BattleSession(player, enemy, Enum.Parse<Difficulty>(d.Difficulty), Enum.Parse<ShrineMode>(d.Shrine), d.KiPerTurn,
            d.EnemyFirstLeader is null ? null : Find(d.EnemyFirstLeader));

        // Збираємо знімок із нових об'єктів і віддаємо його звичайному Restore — та сама логіка, що й у відкату
        var warriors = d.Clans.SelectMany(c => c.Squads.SelectMany(sq => sq.Members.Select(w =>
            new WarriorSnapshot(Find($"{c.Side}:{w.Name}"), new Position(w.X, w.Y), StateOf(w.State))))).ToList();

        var clanSnaps = d.Clans.Select(c => new ClanSnapshot(
            clans[Enum.Parse<ClanSide>(c.Side)],
            c.Leader is null ? null : Find($"{c.Side}:{c.Leader}"),
            c.KiPenalty, c.Rage, c.Blessing)).ToList();

        var acts = new ActSnapshot(d.ActStatus.Select(x => Enum.Parse<ActStatus>(x)).ToList(), d.ActIndex, d.ActStartTurn);

        var memento = new BattleMemento(
            "збереженої миті", d.Turn, clans[Enum.Parse<ClanSide>(d.ActiveSide)], d.KiLeft, d.CommandsUsed,
            warriors, clanSnaps,
            d.Shielded.Select(Find).ToList(), d.NineLivesUsed.Select(Find).ToList(),
            acts, d.Told);

        session.Restore(memento);
        session.Log.Add("Гру продовжено зі збереження.");

        // Останнім кроком — «кубики»: після конструкторів, які теж могли кидати, повертаємо точний стан послідовності
        GameRandom.Restore(d.Seed, d.Draws);

        string who = d.Difficulty == nameof(Difficulty.Hard) ? "складна" : "легка";
        return new LoadedGame(session, d.UndoCharges, $"хід {d.Turn}, {who} складність, збережено {d.SavedAt}");
    }

    // Той самий шлях, що й при створенні: Factory Method + Prototype, потім Decorator'и за збереженими рисами
    private static IWarrior Rebuild(SquadType type, WarriorDto w)
    {
        WarriorFactory factory = type switch
        {
            SquadType.Tanuki => new TanukiFactory(),
            SquadType.Nekomata => new NekomataFactory(),
            SquadType.Kitsune => new KitsuneFactory(),
            _ => throw new InvalidDataException($"невідомий загін «{type}»")
        };

        IWarrior warrior = factory.CreateWarrior(1, new Position(w.X, w.Y));
        warrior.Rename(w.Name);

        if (w.Color is not null) warrior = new ColorDecorator(warrior, Enum.Parse<WarriorColor>(w.Color));
        if (w.Height is { } h) warrior = new HeightDecorator(warrior, h);
        if (w.Clothing is not null) warrior = new ClothingDecorator(warrior, Enum.Parse<ClothingType>(w.Clothing));

        return warrior;
    }
}
