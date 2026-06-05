using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientAddress> PatientAddresses => Set<PatientAddress>();
    public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();
    public DbSet<GPDetails> GPDetails => Set<GPDetails>();
    public DbSet<PatientAllergy> PatientAllergies => Set<PatientAllergy>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Clinician> Clinicians => Set<Clinician>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<AppointmentType> AppointmentTypes => Set<AppointmentType>();
    public DbSet<ClinicianAvailability> ClinicianAvailabilities => Set<ClinicianAvailability>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<WaitingListEntry> WaitingListEntries => Set<WaitingListEntry>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<Observation> Observations => Set<Observation>();
    public DbSet<ClinicalNote> ClinicalNotes => Set<ClinicalNote>();
    public DbSet<Diagnosis> Diagnoses => Set<Diagnosis>();
    public DbSet<CarePlan> CarePlans => Set<CarePlan>();
    public DbSet<LabRequest> LabRequests => Set<LabRequest>();
    public DbSet<LabTestItem> LabTestItems => Set<LabTestItem>();
    public DbSet<LabResult> LabResults => Set<LabResult>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<PharmacyReview> PharmacyReviews => Set<PharmacyReview>();
    public DbSet<MedicationCatalog> MedicationCatalogs => Set<MedicationCatalog>();
    public DbSet<ServiceCatalogue> ServiceCatalogues => Set<ServiceCatalogue>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<InsuranceProvider> InsuranceProviders => Set<InsuranceProvider>();
    public DbSet<InsuranceClaim> InsuranceClaims => Set<InsuranceClaim>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<Ward> Wards => Set<Ward>();
    public DbSet<Bed> Beds => Set<Bed>();
    public DbSet<Admission> Admissions => Set<Admission>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<DischargeChecklist> DischargeChecklists => Set<DischargeChecklist>();
    public DbSet<BedStatusHistory> BedStatusHistories => Set<BedStatusHistory>();
    public DbSet<MedicationSchedule> MedicationSchedules => Set<MedicationSchedule>();
    public DbSet<MedicationAdministrationRecord> MedicationAdministrationRecords => Set<MedicationAdministrationRecord>();
    public DbSet<NursingTask> NursingTasks => Set<NursingTask>();
    public DbSet<ClinicalAlert> ClinicalAlerts => Set<ClinicalAlert>();
    public DbSet<ObservationSchedule> ObservationSchedules => Set<ObservationSchedule>();
    public DbSet<PatientSafetyNote> PatientSafetyNotes => Set<PatientSafetyNote>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<FormTemplate> FormTemplates => Set<FormTemplate>();
    public DbSet<PatientForm> PatientForms => Set<PatientForm>();
    public DbSet<ConsentRecord> ConsentRecords => Set<ConsentRecord>();
    public DbSet<PortalUser> PortalUsers => Set<PortalUser>();
    public DbSet<PortalNotification> PortalNotifications => Set<PortalNotification>();
    public DbSet<IntegrationEndpoint> IntegrationEndpoints => Set<IntegrationEndpoint>();
    public DbSet<IntegrationMessage> IntegrationMessages => Set<IntegrationMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AnalyticsSnapshot> AnalyticsSnapshots => Set<AnalyticsSnapshot>();
    public DbSet<AIInteraction> AIInteractions => Set<AIInteraction>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<GPPractice> GPPractices => Set<GPPractice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
