namespace ClanBattle;

// Три акти Ночі Ста Духів: Поклик (ходи 1–8), Мла (9–14), Світанок (з 15-го). Кожен — своя мета, нагорода й сцена.
// Акти змінюють правила: у другому святилище в тумані, у третьому Інарі обирає переможця.
// Мета Акту I залежить від режиму святилища: без очок Благословення вона інша
public static class ActPlan
{
    public static IReadOnlyList<Act> For(ShrineMode shrine)
    {
        Act first = shrine == ShrineMode.Contest
            ? new ActBuilder()
                .Title("I", "Поклик").Lasts(GameConfig.ActIDuration).RunsFullTime()
                .Goal("Заслужіть 3 очки Благословення (більшість на святилищі)",
                      s => s.Player.Blessing >= 3,
                      s => $"Благословення {s.Player.Blessing}/3")
                .Reward("+3 ярості Глави", s => s.Player.AddRage(3))
                .Scenes("Інарі помітила ваш клан і дарує силу. Та над святилищем уже густішає мла...",
                        "Поклик лишився без відповіді, а над святилищем густішає мла...")
                .Build()
            : new ActBuilder()
                .Title("I", "Поклик").Lasts(GameConfig.ActIDuration).RunsFullTime()
                .Goal("Не втратити жодного воїна",
                      s => s.Player.AllWarriors.All(w => w.InBattle),
                      s => $"втрачено бійців: {s.Player.AllWarriors.Count(w => !w.InBattle)}",
                      evaluateAtEnd: true)
                .Reward("+3 ярості Глави", s => s.Player.AddRage(3))
                .Scenes("Клан зустрів млу в повному складі. Духи схвально шепочуться.",
                        "Поклик забрав частину вашого клану.")
                .Build();

        return new List<Act>
        {
            first,

            new ActBuilder()
                .Title("II", "Мла").Lasts(GameConfig.ActIIDuration).VeilsShrine().RunsFullTime()
                .Goal("Повалити Главу ворога",
                      s => s.EnemyFirstLeader is not null && !s.EnemyFirstLeader.InBattle,
                      s => s.EnemyFirstLeader is not null && !s.EnemyFirstLeader.InBattle ? "Главу повалено" : "Глава ворога ще стоїть")
                .Reward("усі ваші поранені зцілені", s =>
                {
                    foreach (IWarrior warrior in s.Player.ActiveWarriors)
                        warrior.State = warrior.State.OnRetreat();
                })
                .Scenes("Дух ворожої Глави розвіявся, і мла відступає, змиваючи рани вашого клану.",
                        "Мла розсіялась сама, а ворожа Глава досі керує своїми духами.")
                .Build(),

            new ActBuilder()
                .Title("III", "Світанок").DecidesShrine()
                .Goal("Дожити до світанку, не поступившись числом",
                      s => s.Player.ActiveWarriors.Count() >= s.Enemy.ActiveWarriors.Count(),
                      s => $"у строю {s.Player.ActiveWarriors.Count()} проти {s.Enemy.ActiveWarriors.Count()}",
                      evaluateAtEnd: true)
                .Scenes("Небо світлішає, а ваш клан тримає стрій. Духи схиляють голови.",
                        "Світанок застав ваш клан розсіяним. Духи відводять погляди.")
                .Build(),
        };
    }
}
