using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class Consultation
{
    public Guid ConsultationId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ClinicianId { get; set; }
    public DateTime StartedAt { get; set; }
    public string StartedBy { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public ConsultationStatus Status { get; set; }
    public string? Summary { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Appointment Appointment { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Clinician Clinician { get; set; } = null!;
    public ICollection<Observation> Observations { get; set; } = new List<Observation>();
    public ICollection<ClinicalNote> ClinicalNotes { get; set; } = new List<ClinicalNote>();
    public ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();
    public ICollection<CarePlan> CarePlans { get; set; } = new List<CarePlan>();
}
