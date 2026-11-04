namespace ClanBattle;

// Bridge: абстракція. Тримає рендерер і визначає, ЩО виводити
public abstract class ClanReport
{
    protected readonly IClanRenderer Renderer;

    protected ClanReport(IClanRenderer renderer) => Renderer = renderer;

    public abstract void Print(Clan clan);
}
