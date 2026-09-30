namespace AutobaseSystem.Models;

// Технічний стан авто. UnderRepair виставляє SimpleRepairService після поломки під час рейсу,
// Operational - після завершення ремонту.
public enum VehicleCondition
{
    Operational,
    Broken,
    UnderRepair
}
