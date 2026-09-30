using AutobaseSystem.Interfaces;

namespace AutobaseSystem.Services;

// правила конкретної сутності - у ValidateFields.
public abstract class EntityValidator<T> : IValidator<T> where T : class, IEntity
{
    public void Validate(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (entity.Id <= 0)
            throw new ArgumentOutOfRangeException(nameof(entity.Id), "Id має бути > 0.");
        ValidateFields(entity);
    }

    protected abstract void ValidateFields(T entity);
}
