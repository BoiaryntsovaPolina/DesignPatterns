namespace AutobaseSystem.Models;

// Assigned -> InProgress -> Completed; BrokenDown - можливий проміжний стан.
public enum TripStatus
{
    Assigned,
    InProgress,
    BrokenDown,
    Completed
}
