namespace AutobaseSystem.Interfaces;

// where T : class, IEntity - T має бути класом з Id (потрібно для пошуку і для повернення null в GetById).
public interface IReadRepository<T> where T : class, IEntity
{
    T? GetById(int id); // null, якщо сутності з таким Id немає
    IReadOnlyList<T> GetAll();
}
