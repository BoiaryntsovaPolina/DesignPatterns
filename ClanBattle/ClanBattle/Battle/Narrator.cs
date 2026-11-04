namespace ClanBattle;

// Оповідач: розгортає сюжет у ході бою. Нічого не змінює в правилах — лише додає рядки
// в журнал, коли в грі сталася помітна подія (кожна розповідається один раз)
public class Narrator
{
    private readonly HashSet<string> _told = new();

    // Що вже розказано: входить у знімок, щоб після відкату чи завантаження репліки не повторювались
    public IReadOnlyList<string> Told => _told.ToList();

    public void Restore(IEnumerable<string> told)
    {
        _told.Clear();
        foreach (string key in told) _told.Add(key);
    }

    public void Check(BattleSession session)
    {
        bool shrine = session.Shrine != ShrineMode.Off;

        // Початок акту: сцена залежить від того, який акт іде
        switch (session.Acts.Index)
        {
            case 0:
                Tell(session, "act-0", shrine
                    ? "Поклик. Туман піднімається з Долини, святилище Інарі відкрите: заслужіть її Благословення."
                    : "Поклик. Туман піднімається з Долини. Клани сходяться в темряві.");
                break;
            case 1:
                Tell(session, "act-1", shrine
                    ? "Мла. Туман накрив святилище, і Інарі мовчить. Тепер вирішує сталь: повалити Главу ворога!"
                    : "Мла. Туман густішає. Тепер вирішує сталь: повалити Главу ворога!");
                break;
            default:
                Tell(session, "act-2", shrine
                    ? "Світанок. Туман розсіюється, святилище знову видно. Інарі обере гідного!"
                    : "Світанок. Туман розсіюється: небо світлішає.");
                break;
        }

        if (session.Shrine != ShrineMode.Off && session.Acts.Index == 1 && session.Acts.StatusOf(1) == ActStatus.Completed)
            Tell(session, "unveil", "Глава ворога повалена, і мла розсіялась. Святилище знову видно, але Інарі обере гідного лише на світанку.");

        List<IWarrior> all = session.Field.Clans.SelectMany(c => c.AllWarriors).ToList();

        if (all.Any(w => w.InBattle && !w.State.CanAdvance))
            Tell(session, "wound", "Перша кров цієї ночі. Духи затамували подих.");

        if (all.Any(w => !w.InBattle))
            Tell(session, "out", "Один зі духів розвіявся туманом, але ніч іще триває.");

        foreach (Clan clan in session.Field.Clans)
        {
            if (session.Shrine == ShrineMode.Contest)
            {
                if (clan.Blessing >= 1)
                    Tell(session, "bless-first", "Святилище відгукнулося: Інарі помітила тих, хто тримає її землю.");

                if (clan.Blessing >= session.ShrineGoal - 2)
                    Tell(session, "bless-near-" + clan.Tag,
                        $"Лисиці Інарі збираються навколо клану «{clan.Name}». Благословення вже близько!");

                if (clan.Blessing >= session.ShrineGoal && !session.Acts.ShrineDecides)
                    Tell(session, "bless-wait-" + clan.Tag,
                        $"Благословення клану «{clan.Name}» зібрано, але Інарі вирішує лише на світанку. Тримайте перевагу!");
            }

            int alive = clan.ActiveWarriors.Count();
            if (alive > 0 && alive <= 3)
                Tell(session, "last-" + clan.Tag, $"Клан «{clan.Name}» на межі: лишилось усього {alive} бійців.");
        }
    }

    private void Tell(BattleSession session, string key, string text)
    {
        if (_told.Add(key))
            session.Log.Add("~ " + text);
    }
}
