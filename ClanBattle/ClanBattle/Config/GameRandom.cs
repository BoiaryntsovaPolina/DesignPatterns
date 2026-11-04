namespace ClanBattle;

// Єдине джерело випадковості: з seed бій можна відтворити (зручно на захисті)
public static class GameRandom
{
    private static Random _random = new();

    public static int Seed { get; private set; }

    // Скільки разів уже кинули «кубик»: разом із Seed цього досить, щоб продовжити послідовність після завантаження
    public static long Draws { get; private set; }

    public static void Reseed(int seed)
    {
        Seed = seed;
        Draws = 0;
        _random = new Random(seed);
    }

    // Відновлення з файлу: той самий seed і «прокрутка» вже витрачених кидків
    public static void Restore(int seed, long draws)
    {
        Reseed(seed);
        for (long i = 0; i < draws; i++)
            _random.NextDouble();   // кожен виклик Random витрачає рівно один семпл
        Draws = draws;
    }

    public static int Next(int minInclusive, int maxExclusive) { Draws++; return _random.Next(minInclusive, maxExclusive); }
    public static int Next(int maxExclusive) { Draws++; return _random.Next(maxExclusive); }
    public static double NextDouble() { Draws++; return _random.NextDouble(); }
}
