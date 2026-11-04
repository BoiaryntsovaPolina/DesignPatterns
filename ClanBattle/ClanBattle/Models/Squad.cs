namespace ClanBattle;

// Загін — одна колонка воїнів на полі (усі мають однаковий X, різні Y)
public class Squad
{
    private readonly List<IWarrior> _members = new();

    public SquadType Type { get; }
    public IReadOnlyList<IWarrior> Members => _members;

    // Ті, хто ще не вибув із бою
    public IEnumerable<IWarrior> Active => _members.Where(m => m.InBattle);

    // Загін із готових воїнів (завантаження збереження)
    public Squad(SquadType type, IEnumerable<IWarrior> members)
    {
        Type = type;
        _members.AddRange(members);
    }

    public Squad(WarriorFactory factory, SquadType type, IWarriorEnhancer enhancer, int fieldHeight, int columnX)
    {
        Type = type;

        int count = GameRandom.Next(GameConfig.MinSquadSize, GameConfig.MaxSquadSize + 1); // випадкова кількість

        // Рівномірно розподіляємо гравців по висоті колонки.
        // AwayFromZero гарантує, що Y різні навіть для 7 гравців у 9 рядках
        double step = fieldHeight / (double)(count + 1);
        for (int i = 1; i <= count; i++)
        {
            int y = Math.Clamp((int)Math.Round(step * i, MidpointRounding.AwayFromZero), 0, fieldHeight - 1);
            IWarrior warrior = factory.CreateWarrior(i, new Position(columnX, y)); // Prototype
            _members.Add(enhancer.Enhance(warrior));                               // Decorator
        }
    }
}
