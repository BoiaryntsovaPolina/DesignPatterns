namespace ClanBattle;

// Патерн State: поведінка воїна залежить від стану
public interface IWarriorState
{
    string Name { get; }
    bool CanAdvance { get; }   // чи може йти "вперед"
    bool InBattle { get; }     // чи ще бере участь у бою

    IWarriorState OnHit();       // що буде після влучання
    IWarriorState OnRetreat();   // що буде після команди "назад"
}
