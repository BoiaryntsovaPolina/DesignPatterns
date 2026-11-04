namespace ClanBattle;

// Патерн Builder: акт збирається покроково
public sealed class ActBuilder
{
    private string _roman = "", _name = "", _objective = "", _rewardText = "", _success = "", _fail = "";
    private int _duration;
    private bool _atEnd, _veil, _decides, _fullTime, _goalSet;
    private Func<BattleSession, bool> _isMet = _ => false;
    private Func<BattleSession, string> _progress = _ => "";
    private Action<BattleSession>? _reward;

    public ActBuilder Title(string roman, string name)
    {
        _roman = roman;
        _name = name;
        return this;
    }

    // Скільки ходів триває акт (0 — до кінця бою)
    public ActBuilder Lasts(int turns)
    {
        _duration = turns;
        return this;
    }

    // Акт триває весь свій час, навіть коли мету вже виконано (нагорода — одразу).
    // Так світанок настає у фіксований час, а не коли хтось вирішив прискорити ніч
    public ActBuilder RunsFullTime()
    {
        _fullTime = true;
        return this;
    }

    // Поки триває акт, святилище в тумані й нічого не дає
    public ActBuilder VeilsShrine()
    {
        _veil = true;
        return this;
    }

    // У цьому акті Інарі обирає переможця за Благословенням
    public ActBuilder DecidesShrine()
    {
        _decides = true;
        return this;
    }

    public ActBuilder Goal(string text, Func<BattleSession, bool> isMet, Func<BattleSession, string> progress, bool evaluateAtEnd = false)
    {
        _objective = text;
        _isMet = isMet;
        _progress = progress;
        _atEnd = evaluateAtEnd;
        _goalSet = true;
        return this;
    }

    public ActBuilder Reward(string text, Action<BattleSession> apply)
    {
        _rewardText = text;
        _reward = apply;
        return this;
    }

    public ActBuilder Scenes(string success, string fail)
    {
        _success = success;
        _fail = fail;
        return this;
    }

    // Перевірка при збиранні: помилку в плані актів видно одразу, а не посеред бою
    public Act Build()
    {
        if (_name.Length == 0) throw new InvalidOperationException("У акту немає назви (Title).");
        if (!_goalSet) throw new InvalidOperationException($"У акту «{_name}» немає мети (Goal).");
        if (_duration < 0) throw new InvalidOperationException($"Тривалість акту «{_name}» не може бути від'ємною.");

        return new Act
        {
            Roman = _roman,
            Name = _name,
            Duration = _duration,
            VeilsShrine = _veil,
            DecidesShrine = _decides,
            RunsFullTime = _fullTime,
            Objective = _objective,
            IsMet = _isMet,
            Progress = _progress,
            EvaluateAtEnd = _atEnd,
            RewardText = _rewardText,
            Reward = _reward,
            SuccessScene = _success,
            FailScene = _fail
        };
    }
}
