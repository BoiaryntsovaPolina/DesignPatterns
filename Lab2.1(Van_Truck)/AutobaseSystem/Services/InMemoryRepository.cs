using AutobaseSystem.Interfaces;

namespace AutobaseSystem.Services;

// where T : class, IEntity - T має бути класом і реалізовувати IEntity (є Id)
public class InMemoryRepository<T> : IRepository<T> where T : class, IEntity
{
    private readonly List<T> _items = new();            // порядок додавання
    private readonly Dictionary<int, T> _byId = new();  // швидкий пошук за Id

    public void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // TryAdd повертає false, якщо такий Id уже є
        if (!_byId.TryAdd(entity.Id, entity))
            throw new InvalidOperationException($"{typeof(T).Name} з Id={entity.Id} вже є в колекції.");
        _items.Add(entity);
    }

    public T? GetById(int id) => _byId.TryGetValue(id, out var entity) ? entity : default;

    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();
}
