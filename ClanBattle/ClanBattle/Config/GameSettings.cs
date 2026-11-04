namespace ClanBattle;

public enum Difficulty { Easy, Hard }

// Contest — змагання за святилище (перемога Благословенням на світанку); BonusOnly — лише лікує й дає ярість;
// Off — святилища немає (чисті правила лабораторних: розгром або кількість бійців)
public enum ShrineMode { Contest, BonusOnly, Off }

public static class ShrineModeNames
{
    public static string Describe(ShrineMode mode) => mode switch
    {
        ShrineMode.Contest => "змагання",
        ShrineMode.BonusOnly => "лише бонуси",
        _ => "вимкнене"
    };

    // Що святилище робить просто зараз (для легенди карти й карток)
    public static string Rule(BattleSession session) =>
        session.ShrineVeiled ? "у тумані: нічого не дає"
        : session.Shrine == ShrineMode.BonusOnly ? "лікує й додає Главі ярості"
        : session.Acts.ShrineDecides ? "очко — за більшість; Інарі обирає зараз"
        : "очко — за більшість; Інарі обирає на світанку";
}

// Налаштування з меню. Просто дані, без логіки
public class GameSettings
{
    public string PlayerClanName { get; set; } = "Клан Багряного Місяця";
    public string EnemyClanName { get; set; } = "Клан Крижаних Тіней";
    public bool GraphicMode { get; set; } = true;   // Bridge: псевдографіка чи простий текст
    public bool KeyMode { get; set; } = true;       // керування клавішами (курсор, стрілки) чи текстові команди
    public bool PreviewMode { get; set; } = true;   // прогноз наказу з підсвіткою перед виконанням
    public bool ShowChain { get; set; } = true;     // показувати проходження наказу по ланцюгу
    public Difficulty Difficulty { get; set; } = Difficulty.Easy;
    public ShrineMode Shrine { get; set; } = ShrineMode.Contest;   // як працює святилище Інарі
    public int KiPerTurn { get; set; } = 2;         // накази за хід: 2 (динамічніше) або 1 (як у завданні ЛР 5)
    public int? Seed { get; set; }                  // null — випадковий
}
