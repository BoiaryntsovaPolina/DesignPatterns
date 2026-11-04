namespace ClanBattle;

// Бонус: особлива здібність Глави. Іде тим самим ланцюгом, що й звичайні накази
public sealed class AbilityCommand : BattleCommand
{
    public AbilityCommand() : base(null) { }

    public override string Title => "Здібність Глави";

    public override void ExecuteFor(IWarrior warrior, IBattleMediator mediator) =>
        mediator.RequestLeaderAbility(warrior);

    public override string Describe() => Title;
}
