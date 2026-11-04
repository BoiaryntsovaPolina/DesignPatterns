namespace ClanBattle;

public class BattleLog
{
    private readonly List<string> _lines = new();

    public void Add(string line) => _lines.Add(line);

    public IReadOnlyList<string> Last(int count) =>
        _lines.Skip(Math.Max(0, _lines.Count - count)).ToList();
}
