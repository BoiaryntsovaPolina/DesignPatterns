namespace ClanBattle;

// Танукі — броньовані духи-єноти (аналог гномів): важкий захист, утримання фронту.
public class TanukiWarrior : IWarrior
{
    public string Name { get; private set; }
    public SquadType Type => SquadType.Tanuki;
    public Position Position { get; set; }
    public string Weapon { get; }
    public string MovementType { get; }

    public TanukiWarrior(string name, string weapon, string movementType)
    {
        Name = name;
        Weapon = weapon;
        MovementType = movementType;
    }

    public void Rename(string newName) => Name = newName;

    // Поверхнева копія безпечна: string незмінний, Position — struct
    public IWarrior Clone() => (IWarrior)MemberwiseClone();

    public string GetInfo() =>
        $"[Танукі]   {Name,-10} | Зброя: {Weapon,-28} | Пересування: {MovementType,-24} | Позиція: {Position}";
}
