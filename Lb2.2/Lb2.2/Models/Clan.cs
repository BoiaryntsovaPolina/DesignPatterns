namespace ClanBattle;

public class Clan
{
    private static readonly Random Rng = new();

    public string Name { get; }
    public IReadOnlyList<Squad> Squads { get; }
    public int FrontWidth { get; } = 20;

    public Clan(string name)
    {
        Name = name;

        // Ряд кожного загону: 0 — передній край, 1 — середина, 2 — тил
        Squads = new List<Squad>
        {
            new(new TanukiFactory(), SquadType.Tanuki, FrontWidth, rowY: 0),
            new(new NekomataFactory(), SquadType.Nekomata, FrontWidth, rowY: 1),
            new(new KumihoFactory(), SquadType.Kumiho, FrontWidth, rowY: 2),
        };

        ChooseLeader();
    }

    // Випадкова Глава з усіх гравців клану
    private void ChooseLeader()
    {
        List<IWarrior> allWarriors = Squads.SelectMany(s => s.Members).ToList();

        if (allWarriors.Count == 0)
        {
            ClanLeader.GetInstance().ClearLeader(this);
            return;
        }

        IWarrior chosen = allWarriors[Rng.Next(allWarriors.Count)];
        ClanLeader.GetInstance().AssignLeader(this, chosen);
    }

    // Глава переможена: прибираємо її із загону й обираємо нову
    public bool DefeatLeader()
    {
        IWarrior? leader = ClanLeader.GetInstance().GetLeader(this);
        if (leader is null)
            return false;

        Squads.FirstOrDefault(s => s.Members.Contains(leader))?.RemoveMember(leader);

        ChooseLeader();
        return ClanLeader.GetInstance().GetLeader(this) is not null;
    }
}
