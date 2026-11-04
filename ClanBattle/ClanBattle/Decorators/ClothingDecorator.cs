namespace ClanBattle;

public class ClothingDecorator : WarriorDecorator
{
    private readonly ClothingType _clothing;

    public ClothingDecorator(IWarrior inner, ClothingType clothing) : base(inner) => _clothing = clothing;

    public override WarriorTraits Traits => Inner.Traits with { Clothing = _clothing };
    public override string GetInfo() => $"{Inner.GetInfo()} | Одяг: {_clothing.ToUa()}";

    protected override IWarrior Wrap(IWarrior inner) => new ClothingDecorator(inner, _clothing);
}
