namespace ClanBattle;

public class HeightDecorator : WarriorDecorator
{
    private readonly int _heightCm;

    public HeightDecorator(IWarrior inner, int heightCm) : base(inner) => _heightCm = heightCm;

    public override WarriorTraits Traits => Inner.Traits with { Height = _heightCm };
    public override string GetInfo() => $"{Inner.GetInfo()} | Зріст: {_heightCm} см";

    protected override IWarrior Wrap(IWarrior inner) => new HeightDecorator(inner, _heightCm);
}
