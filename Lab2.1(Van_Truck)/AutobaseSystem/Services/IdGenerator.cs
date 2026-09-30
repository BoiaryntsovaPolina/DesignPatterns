using System.Collections.Concurrent;
using AutobaseSystem.Interfaces;

namespace AutobaseSystem.Services;

/*SINGLETON: на всю програму один генератор, тому послідовності Id не можуть "розійтися".*/
public sealed class IdGenerator : IIdGenerator
{
    private static readonly Lazy<IdGenerator> _instance = new(() => new IdGenerator());

    public static IdGenerator Instance => _instance.Value;

    // ConcurrentDictionary безпечний для кількох потоків.
    private readonly ConcurrentDictionary<Type, int> _counters = new();

    private IdGenerator() { }

    /*   першого разу для типу додає 1; далі "(_, current) => current + 1" збільшує на 1 і повертає нове значення.*/
    public int NextId<TEntity>() where TEntity : IEntity =>
        _counters.AddOrUpdate(typeof(TEntity), 1, (_, current) => current + 1);
}
