using ClarityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Patient> Patients { get; }
    DbSet<PatientAddress> PatientAddresses { get; }
    DbSet<EmergencyContact> EmergencyContacts { get; }
    DbSet<GPDetails> GPDetails { get; }
    DbSet<PatientAllergy> PatientAllergies { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Department> Departments { get; }
    DbSet<Clinician> Clinicians { get; }
    DbSet<Room> Rooms { get; }
    DbSet<AppointmentType> AppointmentTypes { get; }
    DbSet<ClinicianAvailability> ClinicianAvailabilities { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<WaitingListEntry> WaitingListEntries { get; }
    DbSet<NotificationLog> NotificationLogs { get; }
    DbSet<Consultation> Consultations { get; }
    DbSet<Observation> Observations { get; }
    DbSet<ClinicalNote> ClinicalNotes { get; }
    DbSet<Diagnosis> Diagnoses { get; }
    DbSet<CarePlan> CarePlans { get; }
    DbSet<LabRequest> LabRequests { get; }
    DbSet<LabTestItem> LabTestItems { get; }
    DbSet<LabResult> LabResults { get; }
    DbSet<Prescription> Prescriptions { get; }
    DbSet<PrescriptionItem> PrescriptionItems { get; }
    DbSet<PharmacyReview> PharmacyReviews { get; }
    DbSet<MedicationCatalog> MedicationCatalogs { get; }
    DbSet<ServiceCatalogue> ServiceCatalogues { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<InsuranceProvider> InsuranceProviders { get; }
    DbSet<InsuranceClaim> InsuranceClaims { get; }
    DbSet<Refund> Refunds { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<SystemSetting> SystemSettings { get; }
    DbSet<SecurityEvent> SecurityEvents { get; }
    DbSet<ReportDefinition> ReportDefinitions { get; }
    DbSet<Ward> Wards { get; }
    DbSet<Bed> Beds { get; }
    DbSet<Admission> Admissions { get; }
    DbSet<Transfer> Transfers { get; }
    DbSet<DischargeChecklist> DischargeChecklists { get; }
    DbSet<BedStatusHistory> BedStatusHistories { get; }
    DbSet<MedicationSchedule> MedicationSchedules { get; }
    DbSet<MedicationAdministrationRecord> MedicationAdministrationRecords { get; }
    DbSet<NursingTask> NursingTasks { get; }
    DbSet<ClinicalAlert> ClinicalAlerts { get; }
    DbSet<ObservationSchedule> ObservationSchedules { get; }
    DbSet<PatientSafetyNote> PatientSafetyNotes { get; }
    DbSet<Document> Documents { get; }
    DbSet<DocumentVersion> DocumentVersions { get; }
    DbSet<FormTemplate> FormTemplates { get; }
    DbSet<PatientForm> PatientForms { get; }
    DbSet<ConsentRecord> ConsentRecords { get; }
    DbSet<PortalUser> PortalUsers { get; }
    DbSet<PortalNotification> PortalNotifications { get; }
    DbSet<IntegrationEndpoint> IntegrationEndpoints { get; }
    DbSet<IntegrationMessage> IntegrationMessages { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AnalyticsSnapshot> AnalyticsSnapshots { get; }
    DbSet<AIInteraction> AIInteractions { get; }
    DbSet<Tenant> Tenants { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
