namespace ClanBattle;

// Окремий звіт про Главу клану з її характеристиками
public class LeaderReport : ClanReport
{
    public LeaderReport(IClanRenderer renderer) : base(renderer) { }

    public override void Print(Clan clan)
    {
        IWarrior? leader = ClanLeader.GetInstance().GetLeader(clan);

        if (leader is null)
            Renderer.DrawNote("Главу клану ще не обрано.");
        else
            Renderer.DrawLeader(LeaderProfile.For(leader));
    }
}
