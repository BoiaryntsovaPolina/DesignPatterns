namespace ClanBattle;

public interface IWarrior : IPrototype<IWarrior>
{
    string Name { get; }
    SquadType Type { get; }
    Position Position { get; set; }
    string Weapon { get; }
    string MovementType { get; }

    // Додаткові особливості (колір, зріст, одяг) — заповнюються декораторами
    WarriorTraits Traits { get; }

    // State: здоровий / поранений / вибув (ЛР 5)
    IWarriorState State { get; set; }
    bool InBattle => State.InBattle;

    // Ім'я клона має бути унікальним
    void Rename(string newName);

    string GetInfo();
}
