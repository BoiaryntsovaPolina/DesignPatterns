using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

public interface IDriverRepository
{
    // споживач не може Add/Remove в обхід репозиторію, лише читати і змінювати доступність через SetAvailability.
    IReadOnlyList<Driver> GetAvailable();
    IReadOnlyList<Driver> GetAll();
    void SetAvailability(Driver driver, bool isAvailable);
}
