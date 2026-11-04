namespace ClanBattle;

// Патерн Chain of Responsibility.
// Кожен гравець клану — ланка. Наказ проходить від першого гравця до останнього:
// ланка виконує його, якщо наказ стосується її загону, і в будь-якому разі передає далі
public abstract class CommandHandler
{
    private CommandHandler? _next;

    // Повертає наступну ланку — зручно будувати ланцюг підряд
    public CommandHandler SetNext(CommandHandler next)
    {
        _next = next;
        return next;
    }

    // trace — необов'язковий "слід" проходження наказу (для режиму показу ланцюга)
    public void Handle(BattleCommand command, List<string>? trace = null)
    {
        bool accepted = Accepts(command);
        if (accepted)
            Execute(command);

        trace?.Add(accepted ? $"{Label}✔" : $"{Label}·");

        _next?.Handle(command, trace);
    }

    protected abstract string Label { get; }
    protected abstract bool Accepts(BattleCommand command);
    protected abstract void Execute(BattleCommand command);
}
