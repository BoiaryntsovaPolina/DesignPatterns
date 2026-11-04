namespace ClanBattle;

public enum ClanSide { West, East }

// Equals не перевизначаємо: кланів з однаковою назвою може бути два,
// а ClanLeader використовує Clan як ключ словника (потрібна рівність за посиланням)
public class Clan
{
    public string Name { get; }
    public ClanSide Side { get; }
    public IReadOnlyList<Squad> Squads { get; }

    // Напрямок "вперед" по осі X: клан А (зліва) іде вправо, клан Б (справа) — вліво
    public int Facing => Side == ClanSide.West ? 1 : -1;
    public string Tag => Side == ClanSide.West ? "А" : "Б";

    // Ігрові ресурси клану (зберігаються в Memento разом з усім іншим)
    public int KiPenalty { get; set; }          // скільки ки не буде на наступному ході
    public int Rage { get; set; }               // ярість Глави: заряджається в бою, повна — відкриває здібність

    public void AddRage(int amount) =>
        Rage = Math.Clamp(Rage + amount, 0, GameConfig.RageToUseAbility);
    public int Blessing { get; set; }           // очки Благословення Інарі (святилище)

    public IEnumerable<IWarrior> AllWarriors => Squads.SelectMany(s => s.Members);
    public IEnumerable<IWarrior> ActiveWarriors => AllWarriors.Where(w => w.InBattle);
    public bool IsDefeated => !ActiveWarriors.Any();

    // Singleton ClanLeader зберігає Главу; тут лише зручний доступ
    public IWarrior? Leader => ClanLeader.GetInstance().GetLeader(this);

    // Загони приходять готовими (їх збирає ClanFactory) — Clan не залежить від конкретних типів
    public Clan(string name, ClanSide side, IReadOnlyList<Squad> squads)
    {
        Name = name;
        Side = side;
        Squads = squads;

        ChooseLeader();
    }

    // Випадкова Глава серед тих, хто ще в строю. false — обирати нікого
    public bool ChooseLeader()
    {
        List<IWarrior> candidates = ActiveWarriors.ToList();

        if (candidates.Count == 0)
        {
            ClanLeader.GetInstance().ClearLeader(this);
            return false;
        }

        IWarrior chosen = candidates[GameRandom.Next(candidates.Count)];
        ClanLeader.GetInstance().AssignLeader(this, chosen);
        return true;
    }
}
