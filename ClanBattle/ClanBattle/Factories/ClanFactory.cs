namespace ClanBattle;

// Збирає клан: єдине місце, де перелічені типи загонів
public class ClanFactory
{
    private readonly IWarriorEnhancer _enhancer;

    public ClanFactory(IWarriorEnhancer enhancer) => _enhancer = enhancer;

    public Clan CreateClan(string name, ClanSide side)
    {
        // Новий тип загону = ще один рядок тут
        (WarriorFactory Factory, SquadType Type)[] specs =
        {
            (new TanukiFactory(), SquadType.Tanuki),
            (new NekomataFactory(), SquadType.Nekomata),
            (new KitsuneFactory(), SquadType.Kitsune),
        };

        // Випадковий порядок колонок: 0 — передній край, далі — глибше в тил
        int[] depths = Enumerable.Range(0, specs.Length)
            .OrderBy(_ => GameRandom.Next(1_000_000))
            .ToArray();

        // Клани стоять посередині поля, між передніми колонками — StartGap порожніх клітин
        int westFront = (GameConfig.FieldWidth - GameConfig.StartGap) / 2 - 1;
        int eastFront = westFront + GameConfig.StartGap + 1;

        int ColumnFor(int depth) => side == ClanSide.West ? westFront - depth : eastFront + depth;

        List<Squad> squads = specs
            .Select((s, i) => new Squad(s.Factory, s.Type, _enhancer, GameConfig.FieldHeight, ColumnFor(depths[i])))
            .ToList();

        return new Clan(name, side, squads);
    }
}
