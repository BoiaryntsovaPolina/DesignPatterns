namespace ClanBattle;

public class KitsuneFactory : WarriorFactory
{
    public KitsuneFactory()
        : base(new KitsuneWarrior(
            name: "Кіцуне",
            weapon: "Лисячий вогонь (Foxfire Orb)",
            movementType: "Містичний стрибок (Phantom Blink)"))
    {
    }
}
