namespace ClanBattle;

// Усі "магічні числа" гри в одному місці — легко балансувати
public static class GameConfig
{
    public const int FieldWidth = 16;        // клітин по горизонталі (X)
    public const int FieldHeight = 9;        // клітин по вертикалі (Y)
    public const int StartGap = 4;           // порожніх клітин між кланами на старті (ЛР 5)
    public const int SquadColumns = 3;       // кожен загін — окрема колонка

    public const int MinSquadSize = 3;
    public const int MaxSquadSize = 7;

    public const int MaxTurns = 25;          // ліміт ходів (світанок: бій вирішує кількість бійців)
    public const int RageToUseAbility = 6;   // скільки ярості потрібно для здібності Глави
    public const int RagePerTurn = 1;        // ярість, яку клан отримує на початку свого ходу

    public const int ShrineGoalEasy = 6;     // очок Благословення для перемоги (легка складність)
    public const int ShrineGoalHard = 8;     // те саме на складній
    public const int ActIDuration = 8;       // скільки ходів дається на Акт I
    public const int ActIIDuration = 6;      // скільки ходів триває Акт II (мла над святилищем)

    public const int MaxCheckpoints = 3;     // слотів «Дзеркала Спогадів» (ЛР 6)
    public const int UndoCharges = 3;        // відкатів ходу за гру
}
