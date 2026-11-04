namespace ClanBattle;

// Легка складність: рветься вперед і стріляє, якщо є по кому. Про святилище не дбає
public sealed class AggressiveAi : AiStrategyBase
{
    public override string Name => "Агресивний";

    public override BattleCommand? Choose(BattleSession session, Clan me)
    {
        if (AbilityWorthUsing(session, me))
            return new AbilityCommand();

        if (BestShootingSquad(session, me) is { } squad)
            return new FightCommand(squad);

        return new ForwardCommand(null);
    }
}
