namespace ClanBattle;

public enum PreviewKind { Move, Blocked, Refused, Shoot, NoTarget, Heal, Shield }

// Один рядок прогнозу: що станеться з одним воїном, якщо підтвердити наказ
public sealed record PreviewEntry(
    IWarrior Warrior,
    PreviewKind Kind,
    string Text,
    Position? From = null,
    Position? To = null,
    IWarrior? Target = null);

// Прогноз наказу. Нічого на полі не змінює — лише описує
public sealed record BattlePreview(Clan Clan, BattleCommand Command, IReadOnlyList<PreviewEntry> Entries)
{
    public string Summary()
    {
        int Count(PreviewKind kind) => Entries.Count(e => e.Kind == kind);

        var parts = new List<string>();
        if (Count(PreviewKind.Move) > 0) parts.Add($"зрушать: {Count(PreviewKind.Move)}");
        if (Count(PreviewKind.Blocked) > 0) parts.Add($"заблоковані: {Count(PreviewKind.Blocked)}");
        if (Count(PreviewKind.Refused) > 0) parts.Add($"поранені не наступають: {Count(PreviewKind.Refused)}");
        // «5 з 8»: видно, скільки бійців загону справді мають у кого стріляти
        int shooters = Count(PreviewKind.Shoot), idle = Count(PreviewKind.NoTarget);
        if (shooters > 0) parts.Add($"стріляють: {shooters} з {shooters + idle}");
        else if (idle > 0) parts.Add($"без цілі: {idle}");
        if (Count(PreviewKind.Heal) > 0) parts.Add($"одужають: {Count(PreviewKind.Heal)}");
        if (Count(PreviewKind.Shield) > 0) parts.Add($"під щитом: {Count(PreviewKind.Shield)}");

        return parts.Count > 0 ? string.Join(", ", parts) : "ніхто не відгукнеться";
    }

    // Найважливіше — вгору: постріли, перешкоди; прості кроки видно на карті
    public IEnumerable<PreviewEntry> DetailLines() =>
        Entries.OrderBy(e => e.Kind switch
        {
            PreviewKind.Shoot => 0,
            PreviewKind.Blocked => 1,
            PreviewKind.Refused => 2,
            PreviewKind.Heal => 3,
            PreviewKind.Shield => 4,
            PreviewKind.NoTarget => 5,
            _ => 6
        });
}

// Що саме малювати поверх звичайної карти
// Preview — прогноз наказу; ShowThreat — підсвічувати загрозу; Cursor — клітина під курсором (режим клавіш);
// SelectedKey — обраний загін (0 — нічого, 1–3 — загін, 4 — усі)
public sealed record BattleView(BattlePreview? Preview = null, bool ShowThreat = false, Position? Cursor = null, int SelectedKey = 0)
{
    public static readonly BattleView Empty = new();
}
