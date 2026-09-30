using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

public interface IDriverFactory
{
    Driver Create(string fullName, int experienceYears);
}
