namespace ClanBattle;

// Базовий Decorator: за замовчуванням усе делегує обгорнутому воїну.
// Нові члени IWarrior (команди, стан) достатньо додати тут один раз.
public abstract class WarriorDecorator : IWarrior
{
    protected readonly IWarrior Inner;

    protected WarriorDecorator(IWarrior inner) => Inner = inner;

    public string Name => Inner.Name;
    public SquadType Type => Inner.Type;
    public string Weapon => Inner.Weapon;
    public string MovementType => Inner.MovementType;

    public Position Position
    {
        get => Inner.Position;
        set => Inner.Position = value;
    }

    // Стан зберігається у внутрішньому воїні — усі обгортки бачать один і той самий
    public IWarriorState State
    {
        get => Inner.State;
        set => Inner.State = value;
    }

    public virtual WarriorTraits Traits => Inner.Traits;
    public virtual string GetInfo() => Inner.GetInfo();

    public void Rename(string newName) => Inner.Rename(newName);

    // Prototype + Decorator: клонуємо внутрішнього воїна й обгортаємо знову
    public IWarrior Clone() => Wrap(Inner.Clone());

    protected abstract IWarrior Wrap(IWarrior inner);
}
