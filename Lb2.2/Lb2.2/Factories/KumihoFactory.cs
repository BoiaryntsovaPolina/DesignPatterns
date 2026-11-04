namespace ClanBattle;

public class KumihoFactory : WarriorFactory
{
    public KumihoFactory()
        : base(new KumihoWarrior(
            name: "Куміхо",
            weapon: "Лисячий вогонь (Foxfire Orb)",
            movementType: "Містичний стрибок (Phantom Blink)"))
    {
    }
}
