namespace ClanBattle;

// Дані для виводу картки Глави: воїн + титул + здібність (залежать від типу загону)
public sealed record LeaderProfile(IWarrior Warrior, string Title, string Ability)
{
    public static LeaderProfile For(IWarrior warrior) =>
        new(warrior, GetTitle(warrior.Type), GetAbility(warrior.Type));

    private static string GetTitle(SquadType type) => type switch
    {
        SquadType.Kitsune => "Дев'ятихвоста Владичиця",
        SquadType.Nekomata => "Тінь-Вартівниця",
        SquadType.Tanuki => "Кам'яний Вартовий",
        _ => "Глава клану"
    };

    private static string GetAbility(SquadType type) => type switch
    {
        SquadType.Kitsune =>
            "«Покровительство Хвостів»: лікує всіх поранених у клані (потрібна повна ярість Глави)",
        SquadType.Nekomata =>
            "«Тіньовий Стрибок»: загін Некомата проходить 2 клітини вперед (потрібна повна ярість Глави)",
        SquadType.Tanuki =>
            "«Кам'яна Стіна»: загін Танукі невразливий до наступного обстрілу (потрібна повна ярість Глави)",
        _ => "Особливих здібностей ще не визначено"
    };
}
