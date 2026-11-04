namespace ClanBattle;

// Bridge (ЛР 3, розвиток): "реалізація" виводу бою. ЩО показувати — вирішує BattleReport,
// ЯК малювати (псевдографіка чи простий текст) — рендерер
public interface IBattleRenderer
{
    void Clear();
    void DrawTitle(string title);
    void DrawStatus(BattleSession session);
    void DrawField(BattleSession session, BattleView view);
    void DrawCard(BattleSession session, BattleView view);
    void DrawPreview(BattlePreview preview);
    void DrawClanPanel(BattleSession session, Clan clan);
    void DrawLog(IReadOnlyList<string> lines);
    void DrawNote(string text);
}
