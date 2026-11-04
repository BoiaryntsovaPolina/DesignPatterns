namespace ClanBattle;

// Factory Method + Prototype: нащадки лише задають прототип, клонування — тут
public abstract class WarriorFactory
{
    private readonly IWarrior _prototype;

    protected WarriorFactory(IWarrior prototype)
    {
        _prototype = prototype;
    }

    public IWarrior CreateWarrior(int number, Position position)
    {
        IWarrior warrior = _prototype.Clone();
        warrior.Rename($"{_prototype.Name}-{number}");
        warrior.Position = position;
        return warrior;
    }
}
