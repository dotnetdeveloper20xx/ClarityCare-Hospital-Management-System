using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class PatientForm
{
    public Guid PatientFormId { get; set; }
    public Guid PatientId { get; set; }
    public Guid TemplateId { get; set; }
    public PatientFormStatus Status { get; set; }
    public string? JsonResponse { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? SubmittedBy { get; set; }

    public Patient Patient { get; set; } = null!;
    public FormTemplate Template { get; set; } = null!;
}
