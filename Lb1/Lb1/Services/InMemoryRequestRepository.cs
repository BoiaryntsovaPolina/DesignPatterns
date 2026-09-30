using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class InMemoryRequestRepository : IRequestRepository
{
    // Дві окремі колекції за станом заявки.
    private readonly List<TransportRequest> _pending = new(); // в очікуванні
    private readonly List<TransportRequest> _assigned = new(); // призначений

    public void Add(TransportRequest request) => _pending.Add(request);

    public IReadOnlyList<TransportRequest> GetPending() => _pending.AsReadOnly();

    public void MarkAsAssigned(TransportRequest request)
    {
        _pending.Remove(request);
        _assigned.Add(request);
    }
}
