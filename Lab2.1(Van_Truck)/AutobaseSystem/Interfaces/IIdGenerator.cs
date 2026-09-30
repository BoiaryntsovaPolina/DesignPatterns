namespace AutobaseSystem.Interfaces;

public interface IIdGenerator
{
    int NextId<TEntity>() where TEntity : IEntity;
}
