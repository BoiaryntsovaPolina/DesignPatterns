namespace AutobaseSystem.Interfaces;

// Читання + додавання. Add відхиляє дубльований Id, тому Id працює як ключ колекції
public interface IRepository<T> : IReadRepository<T> where T : class, IEntity
{
    void Add(T entity);
}
