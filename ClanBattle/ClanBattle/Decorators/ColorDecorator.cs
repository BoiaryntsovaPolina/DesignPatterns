namespace ClanBattle;

public class ColorDecorator : WarriorDecorator
{
    private readonly WarriorColor _color;

    public ColorDecorator(IWarrior inner, WarriorColor color) : base(inner) => _color = color;

    public override WarriorTraits Traits => Inner.Traits with { Color = _color };
    public override string GetInfo() => $"{Inner.GetInfo()} | Колір: {_color.ToUa()}";

    protected override IWarrior Wrap(IWarrior inner) => new ColorDecorator(inner, _color);
}
