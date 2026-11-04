namespace ClanBattle;

// Бойові правила в одному місці: радіус, шанс влучання, міфологічні бонуси
public static class CombatRules
{
    public const double IllusionChance = 0.25;   // Танукі: удар розтає димом
    public const double ComboStep = 0.06;        // кожен влучний удар підряд додає шанс наступному
    public const int MaxComboBonusSteps = 4;

    // Радіус удару: ціль у квадраті навколо воїна (в усі боки)
    public static int RangeOf(SquadType type) => type switch
    {
        SquadType.Nekomata => 1,   // кігті — лише впритул
        SquadType.Tanuki => 2,     // булава з розмахом
        SquadType.Kitsune => 3,    // лисячий вогонь летить далеко
        _ => 1
    };

    public static double BaseHitChance(SquadType type) => type switch
    {
        SquadType.Nekomata => 0.85,
        SquadType.Tanuki => 0.70,
        SquadType.Kitsune => 0.60,
        _ => 0.5
    };

    // Одяг і зріст цілі (Decorator із ЛР 3), лисячий вогонь і комбо
    public static double HitChance(IWarrior attacker, IWarrior target, Battlefield field, int combo = 0)
    {
        double chance = BaseHitChance(attacker.Type);
        WarriorTraits traits = target.Traits;

        if (traits.Clothing == ClothingType.Armor)
            chance -= 0.20;

        if (traits.Height is { } height)
        {
            if (height > 185) chance += 0.10;       // великого легше влучити
            else if (height < 150) chance -= 0.10;  // малого важче
        }

        // «Лисячий вогонь»: Кіцуне — посланці Інарі, на святилищі б'ють точніше
        if (attacker.Type == SquadType.Kitsune && field.ShrineWorks(target.Position))
            chance += 0.10;

        chance += Math.Min(combo, MaxComboBonusSteps) * ComboStep;

        return Math.Clamp(chance, 0.10, 0.95);
    }
}
