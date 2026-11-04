namespace ClanBattle;

// Некомата — духи-кішки (аналог воїнів): швидкі фланкові атаки, ближній бій.
public class NekomataWarrior : Warrior
{
    public NekomataWarrior(string name, string weapon, string movementType)
        : base(name, weapon, movementType) { }

    public override SquadType Type => SquadType.Nekomata;
    protected override string Label => "Некомата";
}
