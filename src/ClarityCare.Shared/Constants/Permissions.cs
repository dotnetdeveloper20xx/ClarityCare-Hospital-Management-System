namespace ClarityCare.Shared.Constants;

public static class Permissions
{
    public const string PatientCreate = "Patient.Create";
    public const string PatientView = "Patient.View";
    public const string PatientEditContactDetails = "Patient.EditContactDetails";
    public const string PatientArchive = "Patient.Archive";
    public const string AppointmentBook = "Appointment.Book";
    public const string AppointmentView = "Appointment.View";
    public const string AppointmentCancel = "Appointment.Cancel";
    public const string AppointmentReschedule = "Appointment.Reschedule";
    public const string AppointmentMarkArrived = "Appointment.MarkArrived";
    public const string AppointmentOverrideAvailability = "Appointment.OverrideAvailability";
    public const string ConsultationStart = "Consultation.Start";
    public const string ConsultationView = "Consultation.View";
    public const string ConsultationAddNote = "Consultation.AddNote";
    public const string ConsultationComplete = "Consultation.Complete";
    public const string LabRequest = "Lab.Request";
    public const string LabView = "Lab.View";
    public const string LabEnterResult = "Lab.EnterResult";
    public const string LabReviewResult = "Lab.ReviewResult";
    public const string PharmacyView = "Pharmacy.View";
    public const string PharmacyApprovePrescription = "Pharmacy.ApprovePrescription";
    public const string PharmacyDispensePrescription = "Pharmacy.DispensePrescription";
    public const string InvoiceCreate = "Invoice.Create";
    public const string InvoiceIssue = "Invoice.Issue";
    public const string BillingView = "Billing.View";
    public const string PaymentRecord = "Payment.Record";
    public const string InsuranceClaimManage = "InsuranceClaim.Manage";
    public const string ReportView = "Report.View";
    public const string AuditLogView = "AuditLog.View";
    public const string AdminUserManage = "Admin.UserManage";
    public const string AdminRoleManage = "Admin.RoleManage";
    public const string AdminPermissionManage = "Admin.PermissionManage";
}
