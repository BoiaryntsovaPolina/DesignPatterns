namespace ClanBattle;

public sealed class FightCommand : BattleCommand
{
    // focus — клітина ворога, по якому першочергово б'ють усі, хто до нього дістає
    public FightCommand(SquadType? target, Position? focus = null) : base(target) => Focus = focus;

    public override string Title => "Битися";
    public override Position? Focus { get; }

    public override void ExecuteFor(IWarrior warrior, IBattleMediator mediator) =>
        mediator.RequestAttack(warrior);

    public override string Describe() =>
        base.Describe() + (Focus is { } cell ? $", фокус {cell}" : "");
}
