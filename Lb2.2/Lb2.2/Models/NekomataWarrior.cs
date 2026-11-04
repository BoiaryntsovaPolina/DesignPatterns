namespace ClanBattle;

// Некомата — духи-кішки (аналог воїнів): швидкі фланкові атаки, ближній бій.
public class NekomataWarrior : IWarrior
{
    public string Name { get; private set; }
    public SquadType Type => SquadType.Nekomata;
    public Position Position { get; set; }
    public string Weapon { get; }
    public string MovementType { get; }

    public NekomataWarrior(string name, string weapon, string movementType)
    {
        Name = name;
        Weapon = weapon;
        MovementType = movementType;
    }

    public void Rename(string newName) => Name = newName;

    // Поверхнева копія безпечна: string незмінний, Position — struct
    public IWarrior Clone() => (IWarrior)MemberwiseClone();

    public string GetInfo() =>
        $"[Некомата] {Name,-10} | Зброя: {Weapon,-28} | Пересування: {MovementType,-24} | Позиція: {Position}";
}
