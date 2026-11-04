namespace ClanBattle;

// Танукі — броньовані духи-єноти (аналог гномів): важкий захист, утримання фронту.
public class TanukiWarrior : Warrior
{
    public TanukiWarrior(string name, string weapon, string movementType)
        : base(name, weapon, movementType) { }

    public override SquadType Type => SquadType.Tanuki;
    protected override string Label => "Танукі";
}
