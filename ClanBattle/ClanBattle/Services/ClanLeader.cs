namespace ClanBattle;

// Патерн Singleton: єдиний реєстр, що зберігає Главу для кожного клану.
// Вивід інформації тут не робимо — це справа Reports/Renderers.
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
}
