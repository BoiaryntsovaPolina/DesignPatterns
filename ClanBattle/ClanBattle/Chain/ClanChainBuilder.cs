namespace ClanBattle;

// Збирає ланцюг із гравців клану, що ще в строю.
// Порядок = від переднього краю до тилу (для "назад" — навпаки):
// той, хто попереду, ходить першим і звільняє клітину тим, хто за ним
public static class ClanChainBuilder
{
    public static CommandHandler? Build(Clan clan, IBattleMediator mediator, bool rearFirst)
    {
        // Позиція вздовж напрямку атаки клану: більше = ближче до ворога
        IOrderedEnumerable<IWarrior> ordered = rearFirst
            ? clan.ActiveWarriors.OrderBy(w => w.Position.X * clan.Facing).ThenBy(w => w.Position.Y)
            : clan.ActiveWarriors.OrderByDescending(w => w.Position.X * clan.Facing).ThenBy(w => w.Position.Y);

        CommandHandler? head = null;
        CommandHandler? tail = null;

        foreach (IWarrior warrior in ordered)
        {
            var handler = new WarriorHandler(warrior, mediator);

            if (head is null)
                head = handler;
            else
                tail!.SetNext(handler);

            tail = handler;
        }

        return head;
    }
}
