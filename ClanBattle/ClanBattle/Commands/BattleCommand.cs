namespace ClanBattle;

// Наказ Глави. Патерн Command: наказ — це об'єкт, що сам знає, що робити з воїном.
// Ланцюг відповідальності лише передає його далі й не знає про конкретні команди
public abstract class BattleCommand
{
    // null — наказ усім загонам
    public SquadType? Target { get; }

    protected BattleCommand(SquadType? target) => Target = target;

    public abstract string Title { get; }

    // Клітина-фокус для «битися» (курсор на ворогові); інші накази її не мають
    public virtual Position? Focus => null;

    // Для "назад" ланцюг іде від тилу: інакше передній ряд упирається в задніх
    public virtual bool RearFirst => false;

    public bool AppliesTo(IWarrior warrior) => Target is null || Target == warrior.Type;

    public abstract void ExecuteFor(IWarrior warrior, IBattleMediator mediator);

    public virtual string Describe() =>
        $"{Title} → {(Target is { } t ? t.ToUa() : "усі загони")}";
}
