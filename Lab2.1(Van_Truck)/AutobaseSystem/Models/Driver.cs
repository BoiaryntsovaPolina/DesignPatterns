namespace AutobaseSystem.Models;

public class Driver : Entity
{
    public string FullName { get; private init; }
    public int ExperienceYears { get; private init; }
    public bool IsAvailable { get; private set; } = true;
    public decimal TotalEarnings { get; private set; } = 0m;


    internal Driver(int id, string fullName, int experienceYears) : base(id)
    {
        FullName = fullName;
        ExperienceYears = experienceYears;
    }


    internal void MarkBusy() => IsAvailable = false;
    internal void MarkAvailable() => IsAvailable = true;
    internal void AddEarnings(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Виплата не може бути від'ємною.");
        TotalEarnings += amount;
    }

    public override string ToString() =>
        $"Водій {FullName} (стаж {ExperienceYears} р., {(IsAvailable ? "вільний" : "у рейсі")}, " +
        $"заробіток: {TotalEarnings:0.00} грн)";
}
