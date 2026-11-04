namespace ClanBattle;

public sealed class OutOfBattleState : IWarriorState
{
    public static readonly OutOfBattleState Instance = new();
    private OutOfBattleState() { }

    public string Name => "вийшов з бою";
    public bool CanAdvance => false;
    public bool InBattle => false;

    public IWarriorState OnHit() => this;
    public IWarriorState OnRetreat() => this;
}
