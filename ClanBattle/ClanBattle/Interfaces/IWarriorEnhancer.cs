namespace ClanBattle;

// Додає воїну особливості. Окремий інтерфейс, щоб Squad не знав, як саме їх обирають
public interface IWarriorEnhancer
{
    IWarrior Enhance(IWarrior warrior);
}
