namespace ClanBattle;

// Контекстна підказка під картою: гра сама пояснює, що зараз має сенс робити
public static class HintProvider
{
    public static string For(BattleSession session, bool keyMode)
    {
        Clan me = session.Player;
        Battlefield field = session.Field;

        string fight = keyMode ? "оберіть загін (1–4) і натисніть Пробіл" : "наказ «битися», напр. 4f";
        string retreat = keyMode ? "← «назад»" : "s «назад»";
        string ability = keyMode ? "X" : "A";

        if (!me.ActiveWarriors.Any())
            return "Ваш клан розгромлено.";

        int threatened = field.ThreatenedWarriors(me).Count();
        if (threatened > 0)
            return $"Ворог дістає до {threatened} ваших бійців (T — показати). Відведіть їх ({retreat}) або вдарте першими.";

        int shooters = me.ActiveWarriors.Count(w => field.FindTarget(w) is not null);
        if (shooters > 0)
            return $"{shooters} ваших бійців дістають ворога: {fight}. Влучання підряд дають КОМБО — наступні удари точніші.";

        if (session.ShrineVeiled && session.EnemyFirstLeader is { InBattle: true })
            return "Мла над святилищем: воно нічого не дає. Поваліть Главу ворога (курсор на неї + оберіть загін + Пробіл) — тоді мла розсіється.";

        if (me.Leader is not null && me.Rage >= GameConfig.RageToUseAbility)
            return $"Ярість Глави повна! Здібність ({ability}) може переломити бій.";

        int wounded = me.ActiveWarriors.Count(w => !w.State.CanAdvance);
        if (wounded > 0)
            return $"{wounded} поранених не можуть наступати. «Назад» їх лікує; святилище теж лікує.";

        if (session.Shrine == ShrineMode.Off)
        {
            return "Святилища немає: просувайтеся вперед і б'йте, коли ворог у радіусі (курсор на ворогові = фокус).";
        }

        int mineOn = me.ActiveWarriors.Count(w => field.IsShrine(w.Position));
        int theirsOn = field.EnemyOf(me).ActiveWarriors.Count(w => field.IsShrine(w.Position));

        if (session.Shrine == ShrineMode.BonusOnly && mineOn > 0)
            return "Ви на святилищі: воно лікує й додає Главі ярості. Очок тут немає — бій вирішує сталь.";

        if (mineOn > 0 && theirsOn >= mineOn && !session.ShrineVeiled)
            return $"Святилище оспорюється ({mineOn} проти {theirsOn}): очок не дає нікому. Приведіть більше бійців або вибийте ворогів із нього.";

        if (mineOn > 0)
            return session.Acts.ShrineDecides
                ? $"Світанок! Благословення {me.Blessing}/{session.ShrineGoal} (у ворога {field.EnemyOf(me).Blessing}). Утримайте святилище: Інарі обирає зараз."
                : $"Ви тримаєте святилище Інарі! Благословення {me.Blessing}/{session.ShrineGoal}. Інарі обирає лише на світанку, тож тримайте перевагу над ворогом.";

        int front = me.ActiveWarriors.Max(w => w.Position.X * me.Facing);
        int shrineEdge = me.Facing > 0 ? field.Width / 2 - 1 : -(field.Width / 2);
        int steps = shrineEdge - front;

        return steps > 0
            ? $"До святилища Інарі ще {steps} кл. Просувайтесь вперед; Кіцуне на святилищі дають подвійне Благословення."
            : "Святилище поруч. Займіть його або вдарте по ворогу, що там стоїть.";
    }
}
