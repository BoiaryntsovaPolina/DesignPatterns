namespace ClanBattle;

// Звичайний текст: кожен гравець — рядок з координатами
public class TextClanRenderer : IClanRenderer
{
    public void BeginClan(Clan clan) => Console.WriteLine($"\n===== КЛАН «{clan.Name}» (клан {clan.Tag}) =====");

    public void BeginSquad(Squad squad) =>
        Console.WriteLine($"\n-- Загін «{squad.Type.ToUa()}», у строю: {squad.Active.Count()}/{squad.Members.Count} --");

    public void DrawWarrior(IWarrior warrior, bool isLeader) =>
        Console.WriteLine("  " + warrior.GetInfo() + (isLeader ? "  <== ГЛАВА" : ""));

    public void DrawLeader(LeaderProfile profile)
    {
        Console.WriteLine("=== ГЛАВА КЛАНУ ===");
        Console.WriteLine($"Титул:              {profile.Title}");
        Console.WriteLine($"Носій титулу:       {profile.Warrior.GetInfo()}");
        Console.WriteLine($"Особлива здібність: {profile.Ability}");
    }

    public void DrawNote(string text) => Console.WriteLine(text);

    public void EndClan() => Console.WriteLine();
}
