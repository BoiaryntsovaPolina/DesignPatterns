namespace ClanBattle;

public readonly record struct MapCell(string Text, ConsoleColor Color, ConsoleColor? Back = null);

// Спільні "фарби" й малювання сітки для всіх графічних рендерерів
public static class Palette
{
    public static char Symbol(SquadType type) => type switch
    {
        SquadType.Tanuki => 'T',
        SquadType.Nekomata => 'N',
        SquadType.Kitsune => 'K',
        _ => '?'
    };

    public static ConsoleColor ToConsole(WarriorColor? color) => color switch
    {
        WarriorColor.Red => ConsoleColor.Red,
        WarriorColor.Blue => ConsoleColor.Blue,
        WarriorColor.Green => ConsoleColor.Green,
        WarriorColor.Yellow => ConsoleColor.Yellow,
        WarriorColor.Purple => ConsoleColor.Magenta,
        WarriorColor.White => ConsoleColor.White,
        _ => ConsoleColor.Gray
    };

    // Клітина з двох символів: літера типу (мала — поранений) + мітка
    // '*' — Глава, '#' — під щитом, '>' / '<' — напрямок кланів А / Б
    public static string Glyph(IWarrior warrior, bool isLeader, bool shielded, int facing)
    {
        char letter = Symbol(warrior.Type);
        if (!warrior.State.CanAdvance)
            letter = char.ToLowerInvariant(letter);

        char mark = isLeader ? '*' : shielded ? '#' : facing > 0 ? '>' : '<';
        return $"{letter}{mark}";
    }

    public static void PrintGrid(MapCell[,] grid)
    {
        int height = grid.GetLength(0);
        int width = grid.GetLength(1);

        Console.WriteLine("     " + string.Concat(Enumerable.Range(0, width).Select(x => x.ToString().PadRight(3))));
        Console.WriteLine("   ┌" + new string('─', width * 3 + 1) + "┐");

        for (int y = 0; y < height; y++)
        {
            Console.Write($" {y} │ ");

            for (int x = 0; x < width; x++)
            {
                MapCell cell = grid[y, x];
                Console.ForegroundColor = cell.Color;
                if (cell.Back is { } background)
                    Console.BackgroundColor = background;
                Console.Write(cell.Text);
                Console.ResetColor();
                Console.Write(" ");
            }

            Console.WriteLine("│");
        }

        Console.WriteLine("   └" + new string('─', width * 3 + 1) + "┘");
    }

    // Скільки рядків у вікні консолі (екран гри підлаштовується під цю висоту)
    public static int Rows
    {
        get
        {
            try { return Console.WindowHeight; }
            catch (IOException) { return 50; }
        }
    }

    public static bool IsShort => Rows < 40;

    public static void ClearScreen()
    {
        try { Console.Clear(); }
        catch (IOException) { } // вивід перенаправлено — чистити нічого
    }
}
