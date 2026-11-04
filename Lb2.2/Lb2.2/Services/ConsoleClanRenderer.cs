using System.Text;

namespace ClanBattle;

public class ConsoleClanRenderer : IClanRenderer
{
    public void RenderReport(Clan clan)
    {
        Console.WriteLine($"===== КЛАН «{clan.Name}» =====");

        foreach (Squad squad in clan.Squads)
        {
            Console.WriteLine($"\n-- Загін «{squad.Type}», гравців: {squad.Members.Count} --");

            foreach (IWarrior warrior in squad.Members)
                Console.WriteLine("  " + warrior.GetInfo());
        }

        Console.WriteLine();
        Console.WriteLine(ClanLeader.GetInstance().GetInfo(clan));
    }

    public void RenderMap(Clan clan)
    {
        const int rows = 3; // 0 — Танукі, 1 — Некомата, 2 — Куміхо
        char[,] grid = new char[rows, clan.FrontWidth];

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < clan.FrontWidth; x++)
                grid[y, x] = '.';

        foreach (Squad squad in clan.Squads)
            foreach (IWarrior warrior in squad.Members)
                grid[warrior.Position.Y, warrior.Position.X] = GetSymbol(squad.Type);

        IWarrior? leader = ClanLeader.GetInstance().GetLeader(clan);

        Console.WriteLine($"\n===== КАРТА ФРОНТУ: «{clan.Name}» =====");
        Console.WriteLine("     " + string.Concat(
            Enumerable.Range(0, clan.FrontWidth).Select(x => (x % 10).ToString())));
        Console.WriteLine("    +" + new string('-', clan.FrontWidth) + "+");

        // Підпис ряду: тип загону та номер шеренги (0 — фронт)
        string[] rowLabels = { "T(0)", "N(1)", "K(2)" };

        var sb = new StringBuilder();
        for (int y = 0; y < rows; y++)
        {
            sb.Append(rowLabels[y].PadRight(4)).Append('|');

            for (int x = 0; x < clan.FrontWidth; x++)
            {
                bool isLeaderCell = leader is not null
                                    && leader.Position.X == x
                                    && leader.Position.Y == y;

                sb.Append(isLeaderCell ? '*' : grid[y, x]);
            }

            sb.Append("|\n");
        }

        Console.Write(sb.ToString());
        Console.WriteLine("    +" + new string('-', clan.FrontWidth) + "+");
        Console.WriteLine("Позначення: T — Танукі, N — Некомата, K — Куміхо, * — Глава клану, . — порожня клітинка.");
        Console.WriteLine("Ряди зверху вниз: 0 — передній край, 1 — середина, 2 — тил (менший ряд — ближче до ворога).");
    }

    private static char GetSymbol(SquadType type) => type switch
    {
        SquadType.Tanuki => 'T',
        SquadType.Nekomata => 'N',
        SquadType.Kumiho => 'K',
        _ => '?'
    };
}
