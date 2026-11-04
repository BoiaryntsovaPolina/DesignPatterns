namespace ClanBattle;

// Кожна особливість з'являється з ймовірністю, тож комбінації різні (Decorator)
public class RandomTraitsEnhancer : IWarriorEnhancer
{
    private const double Chance = 0.75;

    public IWarrior Enhance(IWarrior warrior)
    {
        if (Roll()) warrior = new ColorDecorator(warrior, PickRandom<WarriorColor>());
        if (Roll()) warrior = new HeightDecorator(warrior, GameRandom.Next(140, 201));
        if (Roll()) warrior = new ClothingDecorator(warrior, PickRandom<ClothingType>());
        return warrior;
    }

    private static bool Roll() => GameRandom.NextDouble() < Chance;

    private static T PickRandom<T>() where T : struct, Enum
    {
        T[] values = Enum.GetValues<T>();
        return values[GameRandom.Next(values.Length)];
    }
}
