using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

public class InMemoryRequestRepository : InMemoryRepository<TransportRequest>, IRequestRepository
{
    private readonly HashSet<int> _assignedIds = new();
    public IReadOnlyList<TransportRequest> GetPending() =>
        GetAll().Where(r => !_assignedIds.Contains(r.Id)).ToList();
    public void MarkAsAssigned(TransportRequest request)
    {
        if (GetById(request.Id) is null)
            throw new InvalidOperationException($"Заявки #{request.Id} немає в репозиторії.");
        _assignedIds.Add(request.Id);
    }
}
