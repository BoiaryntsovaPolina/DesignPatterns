namespace AutobaseSystem.Interfaces;

// Окремий шар перевірки даних: правила винесено з конструкторів моделей (SRP).
// Якщо дані некоректні - кидає виняток. Викликається фабрикою/білдером одразу після конструктора моделі,
public interface IValidator<T>
{
    void Validate(T instance);
}
