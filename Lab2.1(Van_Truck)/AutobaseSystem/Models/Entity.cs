using AutobaseSystem.Interfaces;

namespace AutobaseSystem.Models;

// Рівність ще й за типом: Id унікальний лише в межах типу (Driver #1 і Vehicle #1 - різні).
public abstract class Entity : IEntity, IEquatable<Entity>
{

    public int Id { get; }

    protected Entity(int id)
    {
        Id = id;
    }

    public bool Equals(Entity? other)
    {
        if (other is null) return false;                        
        if (ReferenceEquals(this, other)) return true;          // той самий об'єкт у пам'яті
        return GetType() == other.GetType() && Id == other.Id; 
    }


    public override bool Equals(object? obj) => Equals(obj as Entity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
