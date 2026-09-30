namespace AutobaseSystem.Models;

public class Driver
{

    public int Id { get; private init; }
    public string FullName { get; private init; }
    public int ExperienceYears { get; private init; }


    public bool IsAvailable { get; private set; } = true;
    public decimal TotalEarnings { get; private set; } = 0m;


    public Driver(int id, string fullName, int experienceYears)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Ім'я обов'язкове.", nameof(fullName));
        if (experienceYears < 0)
            throw new ArgumentOutOfRangeException(nameof(experienceYears), "Стаж >= 0.");

        Id = id;
        FullName = fullName;
        ExperienceYears = experienceYears;
    }

    public void MarkBusy() => IsAvailable = false;

    public void MarkAvailable() => IsAvailable = true;

    public void AddEarnings(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Виплата не може бути від'ємною.");
        TotalEarnings += amount;
    }

    public bool Equals(Driver? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }
    public override bool Equals(object? obj) => Equals(obj as Driver);

    public override int GetHashCode() => Id.GetHashCode();


    public override string ToString() =>
        $"Водій {FullName} (стаж {ExperienceYears} р., {(IsAvailable ? "вільний" : "у рейсі")}, " +
        $"заробіток: {TotalEarnings:0.00} грн)";
}