namespace ClanBattle;

public sealed class HealthyState : IWarriorState
{
    public static readonly HealthyState Instance = new();
    private HealthyState() { }

    public string Name => "здоровий";
    public bool CanAdvance => true;
    public bool InBattle => true;

    public IWarriorState OnHit() => WoundedState.Instance;
    public IWarriorState OnRetreat() => this;
}
