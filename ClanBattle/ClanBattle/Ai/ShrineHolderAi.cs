namespace ClanBattle;

// Складна складність: відводить поранених, б'є, коли є по кому, а головне —
// займає святилище більшістю й утримує його (стоїть замість зайвого руху)
public sealed class ShrineHolderAi : AiStrategyBase
{
    public override string Name => "Обережний (тримає святилище)";

    public override BattleCommand? Choose(BattleSession session, Clan me)
    {
        if (AbilityWorthUsing(session, me))
            return new AbilityCommand();

        // Загін, у якому поранена половина й більше, відходить лікуватися (на святилищі лікує саме воно)
        SquadType? weak = me.Squads
            .Where(sq => sq.Active.Any()
                         && sq.Active.Count(w => !w.State.CanAdvance) * 2 >= sq.Active.Count()
                         && !sq.Active.Any(w => session.Field.IsShrine(w.Position)))
            .Select(sq => (SquadType?)sq.Type)
            .FirstOrDefault();
        if (weak is { } weakSquad)
            return new BackwardCommand(weakSquad);

        if (BestShootingSquad(session, me) is { } shooting)
            return new FightCommand(shooting, BestFocus(session, me, shooting));

        // Очок немає (туман, інший режим): тримати святилище немає сенсу, тож тиснемо на ворога
        if (session.Shrine == ShrineMode.Contest && !session.ShrineVeiled)
        {
            Clan enemy = session.Field.EnemyOf(me);
            if (OnShrine(session, me) > OnShrine(session, enemy))
                return null;   // більшість у нас — стоїмо

            // Більшість іще далеко від святилища: рушаємо всім кланом (так ми не відстаємо від агресивного ворога)
            int behindAll = me.ActiveWarriors.Count(w => IsBeforeShrine(session, me, w));
            if (behindAll * 2 > me.ActiveWarriors.Count())
                return new ForwardCommand(null);

            // Висуваємо загін, у якого найбільше бійців іще не дійшли до святилища
            SquadType? toShrine = me.Squads
                .Select(sq => (sq.Type, Behind: sq.Active.Count(w => IsBeforeShrine(session, me, w))))
                .Where(x => x.Behind > 0)
                .OrderByDescending(x => x.Behind)
                .Select(x => (SquadType?)x.Type)
                .FirstOrDefault();
            if (toShrine is { } squad)
                return new ForwardCommand(squad);

            return null;   // усі вже там, а більшості немає — лишається лише бій
        }

        bool tanukiAlive = me.Squads.Any(sq => sq.Type == SquadType.Tanuki && sq.Active.Any());
        return new ForwardCommand(tanukiAlive ? SquadType.Tanuki : null);
    }
}
