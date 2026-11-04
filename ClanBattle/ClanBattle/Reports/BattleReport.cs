namespace ClanBattle;

// Bridge: абстракція екрана бою. Тримає рендерер
public abstract class BattleReport
{
    protected readonly IBattleRenderer Renderer;

    protected BattleReport(IBattleRenderer renderer) => Renderer = renderer;

    public abstract void Show(BattleSession session, string? lastTrace, BattleView? view = null);
}
