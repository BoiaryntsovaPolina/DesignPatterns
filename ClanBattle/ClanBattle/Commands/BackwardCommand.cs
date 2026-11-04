namespace ClanBattle;

public sealed class BackwardCommand : BattleCommand
{
    public BackwardCommand(SquadType? target) : base(target) { }

    public override string Title => "Назад";
    public override bool RearFirst => true;

    public override void ExecuteFor(IWarrior warrior, IBattleMediator mediator) =>
        mediator.RequestRetreat(warrior);
}
