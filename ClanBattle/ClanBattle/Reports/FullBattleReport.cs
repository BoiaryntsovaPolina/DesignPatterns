namespace ClanBattle;

// Повний екран бою. Якщо є прогноз наказу — екран коротший: статус, карта з підсвіткою, прогноз
public class FullBattleReport : BattleReport
{
    public FullBattleReport(IBattleRenderer renderer) : base(renderer) { }

    public override void Show(BattleSession session, string? lastTrace, BattleView? view = null)
    {
        BattleView current = view ?? BattleView.Empty;

        Renderer.Clear();

        if (current.Preview is { } preview)
        {
            Renderer.DrawStatus(session);
            Renderer.DrawField(session, current);
            Renderer.DrawPreview(preview);
            return;
        }

        // У режимі клавіш екран тісніший (картка бійця, підказки) — тому без заголовка й з коротшим журналом
        bool compact = current.Cursor is not null;

        if (!compact)
            Renderer.DrawTitle("БИТВА КЛАНІВ: НІЧ СТА ДУХІВ");

        Renderer.DrawStatus(session);
        Renderer.DrawField(session, current);
        Renderer.DrawCard(session, current);
        Renderer.DrawClanPanel(session, session.Player);
        Renderer.DrawClanPanel(session, session.Enemy);

        // Журнал займає те, що лишилось від висоти вікна (карта й накази важливіші за старі рядки)
        int fixedLines = compact ? 30 : 33;
        int logLines = Math.Clamp(Palette.Rows - 1 - fixedLines, 3, compact ? 7 : 10);
        Renderer.DrawLog(session.Log.Last(logLines));

        if (lastTrace is not null)
            Renderer.DrawNote(lastTrace);
    }
}
