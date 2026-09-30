using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

public interface IRequestRepository
{
    void Add(TransportRequest request);
    IReadOnlyList<TransportRequest> GetPending();
    void MarkAsAssigned(TransportRequest request);
}
