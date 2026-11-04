namespace ClanBattle;

public class Squad
{
    private static readonly Random Rng = new();
    private readonly List<IWarrior> _members = new();

    public SquadType Type { get; }
    public IReadOnlyList<IWarrior> Members => _members;

    public Squad(WarriorFactory factory, SquadType type, int frontWidth, int rowY)
    {
        Type = type;

        int count = Rng.Next(3, 8); // випадкова кількість гравців: 3..7

        // Рівномірно розподіляємо гравців по ширині ряду
        double step = frontWidth / (double)(count + 1);
        for (int i = 1; i <= count; i++)
        {
            int x = Math.Clamp((int)Math.Round(step * i), 0, frontWidth - 1);
            _members.Add(factory.CreateWarrior(i, new Position(x, rowY)));
        }
    }

    public bool RemoveMember(IWarrior warrior) => _members.Remove(warrior);
}
