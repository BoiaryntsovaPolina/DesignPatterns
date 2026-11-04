namespace ClanBattle;

public enum WarriorColor { Red, Blue, Green, Yellow, Purple, White }

public enum ClothingType { Armor, Kimono, Cloak, Robe }

// Null означає "особливість не задана" (декоратор не застосовано)
public sealed record WarriorTraits(
    WarriorColor? Color = null,
    int? Height = null,
    ClothingType? Clothing = null)
{
    public static readonly WarriorTraits None = new();
}

// Українські назви для виводу
public static class TraitNames
{
    public static string ToUa(this WarriorColor color) => color switch
    {
        WarriorColor.Red => "червоний",
        WarriorColor.Blue => "синій",
        WarriorColor.Green => "зелений",
        WarriorColor.Yellow => "жовтий",
        WarriorColor.Purple => "фіолетовий",
        WarriorColor.White => "білий",
        _ => color.ToString()
    };

    public static string ToUa(this ClothingType clothing) => clothing switch
    {
        ClothingType.Armor => "обладунок",
        ClothingType.Kimono => "кімоно",
        ClothingType.Cloak => "плащ",
        ClothingType.Robe => "ряса",
        _ => clothing.ToString()
    };
}
