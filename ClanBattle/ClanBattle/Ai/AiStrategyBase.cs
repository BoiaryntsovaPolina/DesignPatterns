namespace ClanBattle;

public abstract class AiStrategyBase : IAiStrategy
{
    public abstract string Name { get; }
    public abstract BattleCommand? Choose(BattleSession session, Clan me);

    // Загін, у якого найбільше бійців має ціль у зоні дії
    protected static SquadType? BestShootingSquad(BattleSession session, Clan me) =>
        me.Squads
            .Select(sq => (Type: sq.Type, Shooters: sq.Active.Count(w => session.Field.FindTarget(w) is not null)))
            .Where(x => x.Shooters > 0)
            .OrderByDescending(x => x.Shooters)
            .Select(x => (SquadType?)x.Type)
            .FirstOrDefault();

    // Фокус-ціль для загону: добити пораненого, а якщо таких немає — того, до кого дістає більше наших стрільців.
    // Сконцентрований вогонь вибиває ворогів з бою швидше, ніж розрізнені постріли
    protected static Position? BestFocus(BattleSession session, Clan me, SquadType? squadType)
    {
        List<IWarrior> shooters = me.Squads
            .Where(sq => squadType is null || sq.Type == squadType)
            .SelectMany(sq => sq.Active)
            .ToList();

        return session.Field.EnemyOf(me).ActiveWarriors
            .Select(e => (Enemy: e, Hits: shooters.Count(w => session.Field.CanReach(w, e.Position))))
            .Where(x => x.Hits > 0)
            .OrderByDescending(x => !x.Enemy.State.CanAdvance)
            .ThenByDescending(x => x.Hits)
            .Select(x => (Position?)x.Enemy.Position)
            .FirstOrDefault();
    }

    // Скільки вільних клітин між нашим переднім краєм і найближчим ворогом
    protected static int Gap(BattleSession session, Clan me)
    {
        Clan enemy = session.Field.EnemyOf(me);
        if (!me.ActiveWarriors.Any() || !enemy.ActiveWarriors.Any()) return 99;

        int myFront = me.ActiveWarriors.Max(w => w.Position.X * me.Facing);
        int enemyNearest = enemy.ActiveWarriors.Min(w => w.Position.X * me.Facing);
        return enemyNearest - myFront - 1;
    }

    protected static int OnShrine(BattleSession session, Clan clan) =>
        clan.ActiveWarriors.Count(w => session.Field.IsShrine(w.Position));

    // Чи воїн іще не дійшов до святилища (дивимось уздовж напрямку клану)
    protected static bool IsBeforeShrine(BattleSession session, Clan me, IWarrior w) =>
        me.Facing > 0 ? w.Position.X < session.Field.Width / 2 - 1
                      : w.Position.X > session.Field.Width / 2;

    protected static bool AbilityWorthUsing(BattleSession session, Clan me)
    {
        IWarrior? leader = me.Leader;
        if (leader is null || me.Rage < GameConfig.RageToUseAbility) return false;

        return leader.Type switch
        {
            SquadType.Kitsune => me.ActiveWarriors.Count(w => !w.State.CanAdvance) >= 2,
            SquadType.Tanuki => Gap(session, me) <= 3,
            SquadType.Nekomata => BestShootingSquad(session, me) is null
                                  && me.Squads.Any(s => s.Type == SquadType.Nekomata && s.Active.Any()),
            _ => false
        };
    }
}
