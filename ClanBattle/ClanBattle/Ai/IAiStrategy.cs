namespace ClanBattle;

// Патерн Strategy: різні "характери" ворожого Глави взаємозамінні
public interface IAiStrategy
{
    string Name { get; }

    // null — ворог тримає позицію й завершує хід (не витрачає ки)
    BattleCommand? Choose(BattleSession session, Clan me);
}
