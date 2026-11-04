namespace ClanBattle;

// Невелике незмінне значення (дві координати): порівнюється за вмістом.
// record struct сам генерує Equals, GetHashCode, == та !=
public readonly record struct Position(int X, int Y)
{
    public override string ToString() => $"({X}; {Y})";
}
