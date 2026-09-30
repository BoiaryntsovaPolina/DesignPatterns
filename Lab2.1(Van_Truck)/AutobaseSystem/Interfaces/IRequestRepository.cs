using AutobaseSystem.Models;

namespace AutobaseSystem.Interfaces;

// GetPending - заявки, які ще не отримали рейс (наприклад, не знайшовся водій чи авто);
public interface IRequestRepository : IRepository<TransportRequest>
{
    IReadOnlyList<TransportRequest> GetPending();
    void MarkAsAssigned(TransportRequest request);
}
