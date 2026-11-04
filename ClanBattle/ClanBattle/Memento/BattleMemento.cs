namespace ClanBattle;

// Патерн Memento: незмінний знімок стану гри. Ззовні його вміст ніхто не змінює
public sealed record WarriorSnapshot(IWarrior Warrior, Position Position, IWarriorState State);

public sealed record ClanSnapshot(Clan Clan, IWarrior? Leader, int KiPenalty, int Rage, int Blessing);

public sealed record BattleMemento(
    string Label,
    int Turn,
    Clan Active,
    int KiLeft,
    int CommandsUsed,
    IReadOnlyList<WarriorSnapshot> Warriors,
    IReadOnlyList<ClanSnapshot> Clans,
    IReadOnlyList<IWarrior> Shielded,
    IReadOnlyList<IWarrior> LivesUsed,
    ActSnapshot Acts,
    IReadOnlyList<string> Told);
