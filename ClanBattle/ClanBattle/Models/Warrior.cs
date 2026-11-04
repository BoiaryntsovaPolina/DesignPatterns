namespace ClanBattle;

// Уся спільна логіка воїна. Нащадки задають лише тип і мітку для виводу.
// Equals не перевизначаємо: воїн — окрема особа (порівняння за посиланням),
// а його позиція й стан змінюються, тому порівнювати "за вмістом" не можна
public abstract class Warrior : IWarrior
{
    protected Warrior(string name, string weapon, string movementType)
    {
        Name = name;
        Weapon = weapon;
        MovementType = movementType;
    }

    public string Name { get; private set; }
    public abstract SquadType Type { get; }
    public Position Position { get; set; }
    public string Weapon { get; }
    public string MovementType { get; }

    // State: поточний стан у бою. Стани — незмінні об'єкти, тому поверхневе клонування безпечне
    public IWarriorState State { get; set; } = HealthyState.Instance;

    // Базовий воїн не має додаткових особливостей
    public WarriorTraits Traits => WarriorTraits.None;

    protected abstract string Label { get; }

    public void Rename(string newName) => Name = newName;

    // Поверхнева копія безпечна: string незмінний, Position — struct, State — незмінний об'єкт
    public IWarrior Clone() => (IWarrior)MemberwiseClone();

    public string GetInfo() =>
        $"{"[" + Label + "]",-11} {Name,-10} | Зброя: {Weapon,-28} | Пересування: {MovementType,-24} | Позиція: {Position} | Стан: {State.Name}";
}
