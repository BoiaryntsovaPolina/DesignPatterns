namespace ClanBattle;

public interface IWarrior : IPrototype<IWarrior>
{
    string Name { get; }
    SquadType Type { get; }
    Position Position { get; set; }
    string Weapon { get; }
    string MovementType { get; }

    // Ім'я клона має бути унікальним
    void Rename(string newName);

    string GetInfo();
}
