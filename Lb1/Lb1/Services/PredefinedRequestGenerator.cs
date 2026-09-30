using AutobaseSystem.Interfaces;
using AutobaseSystem.Models;

namespace AutobaseSystem.Services;

// Взаємозамінна з RandomRequestGenerator реалізація IRequestGenerator (LSP) 
public class PredefinedRequestGenerator : IRequestGenerator
{
    private readonly Queue<TransportRequest> _predefinedRequests;

    public PredefinedRequestGenerator(IEnumerable<TransportRequest> predefinedRequests)
    {
        _predefinedRequests = new Queue<TransportRequest>(predefinedRequests);
    }

    public TransportRequest? GenerateRequest() =>
        _predefinedRequests.Count > 0 ? _predefinedRequests.Dequeue() : null;
}
