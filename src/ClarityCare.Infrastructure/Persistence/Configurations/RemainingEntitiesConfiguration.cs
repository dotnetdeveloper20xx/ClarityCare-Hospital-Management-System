using ClarityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityCare.Infrastructure.Persistence.Configurations;

public class AIInteractionConfiguration : IEntityTypeConfiguration<AIInteraction>
{
    public void Configure(EntityTypeBuilder<AIInteraction> builder)
    {
        builder.HasKey(e => e.InteractionId);
        builder.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AnalyticsSnapshotConfiguration : IEntityTypeConfiguration<AnalyticsSnapshot>
{
    public void Configure(EntityTypeBuilder<AnalyticsSnapshot> builder)
    {
        builder.HasKey(e => e.SnapshotId);
        builder.Property(e => e.MetricValue).HasPrecision(18, 4);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(e => e.AuditLogId);
        builder.Property(e => e.Module).HasMaxLength(100).IsRequired();
        builder.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Action).HasMaxLength(50).IsRequired();
        builder.Property(e => e.UserEmail).HasMaxLength(256);
        builder.Property(e => e.ChangedBy).HasMaxLength(256).IsRequired();
        builder.Property(e => e.IpAddress).HasMaxLength(50);
        builder.Property(e => e.UserAgent).HasMaxLength(500);
        builder.Property(e => e.CorrelationId).HasMaxLength(100);
        builder.Property(e => e.Reason).HasMaxLength(500);
        builder.HasIndex(e => new { e.EntityName, e.EntityId });
        builder.HasIndex(e => e.ChangedAt);
        builder.HasIndex(e => e.Module);
        builder.HasIndex(e => e.UserId);
    }
}

public class CarePlanConfiguration : IEntityTypeConfiguration<CarePlan>
{
    public void Configure(EntityTypeBuilder<CarePlan> builder)
    {
        builder.HasKey(e => e.CarePlanId);
        builder.HasOne(e => e.Consultation).WithMany(c => c.CarePlans).HasForeignKey(e => e.ConsultationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ClinicalAlertConfiguration : IEntityTypeConfiguration<ClinicalAlert>
{
    public void Configure(EntityTypeBuilder<ClinicalAlert> builder)
    {
        builder.HasKey(e => e.AlertId);
        builder.Property(e => e.Message).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(256).IsRequired();
        builder.Property(e => e.AcknowledgedBy).HasMaxLength(256);
        builder.Property(e => e.ResolvedBy).HasMaxLength(256);
        builder.Property(e => e.ResolutionNotes).HasMaxLength(1000);
        builder.HasIndex(e => new { e.Status, e.Severity });
        builder.HasIndex(e => new { e.PatientId, e.Status });
        builder.HasIndex(e => new { e.WardId, e.Status });
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ClinicalNoteConfiguration : IEntityTypeConfiguration<ClinicalNote>
{
    public void Configure(EntityTypeBuilder<ClinicalNote> builder)
    {
        builder.HasKey(e => e.ClinicalNoteId);
        builder.HasOne(e => e.Consultation).WithMany(c => c.ClinicalNotes).HasForeignKey(e => e.ConsultationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.AmendsNote).WithMany().HasForeignKey(e => e.AmendsClinicalNoteId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ClinicianConfiguration : IEntityTypeConfiguration<Clinician>
{
    public void Configure(EntityTypeBuilder<Clinician> builder)
    {
        builder.HasKey(e => e.ClinicianId);
        builder.HasOne(e => e.Department).WithMany(d => d.Clinicians).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ClinicianAvailabilityConfiguration : IEntityTypeConfiguration<ClinicianAvailability>
{
    public void Configure(EntityTypeBuilder<ClinicianAvailability> builder)
    {
        builder.HasKey(e => e.AvailabilityId);
        builder.HasOne(e => e.Clinician).WithMany(c => c.Availabilities).HasForeignKey(e => e.ClinicianId);
    }
}

public class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> builder)
    {
        builder.HasKey(e => e.ConsentId);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(e => e.DepartmentId);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
    }
}

public class DiagnosisConfiguration : IEntityTypeConfiguration<Diagnosis>
{
    public void Configure(EntityTypeBuilder<Diagnosis> builder)
    {
        builder.HasKey(e => e.DiagnosisId);
        builder.HasOne(e => e.Consultation).WithMany(c => c.Diagnoses).HasForeignKey(e => e.ConsultationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(e => e.DocumentId);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.HasKey(e => e.VersionId);
        builder.HasOne(e => e.Document).WithMany(d => d.Versions).HasForeignKey(e => e.DocumentId);
    }
}

public class EmergencyContactConfiguration : IEntityTypeConfiguration<EmergencyContact>
{
    public void Configure(EntityTypeBuilder<EmergencyContact> builder)
    {
        builder.HasKey(e => e.EmergencyContactId);
        builder.Property(e => e.FullName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Relationship).HasMaxLength(100).IsRequired();
        builder.Property(e => e.PhoneNumber).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(256);
        builder.Property(e => e.AddressLine).HasMaxLength(500);
        builder.Property(e => e.CreatedBy).HasMaxLength(256).IsRequired();
    }
}

public class FormTemplateConfiguration : IEntityTypeConfiguration<FormTemplate>
{
    public void Configure(EntityTypeBuilder<FormTemplate> builder)
    {
        builder.HasKey(e => e.TemplateId);
    }
}

public class GPDetailsConfiguration : IEntityTypeConfiguration<GPDetails>
{
    public void Configure(EntityTypeBuilder<GPDetails> builder)
    {
        builder.HasKey(e => e.GPDetailsId);
    }
}

public class InsuranceClaimConfiguration : IEntityTypeConfiguration<InsuranceClaim>
{
    public void Configure(EntityTypeBuilder<InsuranceClaim> builder)
    {
        builder.HasKey(e => e.ClaimId);
        builder.Property(e => e.ClaimAmount).HasPrecision(18, 2);
        builder.Property(e => e.ApprovedAmount).HasPrecision(18, 2);
        builder.HasOne(e => e.Invoice).WithMany().HasForeignKey(e => e.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Provider).WithMany().HasForeignKey(e => e.ProviderId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class InsuranceProviderConfiguration : IEntityTypeConfiguration<InsuranceProvider>
{
    public void Configure(EntityTypeBuilder<InsuranceProvider> builder)
    {
        builder.HasKey(e => e.ProviderId);
    }
}

public class IntegrationEndpointConfiguration : IEntityTypeConfiguration<IntegrationEndpoint>
{
    public void Configure(EntityTypeBuilder<IntegrationEndpoint> builder)
    {
        builder.HasKey(e => e.IntegrationEndpointId);
    }
}

public class IntegrationMessageConfiguration : IEntityTypeConfiguration<IntegrationMessage>
{
    public void Configure(EntityTypeBuilder<IntegrationMessage> builder)
    {
        builder.HasKey(e => e.MessageId);
        builder.HasOne(e => e.Endpoint).WithMany().HasForeignKey(e => e.EndpointId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.HasKey(e => e.InvoiceItemId);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.LineTotal).HasPrecision(18, 2);
        builder.HasOne(e => e.Service).WithMany().HasForeignKey(e => e.ServiceId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LabRequestConfiguration : IEntityTypeConfiguration<LabRequest>
{
    public void Configure(EntityTypeBuilder<LabRequest> builder)
    {
        builder.HasKey(e => e.LabRequestId);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Consultation).WithMany().HasForeignKey(e => e.ConsultationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LabResultConfiguration : IEntityTypeConfiguration<LabResult>
{
    public void Configure(EntityTypeBuilder<LabResult> builder)
    {
        builder.HasKey(e => e.LabResultId);
        builder.HasOne(e => e.LabRequest).WithMany(lr => lr.Results).HasForeignKey(e => e.LabRequestId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LabTestItemConfiguration : IEntityTypeConfiguration<LabTestItem>
{
    public void Configure(EntityTypeBuilder<LabTestItem> builder)
    {
        builder.HasKey(e => e.LabTestItemId);
        builder.HasOne(e => e.LabRequest).WithMany(lr => lr.TestItems).HasForeignKey(e => e.LabRequestId);
    }
}

public class MedicationAdministrationRecordConfiguration : IEntityTypeConfiguration<MedicationAdministrationRecord>
{
    public void Configure(EntityTypeBuilder<MedicationAdministrationRecord> builder)
    {
        builder.HasKey(e => e.AdministrationId);
        builder.HasOne(e => e.MedicationSchedule).WithMany(ms => ms.Administrations).HasForeignKey(e => e.MedicationScheduleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Admission).WithMany().HasForeignKey(e => e.AdmissionId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicationCatalogConfiguration : IEntityTypeConfiguration<MedicationCatalog>
{
    public void Configure(EntityTypeBuilder<MedicationCatalog> builder)
    {
        builder.HasKey(e => e.MedicationId);
    }
}

public class MedicationScheduleConfiguration : IEntityTypeConfiguration<MedicationSchedule>
{
    public void Configure(EntityTypeBuilder<MedicationSchedule> builder)
    {
        builder.HasKey(e => e.MedicationScheduleId);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Admission).WithMany().HasForeignKey(e => e.AdmissionId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(e => e.NotificationId);
    }
}

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.HasKey(e => e.NotificationId);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class NursingTaskConfiguration : IEntityTypeConfiguration<NursingTask>
{
    public void Configure(EntityTypeBuilder<NursingTask> builder)
    {
        builder.HasKey(e => e.NursingTaskId);
        builder.Property(e => e.TaskType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.AssignedTo).HasMaxLength(256);
        builder.Property(e => e.CompletedBy).HasMaxLength(256);
        builder.Property(e => e.CancellationReason).HasMaxLength(500);
        builder.Property(e => e.CreatedBy).HasMaxLength(256).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
        builder.HasIndex(e => new { e.WardId, e.Status });
        builder.HasIndex(e => new { e.PatientId, e.Status });
        builder.HasIndex(e => e.DueAt);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Admission).WithMany().HasForeignKey(e => e.AdmissionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Ward).WithMany().HasForeignKey(e => e.WardId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ObservationConfiguration : IEntityTypeConfiguration<Observation>
{
    public void Configure(EntityTypeBuilder<Observation> builder)
    {
        builder.HasKey(e => e.ObservationId);
        builder.Property(e => e.Temperature).HasPrecision(5, 2);
        builder.Property(e => e.Weight).HasPrecision(6, 2);
        builder.Property(e => e.Height).HasPrecision(5, 2);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Consultation).WithMany(c => c.Observations).HasForeignKey(e => e.ConsultationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ObservationScheduleConfiguration : IEntityTypeConfiguration<ObservationSchedule>
{
    public void Configure(EntityTypeBuilder<ObservationSchedule> builder)
    {
        builder.HasKey(e => e.ObservationScheduleId);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Admission).WithMany().HasForeignKey(e => e.AdmissionId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PatientAllergyConfiguration : IEntityTypeConfiguration<PatientAllergy>
{
    public void Configure(EntityTypeBuilder<PatientAllergy> builder)
    {
        builder.HasKey(e => e.PatientAllergyId);
        builder.Property(e => e.AllergyName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Reaction).HasMaxLength(500);
        builder.Property(e => e.RecordedBy).HasMaxLength(256).IsRequired();
        builder.HasIndex(e => new { e.PatientId, e.IsActive });
    }
}

public class PatientAddressConfiguration : IEntityTypeConfiguration<PatientAddress>
{
    public void Configure(EntityTypeBuilder<PatientAddress> builder)
    {
        builder.HasKey(e => e.PatientAddressId);
        builder.Property(e => e.Line1).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Line2).HasMaxLength(200);
        builder.Property(e => e.Town).HasMaxLength(100).IsRequired();
        builder.Property(e => e.County).HasMaxLength(100);
        builder.Property(e => e.Postcode).HasMaxLength(10).IsRequired();
        builder.Property(e => e.Country).HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(256).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
        builder.HasIndex(e => new { e.PatientId, e.IsPrimary });
    }
}

public class PatientFormConfiguration : IEntityTypeConfiguration<PatientForm>
{
    public void Configure(EntityTypeBuilder<PatientForm> builder)
    {
        builder.HasKey(e => e.PatientFormId);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Template).WithMany().HasForeignKey(e => e.TemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PatientSafetyNoteConfiguration : IEntityTypeConfiguration<PatientSafetyNote>
{
    public void Configure(EntityTypeBuilder<PatientSafetyNote> builder)
    {
        builder.HasKey(e => e.PatientSafetyNoteId);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(e => e.PaymentId);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(e => e.PermissionId);
        builder.Property(e => e.Code).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.Code).IsUnique();
    }
}

public class PharmacyReviewConfiguration : IEntityTypeConfiguration<PharmacyReview>
{
    public void Configure(EntityTypeBuilder<PharmacyReview> builder)
    {
        builder.HasKey(e => e.ReviewId);
        builder.HasOne(e => e.Prescription).WithOne(p => p.PharmacyReview).HasForeignKey<PharmacyReview>(e => e.PrescriptionId);
    }
}

public class PortalNotificationConfiguration : IEntityTypeConfiguration<PortalNotification>
{
    public void Configure(EntityTypeBuilder<PortalNotification> builder)
    {
        builder.HasKey(e => e.NotificationId);
        builder.HasOne(e => e.PortalUser).WithMany(pu => pu.Notifications).HasForeignKey(e => e.PortalUserId);
    }
}

public class PortalUserConfiguration : IEntityTypeConfiguration<PortalUser>
{
    public void Configure(EntityTypeBuilder<PortalUser> builder)
    {
        builder.HasKey(e => e.PortalUserId);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.HasKey(e => e.PrescriptionId);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Consultation).WithMany().HasForeignKey(e => e.ConsultationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.HasKey(e => e.PrescriptionItemId);
        builder.HasOne(e => e.Prescription).WithMany(p => p.Items).HasForeignKey(e => e.PrescriptionId);
    }
}

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.HasKey(e => e.RefundId);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.HasOne(e => e.Payment).WithOne(p => p.Refund).HasForeignKey<Refund>(e => e.PaymentId);
    }
}

public class ReportDefinitionConfiguration : IEntityTypeConfiguration<ReportDefinition>
{
    public void Configure(EntityTypeBuilder<ReportDefinition> builder)
    {
        builder.HasKey(e => e.ReportDefinitionId);
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(e => e.RoleId);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.Name).IsUnique();
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(e => e.RolePermissionId);
        builder.HasOne(e => e.Role).WithMany(r => r.RolePermissions).HasForeignKey(e => e.RoleId);
        builder.HasOne(e => e.Permission).WithMany(p => p.RolePermissions).HasForeignKey(e => e.PermissionId);
    }
}

public class SecurityEventConfiguration : IEntityTypeConfiguration<SecurityEvent>
{
    public void Configure(EntityTypeBuilder<SecurityEvent> builder)
    {
        builder.HasKey(e => e.SecurityEventId);
        builder.Property(e => e.Description).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.IpAddress).HasMaxLength(50);
        builder.Property(e => e.UserAgent).HasMaxLength(500);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.UserId);
    }
}

public class ServiceCatalogueConfiguration : IEntityTypeConfiguration<ServiceCatalogue>
{
    public void Configure(EntityTypeBuilder<ServiceCatalogue> builder)
    {
        builder.HasKey(e => e.ServiceId);
        builder.Property(e => e.ServiceCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.ServiceCode).IsUnique();
        builder.Property(e => e.Price).HasPrecision(18, 2);
    }
}

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.HasKey(e => e.SystemSettingId);
        builder.Property(e => e.Key).HasMaxLength(200).IsRequired();
        builder.HasIndex(e => e.Key).IsUnique();
    }
}

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(e => e.TenantId);
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.Code).IsUnique();
    }
}

public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.HasKey(e => e.TransferId);
        builder.HasOne(e => e.Admission).WithMany(a => a.Transfers).HasForeignKey(e => e.AdmissionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(e => e.UserRoleId);
        builder.HasOne(e => e.Role).WithMany(r => r.UserRoles).HasForeignKey(e => e.RoleId);
    }
}

public class WaitingListEntryConfiguration : IEntityTypeConfiguration<WaitingListEntry>
{
    public void Configure(EntityTypeBuilder<WaitingListEntry> builder)
    {
        builder.HasKey(e => e.WaitingListEntryId);
        builder.HasOne(e => e.Patient).WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.AppointmentType).WithMany().HasForeignKey(e => e.AppointmentTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class WardConfiguration : IEntityTypeConfiguration<Ward>
{
    public void Configure(EntityTypeBuilder<Ward> builder)
    {
        builder.HasKey(e => e.WardId);
        builder.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class BedStatusHistoryConfiguration : IEntityTypeConfiguration<BedStatusHistory>
{
    public void Configure(EntityTypeBuilder<BedStatusHistory> builder)
    {
        builder.HasKey(e => e.BedStatusHistoryId);
        builder.HasOne(e => e.Bed).WithMany().HasForeignKey(e => e.BedId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DischargeChecklistConfiguration : IEntityTypeConfiguration<DischargeChecklist>
{
    public void Configure(EntityTypeBuilder<DischargeChecklist> builder)
    {
        builder.HasKey(e => e.DischargeChecklistId);
    }
}

public class AppointmentTypeConfiguration : IEntityTypeConfiguration<AppointmentType>
{
    public void Configure(EntityTypeBuilder<AppointmentType> builder)
    {
        builder.HasKey(e => e.AppointmentTypeId);
        builder.HasOne(e => e.Department).WithMany(d => d.AppointmentTypes).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(e => e.RoomId);
        builder.HasOne(e => e.Department).WithMany(d => d.Rooms).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
