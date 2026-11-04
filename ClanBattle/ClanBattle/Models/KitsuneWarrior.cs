namespace ClanBattle;

// Кіцуне — лисиці-духи, посланці Інарі (аналог ельфів): магія та далекий бій.
public class KitsuneWarrior : Warrior
{
    public KitsuneWarrior(string name, string weapon, string movementType)
        : base(name, weapon, movementType) { }

    public override SquadType Type => SquadType.Kitsune;
    protected override string Label => "Кіцуне";
}
