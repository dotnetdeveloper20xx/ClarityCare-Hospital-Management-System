namespace ClarityCare.Domain.Entities;

public class PrescriptionItem
{
    public Guid PrescriptionItemId { get; set; }
    public Guid PrescriptionId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public string? Instructions { get; set; }

    public Prescription Prescription { get; set; } = null!;
}
