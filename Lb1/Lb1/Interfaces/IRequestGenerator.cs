using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;


public interface IRequestGenerator
{
    TransportRequest? GenerateRequest();
}
