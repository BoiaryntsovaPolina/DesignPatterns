namespace ClanBattle;

// Звіт про склад клану: загони та гравці (Главу лише позначаємо)
public class ClanCompositionReport : ClanReport
{
    public ClanCompositionReport(IClanRenderer renderer) : base(renderer) { }

    public override void Print(Clan clan)
    {
        IWarrior? leader = ClanLeader.GetInstance().GetLeader(clan);

        Renderer.BeginClan(clan);

        foreach (Squad squad in clan.Squads)
        {
            Renderer.BeginSquad(squad);

            foreach (IWarrior warrior in squad.Members)
                Renderer.DrawWarrior(warrior, ReferenceEquals(warrior, leader));
        }

        Renderer.EndClan();
    }
}
