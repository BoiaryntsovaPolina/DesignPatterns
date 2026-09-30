namespace AutobaseSystem.Models;

// Технічний стан авто. Broken/UnderRepair виставляє SimpleRepairService під час рейсу.
public enum VehicleCondition
{
    Operational,
    Broken,
    UnderRepair
}
