namespace ClanBattle;

public static class AiStrategies
{
    public static IAiStrategy For(Difficulty difficulty) =>
        difficulty == Difficulty.Hard ? new ShrineHolderAi() : new AggressiveAi();
}
