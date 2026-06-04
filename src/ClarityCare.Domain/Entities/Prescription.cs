using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class Prescription
{
    public Guid PrescriptionId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ConsultationId { get; set; }
    public string PrescribedBy { get; set; } = string.Empty;
    public PrescriptionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Patient Patient { get; set; } = null!;
    public Consultation Consultation { get; set; } = null!;
    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
    public PharmacyReview? PharmacyReview { get; set; }
}
