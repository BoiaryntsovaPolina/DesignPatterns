namespace ClanBattle;

public enum ActStatus { Pending, Completed, Failed }

// Акт ночі: власна мета, нагорода й сцени. Збирається через ActBuilder.
// Акти йдуть послідовно й змінюються за ПОДІЄЮ (мету виконано) або за браком часу (Duration)
public sealed class Act
{
    public string Roman { get; init; } = "";
    public string Name { get; init; } = "";
    public int Duration { get; init; }                  // ходів на акт; 0 — до кінця бою
    public bool VeilsShrine { get; init; }              // поки акт триває, святилище в тумані: нічого не дає
    public bool DecidesShrine { get; init; }            // лише в цьому акті Інарі обирає переможця
    public string Objective { get; init; } = "";
    public Func<BattleSession, bool> IsMet { get; init; } = _ => false;
    public Func<BattleSession, string> Progress { get; init; } = _ => "";
    public bool RunsFullTime { get; init; }             // мету виконано — нагорода одразу, але акт триває весь свій час
    public bool EvaluateAtEnd { get; init; }            // перевіряти лише наприкінці акту
    public string RewardText { get; init; } = "";
    public Action<BattleSession>? Reward { get; init; }
    public string SuccessScene { get; init; } = "";
    public string FailScene { get; init; } = "";

    public string Title => $"Акт {Roman} «{Name}»";
}
