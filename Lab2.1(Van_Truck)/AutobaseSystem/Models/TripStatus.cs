namespace AutobaseSystem.Models;

// Assigned -> InProgress -> Completed; BrokenDown - можливий проміжний стан (InProgress -> BrokenDown -> Completed).
public enum TripStatus
{
    Assigned,
    InProgress,
    BrokenDown,
    Completed
}
