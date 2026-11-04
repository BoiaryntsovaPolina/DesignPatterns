namespace ClanBattle;

// Складна складність: відводить поранених, наступає танукі першими
public sealed class CautiousAi : AiStrategyBase
{
    public override string Name => "Обережний";

    public override BattleCommand Choose(BattleSession session, Clan me)
    {
        if (AbilityWorthUsing(session, me))
            return new AbilityCommand();

        // Загін, у якому поранена половина й більше, відходить лікуватися
        SquadType? weak = me.Squads
            .Where(sq => sq.Active.Any() && sq.Active.Count(w => !w.State.CanAdvance) * 2 >= sq.Active.Count())
            .Select(sq => (SquadType?)sq.Type)
            .FirstOrDefault();
        if (weak is { } weakSquad)
            return new BackwardCommand(weakSquad);

        if (BestShootingSquad(session, me) is { } shooting)
            return new FightCommand(shooting);

        bool tanukiAlive = me.Squads.Any(sq => sq.Type == SquadType.Tanuki && sq.Active.Any());
        return new ForwardCommand(tanukiAlive ? SquadType.Tanuki : null);
    }
}
