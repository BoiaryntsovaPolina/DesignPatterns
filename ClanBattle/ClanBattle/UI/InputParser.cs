namespace ClanBattle;

public enum InputKind { Command, NeedSquad, SelectAtCursor, Save, Load, Undo, Info, Threat, Help, EndTurn, Quit, Invalid }

// Focus — клітина курсора в момент наказу (GameUi сам вирішує, чи стоїть там ворог)
public sealed record ParsedInput(InputKind Kind, BattleCommand? Command = null, char Action = ' ', Position? Focus = null);

// Розбір введення гравця: "3f", "1 w", "a", "c" ... Підтримує й українську розкладку
public static class InputParser
{
    // Клавіші української розкладки -> латинські
    private static readonly Dictionary<char, char> Layout = new()
    {
        ['ц'] = 'w', ['і'] = 's', ['а'] = 'f', ['ф'] = 'a', ['с'] = 'c',
        ['д'] = 'l', ['г'] = 'u', ['ш'] = 'i', ['р'] = 'h', ['у'] = 'e', ['й'] = 'q', ['е'] = 't',
        ['в'] = 'd', ['ч'] = 'x',
    };

    public static ParsedInput Parse(string? raw)
    {
        string s = Normalize(raw);

        if (s.Length == 0) return new ParsedInput(InputKind.Invalid);

        switch (s)
        {
            case "c": return new ParsedInput(InputKind.Save);
            case "l": return new ParsedInput(InputKind.Load);
            case "u": return new ParsedInput(InputKind.Undo);
            case "i": return new ParsedInput(InputKind.Info);
            case "t": return new ParsedInput(InputKind.Threat);
            case "h":
            case "?": return new ParsedInput(InputKind.Help);
            case "e": return new ParsedInput(InputKind.EndTurn);
            case "q": return new ParsedInput(InputKind.Quit);
            case "a": return new ParsedInput(InputKind.Command, new AbilityCommand());
        }

        // Лише дія без загону ("w") — запитаємо загін окремо
        if (s.Length == 1 && "wsf".Contains(s[0]))
            return new ParsedInput(InputKind.NeedSquad, Action: s[0]);

        if (s.Length == 2)
        {
            // Дозволяємо і "3f", і "f3"
            char first = s[0], second = s[1];
            if (char.IsLetter(first) && char.IsDigit(second))
                (first, second) = (second, first);

            if (TryParseSquad(first, out SquadType? target) && "wsf".Contains(second))
                return new ParsedInput(InputKind.Command, CommandFactory.Create(second, target));
        }

        return new ParsedInput(InputKind.Invalid);
    }

    // '1'..'3' — окремий загін, '4' — усі (target = null)
    public static bool TryParseSquad(char key, out SquadType? target)
    {
        target = null;
        switch (key)
        {
            case '1': target = SquadType.Tanuki; return true;
            case '2': target = SquadType.Nekomata; return true;
            case '3': target = SquadType.Kitsune; return true;
            case '4': target = null; return true;
            default: return false;
        }
    }

    // Одна клавіша української розкладки -> латинська (за фізичним розташуванням)
    public static char ToLatin(char c) => Layout.TryGetValue(c, out char latin) ? latin : c;

    private static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";

        var chars = raw.Trim().ToLowerInvariant()
            .Where(c => !char.IsWhiteSpace(c))
            .Select(c => Layout.TryGetValue(c, out char latin) ? latin : c);

        return new string(chars.ToArray());
    }
}
