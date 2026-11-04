namespace ClanBattle;

public static class CommandFactory
{
    // action: 'w' — вперед, 's' — назад, 'f' — битися
    // focus має сенс лише для 'f'
    public static BattleCommand Create(char action, SquadType? target, Position? focus = null) => action switch
    {
        'w' => new ForwardCommand(target),
        's' => new BackwardCommand(target),
        'f' => new FightCommand(target, focus),
        _ => throw new ArgumentException($"Невідома дія '{action}'", nameof(action))
    };
}
