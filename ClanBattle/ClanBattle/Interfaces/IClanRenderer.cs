namespace ClanBattle;

// Bridge
// Що саме показувати вирішують ClanReport (абстракція), а як малювати — рендерер.
public interface IClanRenderer
{
    void BeginClan(Clan clan);
    void BeginSquad(Squad squad);
    void DrawWarrior(IWarrior warrior, bool isLeader);
    void DrawLeader(LeaderProfile profile);
    void DrawNote(string text);
    void EndClan();
}
