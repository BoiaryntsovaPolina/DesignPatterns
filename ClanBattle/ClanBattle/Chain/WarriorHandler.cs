namespace ClanBattle;

// Ланка ланцюга для одного воїна. Обгортає воїна й не чіпає декоратори.
// Сама нічого не знає про карту: усе робить через медіатор
public sealed class WarriorHandler : CommandHandler
{
    private readonly IWarrior _warrior;
    private readonly IBattleMediator _mediator;

    public WarriorHandler(IWarrior warrior, IBattleMediator mediator)
    {
        _warrior = warrior;
        _mediator = mediator;
    }

    protected override string Label => _warrior.Name;

    protected override bool Accepts(BattleCommand command) =>
        _warrior.InBattle && command.AppliesTo(_warrior);

    protected override void Execute(BattleCommand command) =>
        command.ExecuteFor(_warrior, _mediator);
}
