namespace ClanBattle;

// Керування клавішами без Enter: курсор по карті (огляд і фокус-ціль), вибір загону цифрою, дія стрілкою.
// Повертає ParsedInput, коли гравець віддав наказ або команду меню; null — просто перемалювати екран
public sealed class KeyboardController
{
    public Position Cursor { get; private set; }
    public int SelectedKey { get; private set; }   // 0 — нічого, 1–3 — загін, 4 — усі
    public string? Notice { get; private set; }

    public KeyboardController(Position start) => Cursor = start;

    public void Select(int key) => SelectedKey = key;

    public string SelectedText => SelectedKey switch
    {
        1 => "Танукі",
        2 => "Некомата",
        3 => "Кіцуне",
        4 => "усі загони",
        _ => "нікого (натисніть 1–4)"
    };

    public ParsedInput? Handle(ConsoleKeyInfo info)
    {
        Notice = null;

        // Спочатку службові клавіші, яких немає серед символів
        switch (info.Key)
        {
            case ConsoleKey.RightArrow: return Order('w');       // ваш клан іде вправо
            case ConsoleKey.LeftArrow: return Order('s');
            case ConsoleKey.Spacebar: return Order('f');
            case ConsoleKey.Tab:
                SelectedKey = SelectedKey % 4 + 1;
                return null;
            case ConsoleKey.Escape:
                SelectedKey = 0;
                return null;
            case ConsoleKey.UpArrow:
            case ConsoleKey.DownArrow:
                Notice = "Стрілки вгору/вниз не використовуються: курсор рухайте клавішами W та S.";
                return null;
            case ConsoleKey.Enter:
                return new ParsedInput(InputKind.SelectAtCursor);   // обрати загін бійця під курсором
        }

        char c = InputParser.ToLatin(char.ToLowerInvariant(info.KeyChar));

        switch (c)
        {
            case '1':
            case '2':
            case '3':
            case '4':
                SelectedKey = c - '0';
                return null;

            case 'w': MoveCursor(0, -1); return null;
            case 's': MoveCursor(0, 1); return null;
            case 'a': MoveCursor(-1, 0); return null;
            case 'd': MoveCursor(1, 0); return null;

            case 'f': return Order('f');
            case 'x': return new ParsedInput(InputKind.Command, new AbilityCommand());

            case 'c': return new ParsedInput(InputKind.Save);
            case 'l': return new ParsedInput(InputKind.Load);
            case 'u': return new ParsedInput(InputKind.Undo);
            case 'i': return new ParsedInput(InputKind.Info);
            case 't': return new ParsedInput(InputKind.Threat);
            case 'h': return new ParsedInput(InputKind.Help);
            case 'e': return new ParsedInput(InputKind.EndTurn);
            case 'q': return new ParsedInput(InputKind.Quit);
        }

        Notice = "Невідома клавіша. H — довідка.";
        return null;
    }

    private void MoveCursor(int dx, int dy) =>
        Cursor = new Position(
            Math.Clamp(Cursor.X + dx, 0, GameConfig.FieldWidth - 1),
            Math.Clamp(Cursor.Y + dy, 0, GameConfig.FieldHeight - 1));

    private ParsedInput? Order(char action)
    {
        if (SelectedKey == 0)
        {
            Notice = "Спершу оберіть загін: 1 Танукі, 2 Некомата, 3 Кіцуне, 4 усі (або Tab).";
            return null;
        }

        InputParser.TryParseSquad((char)('0' + SelectedKey), out SquadType? target);

        // Для «битися» передаємо курсор: якщо там ворог, GameUi зробить його фокусом
        return new ParsedInput(InputKind.Command, CommandFactory.Create(action, target),
                               Focus: action == 'f' ? Cursor : null);
    }
}
