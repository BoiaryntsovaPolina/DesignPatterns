using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class ExperienceBasedDriverSelector : IDriverSelector
{
    public Driver? SelectDriver(TransportRequest request, IReadOnlyList<Driver> availableDrivers)
    {
        // Для дуже складних маршрутів (>= 8 з 10) додатково вимагаємо +2 роки стажу "про запас".
        int requiredExperience = request.RequiredDriverExperienceYears +
                                  (request.DrivingComplexity >= 8 ? 2 : 0);

        /* З усіх, хто підходить за стажем, беремо того, чий стаж найближчий
         до необхідного*/
        return availableDrivers
            .Where(d => d.ExperienceYears >= requiredExperience)
            .OrderBy(d => d.ExperienceYears - requiredExperience)
            .FirstOrDefault();
    }
}
