namespace ClanBattle;

// Збирає клітини карти з підсвіткою. Правила, щоб підказки не накладались:
//  • ФОН клітини = роль бійця (хто виконує, хто ціль, хто не зможе...) — одна роль на клітину;
//  • крапки на ПОРОЖНІХ клітинках = шлях і зона (лінія пострілу, радіус воїна під курсором), без заливки;
//  • курсор = інверсна клітина (чорне на сірому).
// Кольори фону: синій — виконавець/обраний · червоний — ціль або ворог у зоні · зелений — куди піде/одужає ·
// бордовий — не зможе · сірий — немає цілі · бірюзовий — щит · пурпур — ворог, що дістає вас ·
// жовтогарячий — ваш боєць під загрозою
public static class MapOverlay
{
    private const string Dot = "• ";   // зона (радіус) — тьмяна крапка
    private const string Path = "+ ";  // лінія пострілу — яскравий плюс

    public static MapCell[,] Build(BattleSession session, BattleView view)
    {
        Battlefield field = session.Field;
        var back = new Dictionary<Position, ConsoleColor>();   // ролі бійців (фон)
        var marks = new Dictionary<Position, (string Text, ConsoleColor Color)>();   // крапки/стрілки на порожніх клітинках

        bool squadSelected = view.SelectedKey is >= 1 and <= 4;

        // 1. Загроза (T): хто дістає вас і кого саме
        if (view.ShowThreat)
        {
            foreach (IWarrior enemyWarrior in field.ThreateningWarriors(session.Player))
                back[enemyWarrior.Position] = ConsoleColor.DarkMagenta;

            foreach (IWarrior w in field.ThreatenedWarriors(session.Player))
                back[w.Position] = ConsoleColor.DarkYellow;
        }

        if (view.Preview is null)
        {
            // 2. Обраний загін: він синій, а вороги, до яких хтось із нього дістає, — червоні. Зони не заливаємо
            if (squadSelected)
            {
                List<IWarrior> squad = SelectedWarriors(session, view).ToList();

                foreach (IWarrior enemyWarrior in session.Enemy.ActiveWarriors)
                    if (squad.Any(w => field.CanReach(w, enemyWarrior.Position)))
                        back[enemyWarrior.Position] = ConsoleColor.Red;

                // Фокус (курсор на ворогові): ті, хто його дістає, світліші й мають лінію вогню
                IWarrior? focus = FocusOf(session, view);
                foreach (IWarrior w in squad)
                {
                    bool hitsFocus = focus is not null && field.CanReach(w, focus.Position);
                    back[w.Position] = hitsFocus ? ConsoleColor.Blue : ConsoleColor.DarkBlue;

                    if (hitsFocus)
                        foreach (Position cell in Line(w.Position, focus!.Position))
                            marks[cell] = (Path, ConsoleColor.Yellow);
                }
            }

            // 3. Огляд курсором: радіус воїна під курсором — крапки на порожніх клітинках;
            //    свій воїн показує ворогів у радіусі (червоні), ворог — наших під ударом (жовтогарячі)
            if (view.Cursor is { } at && field.WarriorAt(at) is { } inspected)
            {
                bool mine = ReferenceEquals(field.ClanOf(inspected), session.Player);

                foreach (Position cell in field.RangeCells(inspected))
                {
                    IWarrior? occupant = field.WarriorAt(cell);
                    if (occupant is null)
                        marks[cell] = (Dot, ConsoleColor.DarkYellow);
                    else if (mine && !ReferenceEquals(field.ClanOf(occupant), session.Player))
                        back[cell] = ConsoleColor.Red;
                    else if (!mine && ReferenceEquals(field.ClanOf(occupant), session.Player))
                        back[cell] = ConsoleColor.DarkYellow;
                }
            }
        }
        else
        {
            BattlePreview preview = view.Preview;

            // 2. Лінія пострілу: плюси на порожніх клітинках між стрільцем і ціллю
            foreach (PreviewEntry e in preview.Entries)
                if (e is { Kind: PreviewKind.Shoot, Target: { } victim })
                    foreach (Position p in Line(e.Warrior.Position, victim.Position))
                        marks[p] = (Path, ConsoleColor.Yellow);

            // 3. Куди підуть: зелений фон і стрілка
            foreach (PreviewEntry e in preview.Entries)
            {
                if (e is { Kind: PreviewKind.Move, From: { } origin, To: { } dest })
                {
                    back[dest] = ConsoleColor.DarkGreen;
                    marks[dest] = (dest.X > origin.X ? ">>" : "<<", ConsoleColor.White);
                }
            }

            // 4. Виконавці: колір за тим, що з ними станеться
            foreach (PreviewEntry e in preview.Entries)
            {
                back[e.Warrior.Position] = e.Kind switch
                {
                    PreviewKind.Blocked or PreviewKind.Refused => ConsoleColor.DarkRed,
                    PreviewKind.Heal => ConsoleColor.DarkGreen,
                    PreviewKind.Shield => ConsoleColor.DarkCyan,
                    PreviewKind.NoTarget => ConsoleColor.DarkGray,   // нема чим виконати наказ (немає цілі)
                    _ => ConsoleColor.DarkBlue
                };
            }

            // 5. Цілі пострілів
            foreach (PreviewEntry e in preview.Entries)
                if (e.Target is { } target)
                    back[target.Position] = ConsoleColor.Red;
        }

        // Збираємо клітини: порожня / святилище / позначка (крапка чи стрілка)
        var grid = new MapCell[field.Height, field.Width];
        for (int y = 0; y < field.Height; y++)
        {
            for (int x = 0; x < field.Width; x++)
            {
                var pos = new Position(x, y);
                bool shrine = field.IsShrine(pos);
                string shrineGlyph = session.ShrineVeiled ? "~~" : "[]";   // ~~ — святилище в тумані
                ConsoleColor shrineColor = session.ShrineVeiled ? ConsoleColor.DarkCyan : ConsoleColor.Yellow;

                string text = shrine ? shrineGlyph : ". ";
                ConsoleColor color = shrine ? shrineColor : ConsoleColor.DarkGray;

                // Позначка зони/шляху не затирає святилище (його видно завжди)
                if (marks.TryGetValue(pos, out var mark) && !(shrine && mark.Text is Dot or Path))
                    (text, color) = mark;

                bool has = back.TryGetValue(pos, out ConsoleColor bg);
                grid[y, x] = new MapCell(text, has ? ConsoleColor.White : color, has ? bg : null);
            }
        }

        // Воїни поверх підсвітки (на кольоровому фоні літера біла, щоб було видно)
        foreach (Clan clan in field.Clans)
        {
            IWarrior? leader = clan.Leader;

            foreach (IWarrior warrior in clan.ActiveWarriors)
            {
                bool has = back.TryGetValue(warrior.Position, out ConsoleColor bg);
                string glyph = Palette.Glyph(warrior, ReferenceEquals(warrior, leader),
                    field.Shielded.Contains(warrior), clan.Facing);

                grid[warrior.Position.Y, warrior.Position.X] = new MapCell(
                    glyph,
                    has ? ConsoleColor.White : Palette.ToConsole(warrior.Traits.Color),
                    has ? bg : null);
            }
        }

        // Курсор (режим клавіш): інверсна клітина
        if (view.Cursor is { } cursor && field.InBounds(cursor))
            grid[cursor.Y, cursor.X] = grid[cursor.Y, cursor.X] with { Color = ConsoleColor.Black, Back = ConsoleColor.Gray };

        return grid;
    }

    // Ворог під курсором: саме по ньому б'ють усі, хто дістає (фокус-вогонь)
    public static IWarrior? FocusOf(BattleSession session, BattleView view) =>
        view.Cursor is { } at ? session.Enemy.ActiveWarriors.FirstOrDefault(e => e.Position == at) : null;

    // Воїни гравця, яких зараз охоплює вибір (1–3 — загін, 4 — усі)
    private static IEnumerable<IWarrior> SelectedWarriors(BattleSession session, BattleView view) =>
        session.Player.ActiveWarriors.Where(w => view.SelectedKey == 4 || w.Type.Key() == view.SelectedKey);

    // Клітини між двома точками (без кінців) — лінія вогню
    private static IEnumerable<Position> Line(Position a, Position b)
    {
        int steps = Math.Max(Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));

        for (int i = 1; i < steps; i++)
        {
            int x = a.X + (int)Math.Round((b.X - a.X) * (double)i / steps, MidpointRounding.AwayFromZero);
            int y = a.Y + (int)Math.Round((b.Y - a.Y) * (double)i / steps, MidpointRounding.AwayFromZero);
            yield return new Position(x, y);
        }
    }
}
