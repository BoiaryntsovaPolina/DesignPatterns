namespace ClanBattle;

public sealed class WoundedState : IWarriorState
{
    public static readonly WoundedState Instance = new();
    private WoundedState() { }

    public string Name => "поранений";
    public bool CanAdvance => false;   // пораненим наступати не можна
    public bool InBattle => true;

    public IWarriorState OnHit() => OutOfBattleState.Instance;   // повторне влучання
    public IWarriorState OnRetreat() => HealthyState.Instance;   // відступ лікує
}
