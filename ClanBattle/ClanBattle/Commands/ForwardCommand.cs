namespace ClanBattle;

public sealed class ForwardCommand : BattleCommand
{
    public ForwardCommand(SquadType? target) : base(target) { }

    public override string Title => "Вперед";

    public override void ExecuteFor(IWarrior warrior, IBattleMediator mediator) =>
        mediator.RequestAdvance(warrior);
}
