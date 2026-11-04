namespace ClanBattle;

public class TanukiFactory : WarriorFactory
{
    public TanukiFactory()
        : base(new TanukiWarrior(
            name: "Танукі",
            weapon: "Кам'яна булава (Tetsubo) та щит-барикада",
            movementType: "Важкий монолітний марш"))
    {
    }
}
