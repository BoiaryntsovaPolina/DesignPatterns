namespace ClanBattle;

public class NekomataFactory : WarriorFactory
{
    public NekomataFactory()
        : base(new NekomataWarrior(
            name: "Некомата",
            weapon: "Парні кігті-кама (Dual Kama Claws)",
            movementType: "Стелс-біг по деревах"))
    {
    }
}
