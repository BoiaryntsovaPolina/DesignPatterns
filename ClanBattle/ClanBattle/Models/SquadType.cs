namespace ClanBattle;

public enum SquadType
{
    Tanuki,   // важка піхота переднього краю (аналог гномів)
    Nekomata, // швидкий ближній бій (аналог воїнів)
    Kitsune    // магія на відстані (аналог ельфів)
}

public static class SquadTypeNames
{
    public static string ToUa(this SquadType type) => type switch
    {
        SquadType.Tanuki => "Танукі",
        SquadType.Nekomata => "Некомата",
        SquadType.Kitsune => "Кіцуне",
        _ => type.ToString()
    };

    // Клавіша вибору загону в меню: 1, 2, 3
    public static int Key(this SquadType type) => (int)type + 1;
}
