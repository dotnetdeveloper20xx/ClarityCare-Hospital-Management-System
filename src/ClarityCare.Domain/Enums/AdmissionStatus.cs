namespace ClarityCare.Domain.Enums;

public enum AdmissionStatus
{
    Requested,
    PendingBedAllocation,
    Admitted,
    Transferred,
    DischargePlanned,
    Discharged,
    Cancelled
}
