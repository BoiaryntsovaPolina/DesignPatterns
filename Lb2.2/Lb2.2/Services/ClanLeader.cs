namespace ClanBattle;

// Патерн Singleton: єдиний реєстр, що зберігає Главу для кожного клану
public sealed class ClanLeader
{
    private static ClanLeader? _instance;

    private readonly Dictionary<Clan, IWarrior> _leaders = new();

    private ClanLeader()
    {
    }

    public static ClanLeader GetInstance()
    {
        if (_instance == null)
            _instance = new ClanLeader();

        return _instance;
    }

    public void AssignLeader(Clan clan, IWarrior warrior) => _leaders[clan] = warrior;

    public void ClearLeader(Clan clan) => _leaders.Remove(clan);

    public IWarrior? GetLeader(Clan clan) => _leaders.GetValueOrDefault(clan);

    private static string GetTitle(SquadType type) => type switch
    {
        SquadType.Kumiho => "Дев'ятихвоста Владичиця",
        SquadType.Nekomata => "Тінь-Вартівниця",
        SquadType.Tanuki => "Кам'яний Вартовий",
        _ => "Глава клану"
    };

    private static string GetSpecialAbility(SquadType type) => type switch
    {
        SquadType.Kumiho =>
            "Аура «Покровительство Хвостів» — посилює магію Куміхо та дає захисний бар'єр усьому клану",
        SquadType.Nekomata =>
            "«Тіньовий Стрибок» — підвищує швидкість і непомітність усього загону Некомата",
        SquadType.Tanuki =>
            "«Кам'яна Стіна» — зміцнює захист переднього краю Танукі",
        _ => "Особливих здібностей ще не визначено"
    };

    public string GetInfo(Clan clan)
    {
        if (!_leaders.TryGetValue(clan, out IWarrior? leader))
            return "Главу клану ще не обрано.";

        return
            "=== ГЛАВА КЛАНУ ===\n" +
            $"Титул:              {GetTitle(leader.Type)}\n" +
            $"Носій титулу:       {leader.GetInfo()}\n" +
            $"Особлива здібність: {GetSpecialAbility(leader.Type)}";
    }
}
