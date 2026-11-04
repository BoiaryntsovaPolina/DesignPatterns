namespace ClanBattle;

// Caretaker: зберігає знімки, але не знає, що всередині.
// Два види "Дзеркала Спогадів": ручні слоти (C / L) та стек початків ходів для відкату (U)
public class CheckpointManager
{
    private readonly BattleMemento?[] _slots = new BattleMemento?[GameConfig.MaxCheckpoints];
    private readonly Stack<BattleMemento> _turnStarts = new();

    public int UndoCharges { get; private set; }

    public CheckpointManager(int undoCharges = GameConfig.UndoCharges) => UndoCharges = undoCharges;
    public IReadOnlyList<BattleMemento?> Slots => _slots;

    public void Save(int slot, BattleMemento memento) => _slots[slot] = memento;

    public BattleMemento? Load(int slot) => _slots[slot];

    public void PushTurnStart(BattleMemento memento) => _turnStarts.Push(memento);

    // Після завантаження слота стара історія відкатів уже не відповідає реальності
    public void ResetTimeline(BattleMemento memento)
    {
        _turnStarts.Clear();
        _turnStarts.Push(memento);
    }

    // Якщо в цьому ході вже були накази — повертаємось на початок ходу.
    // Якщо ні — на початок попереднього (разом із ходом ворога)
    public BattleMemento? Undo(bool commandsUsed)
    {
        if (UndoCharges <= 0) return null;

        if (!commandsUsed)
        {
            if (_turnStarts.Count < 2) return null;
            _turnStarts.Pop();
        }

        if (_turnStarts.Count == 0) return null;

        UndoCharges--;
        return _turnStarts.Peek();
    }
}
