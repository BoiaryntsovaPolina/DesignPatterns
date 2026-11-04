namespace ClanBattle;

// Куміхо — лисиці-духи (аналог ельфів): магія та далекий бій.
public class KumihoWarrior : IWarrior
{
    public string Name { get; private set; }
    public SquadType Type => SquadType.Kumiho;
    public Position Position { get; set; }
    public string Weapon { get; }
    public string MovementType { get; }

    public KumihoWarrior(string name, string weapon, string movementType)
    {
        Name = name;
        Weapon = weapon;
        MovementType = movementType;
    }

    public void Rename(string newName) => Name = newName;

    // Поверхнева копія безпечна: string незмінний, Position — struct
    public IWarrior Clone() => (IWarrior)MemberwiseClone();

    public string GetInfo() =>
        $"[Куміхо]   {Name,-10} | Зброя: {Weapon,-28} | Пересування: {MovementType,-24} | Позиція: {Position}";
}
