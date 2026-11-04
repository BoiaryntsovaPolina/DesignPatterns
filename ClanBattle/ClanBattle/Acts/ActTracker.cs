namespace ClanBattle;

// Стан актів для Memento: статуси, номер поточного акту й хід, коли він почався
public sealed record ActSnapshot(IReadOnlyList<ActStatus> Status, int Index, int StartTurn);

// Стежить за актами: перевіряє цілі, видає нагороди, пише сцени в журнал.
// Мету перевіряємо щоходу; акт змінюється за часом (Duration) або одразу після виконання мети, а RunsFullTime-акти ще й дотягують свій час
public class ActTracker
{
    private readonly IReadOnlyList<Act> _acts;
    private readonly ActStatus[] _status;
    private int _index;        // поточний акт (== кількості актів, коли всі пройдено)
    private int _startTurn = 1;

    public ActTracker(IReadOnlyList<Act> acts)
    {
        _acts = acts;
        _status = new ActStatus[acts.Count];
    }

    public IReadOnlyList<Act> Acts => _acts;
    public ActStatus StatusOf(int index) => _status[index];
    public int Stars => _status.Count(x => x == ActStatus.Completed);

    // Поточний акт (null — усі акти вже пройдено)
    public Act? Current => _index < _acts.Count ? _acts[_index] : null;

    // Назва поточного акту для заголовка екрана (після всіх актів — останній)
    public string Label => (Current ?? _acts[^1]).Title;

    // Номер поточного акту (0–2); після завершення всіх — їх кількість
    public int Index => _index;

    // Святилище в тумані: нічого не дає
    // (розсіюється, щойно мету акту виконано: повалено Главу ворога)
    public bool ShrineVeiled => Current is { VeilsShrine: true } && _status[_index] == ActStatus.Pending;

    // Світанок: Інарі обирає переможця
    public bool ShrineDecides => Current is { DecidesShrine: true };

    // Memento: стан актів входить до знімка, тож відкат часу не видає нагороди двічі
    public ActSnapshot Snapshot() => new(_status.ToList(), _index, _startTurn);

    public void Restore(ActSnapshot saved)
    {
        for (int i = 0; i < _status.Length && i < saved.Status.Count; i++)
            _status[i] = saved.Status[i];

        _index = saved.Index;
        _startTurn = saved.StartTurn;
    }

    // Рядок про поточний акт для заголовка екрана
    public string StatusLine(BattleSession session)
    {
        Act? act = Current ?? (_acts.Count > 0 ? _acts[^1] : null);
        if (act is null) return "";

        int index = _acts.ToList().IndexOf(act);
        string state = _status[index] switch
        {
            ActStatus.Completed => "★ виконано",
            ActStatus.Failed => "✗ не вдалося",
            _ => act.Progress(session)
        };

        string left = Current is not null && act.Duration > 0
            ? $" (лишилось ходів: {Math.Max(0, act.Duration - (session.Turn - _startTurn))})"
            : "";

        return $"{act.Title}: {act.Objective} — {state}{left}";
    }

    public void Update(BattleSession session)
    {
        // Цикл, бо один акт може одразу передати естафету наступному
        while (Current is { } act)
        {
            bool ended = session.Outcome != BattleOutcome.None;
            bool timedOut = act.Duration > 0 && session.Turn - _startTurn >= act.Duration;

            if (_status[_index] == ActStatus.Pending)
            {
                if (act.EvaluateAtEnd)
                {
                    if (ended || timedOut) Award(session, act.IsMet(session));
                }
                else if (act.IsMet(session))
                {
                    Award(session, true);
                }
                else if (timedOut && !ended)
                {
                    Award(session, false);
                }
            }

            bool judged = _status[_index] != ActStatus.Pending;
            bool leave = judged && (!act.RunsFullTime || timedOut);
            if (!leave || ended) return;   // акт триває (або бій скінчився — акт лишається як є)

            _index++;
            _startTurn = session.Turn;
        }
    }

    // Підсумок акту: статус, сцена в журнал і нагорода (один раз)
    private void Award(BattleSession session, bool success)
    {
        Act act = _acts[_index];
        _status[_index] = success ? ActStatus.Completed : ActStatus.Failed;

        session.Log.Add($"~ {act.Title} {(success ? "★ виконано" : "✗ не вдалося")}. {(success ? act.SuccessScene : act.FailScene)}");

        if (success && act.Reward is not null)
        {
            act.Reward(session);
            session.Log.Add($"~ Нагорода: {act.RewardText}.");
        }
    }
}
