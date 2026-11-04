namespace ClanBattle;

// Патерн Mediator: воїни не знають ні про карту, ні один про одного.
// Усе, що стосується сусідів, меж поля й ворогів, вирішує медіатор
public interface IBattleMediator
{
    void RequestAdvance(IWarrior warrior);
    void RequestRetreat(IWarrior warrior);
    void RequestAttack(IWarrior warrior);
    void RequestLeaderAbility(IWarrior warrior);
}
