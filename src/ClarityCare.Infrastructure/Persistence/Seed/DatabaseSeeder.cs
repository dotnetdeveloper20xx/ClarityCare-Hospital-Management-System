using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using ClarityCare.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ClarityCare.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        // === PHASE 1: Foundation (Permissions, Roles, Users, Departments) ===
        var permissions = GetPermissions();
        context.Permissions.AddRange(permissions);

        var roles = GetRoles();
        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();

        var rolePermissions = GetRolePermissions(roles, permissions);
        context.RolePermissions.AddRange(rolePermissions);

        var users = GetUsers();
        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        var userRoles = GetUserRoles(users, roles);
        context.UserRoles.AddRange(userRoles);

        var departments = GetDepartments();
        context.Departments.AddRange(departments);
        await context.SaveChangesAsync();

        // === PHASE 2: Clinicians, Rooms, Appointment Types ===
        var clinicians = GetClinicians(departments, users);
        context.Clinicians.AddRange(clinicians);

        var rooms = GetRooms(departments);
        context.Rooms.AddRange(rooms);

        var appointmentTypes = GetAppointmentTypes(departments);
        context.AppointmentTypes.AddRange(appointmentTypes);

        var availabilities = GetClinicianAvailabilities(clinicians);
        context.ClinicianAvailabilities.AddRange(availabilities);
        await context.SaveChangesAsync();

        // === PHASE 3: Patients with addresses, contacts, allergies ===
        var patients = GetPatients();
        context.Patients.AddRange(patients);
        await context.SaveChangesAsync();

        var addresses = GetPatientAddresses(patients);
        context.PatientAddresses.AddRange(addresses);

        var emergencyContacts = GetEmergencyContacts(patients);
        context.EmergencyContacts.AddRange(emergencyContacts);

        var allergies = GetPatientAllergies(patients);
        context.PatientAllergies.AddRange(allergies);
        await context.SaveChangesAsync();

        // === PHASE 4: Appointments (past, today, future) ===
        var appointments = GetAppointments(patients, clinicians, departments, rooms, appointmentTypes);
        context.Appointments.AddRange(appointments);
        await context.SaveChangesAsync();

        // === PHASE 5: Consultations & Clinical data ===
        var consultations = GetConsultations(appointments, patients, clinicians);
        context.Consultations.AddRange(consultations);
        await context.SaveChangesAsync();

        var observations = GetObservations(patients, consultations);
        context.Observations.AddRange(observations);

        var clinicalNotes = GetClinicalNotes(consultations, patients);
        context.ClinicalNotes.AddRange(clinicalNotes);

        var diagnoses = GetDiagnoses(consultations, patients);
        context.Diagnoses.AddRange(diagnoses);

        var carePlans = GetCarePlans(consultations, patients);
        context.CarePlans.AddRange(carePlans);
        await context.SaveChangesAsync();

        // === PHASE 6: Lab Requests & Results ===
        var labRequests = GetLabRequests(patients, consultations);
        context.LabRequests.AddRange(labRequests);
        await context.SaveChangesAsync();

        var labTestItems = GetLabTestItems(labRequests);
        context.LabTestItems.AddRange(labTestItems);

        var labResults = GetLabResults(labRequests, patients);
        context.LabResults.AddRange(labResults);
        await context.SaveChangesAsync();

        // === PHASE 7: Prescriptions ===
        var prescriptions = GetPrescriptions(patients, consultations);
        context.Prescriptions.AddRange(prescriptions);
        await context.SaveChangesAsync();

        var prescriptionItems = GetPrescriptionItems(prescriptions);
        context.PrescriptionItems.AddRange(prescriptionItems);
        await context.SaveChangesAsync();

        // === PHASE 8: Billing (Services, Invoices, Payments) ===
        var services = GetServiceCatalogues(departments);
        context.ServiceCatalogues.AddRange(services);
        await context.SaveChangesAsync();

        var invoices = GetInvoices(patients);
        context.Invoices.AddRange(invoices);
        await context.SaveChangesAsync();

        var invoiceItems = GetInvoiceItems(invoices, services);
        context.InvoiceItems.AddRange(invoiceItems);

        var payments = GetPayments(invoices, patients);
        context.Payments.AddRange(payments);
        await context.SaveChangesAsync();

        // === PHASE 9: Wards, Beds, Admissions ===
        var wards = GetWards(departments);
        context.Wards.AddRange(wards);
        await context.SaveChangesAsync();

        var beds = GetBeds(wards);
        context.Beds.AddRange(beds);
        await context.SaveChangesAsync();

        var admissions = GetAdmissions(patients, wards, beds);
        context.Admissions.AddRange(admissions);
        await context.SaveChangesAsync();

        // === PHASE 10: Medication Schedules, Nursing Tasks, Alerts ===
        var medSchedules = GetMedicationSchedules(patients, admissions);
        context.MedicationSchedules.AddRange(medSchedules);

        var nursingTasks = GetNursingTasks(patients, admissions, wards);
        context.NursingTasks.AddRange(nursingTasks);

        var alerts = GetClinicalAlerts(patients, admissions, wards);
        context.ClinicalAlerts.AddRange(alerts);
        await context.SaveChangesAsync();

        // === PHASE 11: Insurance Providers ===
        var insuranceProviders = GetInsuranceProviders();
        context.InsuranceProviders.AddRange(insuranceProviders);

        // === PHASE 12: Medication Catalog ===
        var medications = GetMedicationCatalog();
        context.MedicationCatalogs.AddRange(medications);

        // === PHASE 13: GP Practices ===
        var gpPractices = GetGPPractices();
        context.GPPractices.AddRange(gpPractices);

        await context.SaveChangesAsync();
    }

    // ============ PATIENTS ============
    private static List<Patient> GetPatients()
    {
        var now = DateTime.UtcNow;
        return new List<Patient>
        {
            new() { PatientId = Guid.NewGuid(), HospitalNumber = "HOSP-2026-000001", NhsNumber = "9876543210", FirstName = "James", LastName = "Morrison", DateOfBirth = new DateTime(1965, 3, 15), Gender = Gender.Male, Email = "james.morrison@email.com", PhoneNumber = "07700900001", Status = PatientStatus.Active, CreatedAt = now.AddDays(-90), CreatedBy = "System" },
            new() { PatientId = Guid.NewGuid(), HospitalNumber = "HOSP-2026-000002", NhsNumber = "9876543211", FirstName = "Margaret", LastName = "Williams", DateOfBirth = new DateTime(1948, 7, 22), Gender = Gender.Female, Email = "margaret.w@email.com", PhoneNumber = "07700900002", Status = PatientStatus.Active, CreatedAt = now.AddDays(-85), CreatedBy = "System" },
            new() { PatientId = Guid.NewGuid(), HospitalNumber = "HOSP-2026-000003", NhsNumber = "9876543212", FirstName = "Ahmed", LastName = "Khan", DateOfBirth = new DateTime(1980, 11, 5), Gender = Gender.Male, Email = "ahmed.khan@email.com", PhoneNumber = "07700900003", Status = PatientStatus.Active, CreatedAt = now.AddDays(-60), CreatedBy = "System" },
            new() { PatientId = Guid.NewGuid(), HospitalNumber = "HOSP-2026-000004", NhsNumber = "9876543213", FirstName = "Sophie", LastName = "Taylor", DateOfBirth = new DateTime(1992, 1, 30), Gender = Gender.Female, Email = "sophie.t@email.com", PhoneNumber = "07700900004", Status = PatientStatus.Active, CreatedAt = now.AddDays(-45), CreatedBy = "System" },
            new() { PatientId = Guid.NewGuid(), HospitalNumber = "HOSP-2026-000005", NhsNumber = "9876543214", FirstName = "Robert", LastName = "Davies", DateOfBirth = new DateTime(1955, 9, 12), Gender = Gender.Male, Email = "r.davies@email.com", PhoneNumber = "07700900005", Status = PatientStatus.Active, CreatedAt = now.AddDays(-30), CreatedBy = "System" },
            new() { PatientId = Guid.NewGuid(), HospitalNumber = "HOSP-2026-000006", NhsNumber = "9876543215", FirstName = "Priya", LastName = "Patel", DateOfBirth = new DateTime(1988, 4, 18), Gender = Gender.Female, Email = "priya.patel@email.com", PhoneNumber = "07700900006", Status = PatientStatus.Active, CreatedAt = now.AddDays(-20), CreatedBy = "System" },
            new() { PatientId = Guid.NewGuid(), HospitalNumber = "HOSP-2026-000007", NhsNumber = "9876543216", FirstName = "Thomas", LastName = "O'Brien", DateOfBirth = new DateTime(1972, 12, 3), Gender = Gender.Male, Email = "tom.obrien@email.com", PhoneNumber = "07700900007", Status = PatientStatus.Active, CreatedAt = now.AddDays(-15), CreatedBy = "System" },
            new() { PatientId = Guid.NewGuid(), HospitalNumber = "HOSP-2026-000008", NhsNumber = "9876543217", FirstName = "Emma", LastName = "Wilson", DateOfBirth = new DateTime(2015, 6, 25), Gender = Gender.Female, Email = "emma.parent@email.com", PhoneNumber = "07700900008", Status = PatientStatus.Active, CreatedAt = now.AddDays(-10), CreatedBy = "System" },
            new() { PatientId = Guid.NewGuid(), HospitalNumber = "HOSP-2026-000009", NhsNumber = "9876543218", FirstName = "George", LastName = "Thompson", DateOfBirth = new DateTime(1940, 2, 14), Gender = Gender.Male, Email = null, PhoneNumber = "07700900009", Status = PatientStatus.Active, CreatedAt = now.AddDays(-5), CreatedBy = "System" },
            new() { PatientId = Guid.NewGuid(), HospitalNumber = "HOSP-2026-000010", NhsNumber = "9876543219", FirstName = "Fatima", LastName = "Hassan", DateOfBirth = new DateTime(1995, 8, 7), Gender = Gender.Female, Email = "fatima.h@email.com", PhoneNumber = "07700900010", Status = PatientStatus.Archived, ArchivedAt = now.AddDays(-2), ArchivedBy = "admin@claritycare.local", ArchiveReason = "Patient moved abroad", CreatedAt = now.AddDays(-100), CreatedBy = "System" },
        };
    }

    private static List<PatientAddress> GetPatientAddresses(List<Patient> patients)
    {
        return patients.Where(p => p.Status == PatientStatus.Active).Select(p => new PatientAddress
        {
            PatientAddressId = Guid.NewGuid(), PatientId = p.PatientId, Line1 = $"{Random.Shared.Next(1, 200)} High Street", Town = "London", County = "Greater London", Postcode = $"SW{Random.Shared.Next(1, 20)} {Random.Shared.Next(1, 9)}AB", Country = "United Kingdom", IsPrimary = true, CreatedAt = p.CreatedAt, CreatedBy = "System"
        }).ToList();
    }

    private static List<EmergencyContact> GetEmergencyContacts(List<Patient> patients)
    {
        var relationships = new[] { "Spouse", "Parent", "Child", "Sibling", "Partner" };
        return patients.Where(p => p.Status == PatientStatus.Active).Select((p, i) => new EmergencyContact
        {
            EmergencyContactId = Guid.NewGuid(), PatientId = p.PatientId, FullName = $"Contact for {p.FirstName}", Relationship = relationships[i % relationships.Length], PhoneNumber = $"0770090{1000 + i}", IsPrimary = true, CreatedAt = p.CreatedAt, CreatedBy = "System"
        }).ToList();
    }

    private static List<PatientAllergy> GetPatientAllergies(List<Patient> patients)
    {
        var result = new List<PatientAllergy>();
        // Give first 5 patients allergies
        var allergyData = new[] {
            ("Penicillin", "Rash and hives", AllergySeverity.Severe),
            ("Aspirin", "Stomach upset", AllergySeverity.Moderate),
            ("Latex", "Skin irritation", AllergySeverity.Mild),
            ("Codeine", "Nausea and vomiting", AllergySeverity.Moderate),
            ("Ibuprofen", "Anaphylaxis", AllergySeverity.LifeThreatening),
        };
        for (int i = 0; i < 5 && i < patients.Count; i++)
        {
            result.Add(new PatientAllergy { PatientAllergyId = Guid.NewGuid(), PatientId = patients[i].PatientId, AllergyName = allergyData[i].Item1, Reaction = allergyData[i].Item2, Severity = allergyData[i].Item3, IsActive = true, RecordedAt = DateTime.UtcNow.AddDays(-30), RecordedBy = "doctor@claritycare.local" });
        }
        return result;
    }

    // ============ CLINICIANS & SCHEDULING ============
    private static List<Clinician> GetClinicians(List<Department> departments, List<User> users)
    {
        var doctor = users.First(u => u.Email == "doctor@claritycare.local");
        var genMed = departments.First(d => d.Name == "General Medicine");
        var cardio = departments.First(d => d.Name == "Cardiology");
        var ortho = departments.First(d => d.Name == "Orthopaedics");

        return new List<Clinician>
        {
            new() { ClinicianId = Guid.NewGuid(), UserId = doctor.UserId, DepartmentId = genMed.DepartmentId, FullName = "Dr. John Smith", JobTitle = "Consultant", Specialism = "General Practice", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { ClinicianId = Guid.NewGuid(), DepartmentId = cardio.DepartmentId, FullName = "Dr. Sarah Chen", JobTitle = "Consultant Cardiologist", Specialism = "Cardiology", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { ClinicianId = Guid.NewGuid(), DepartmentId = ortho.DepartmentId, FullName = "Mr. David Hughes", JobTitle = "Orthopaedic Surgeon", Specialism = "Orthopaedic Surgery", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { ClinicianId = Guid.NewGuid(), DepartmentId = genMed.DepartmentId, FullName = "Dr. Amira Osman", JobTitle = "Registrar", Specialism = "Internal Medicine", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        };
    }

    private static List<Room> GetRooms(List<Department> departments)
    {
        var result = new List<Room>();
        foreach (var dept in departments)
        {
            result.Add(new Room { RoomId = Guid.NewGuid(), DepartmentId = dept.DepartmentId, RoomName = $"{dept.Name.Substring(0, 3).ToUpper()}-01", RoomType = RoomType.ConsultationRoom, Location = "Ground Floor", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" });
            result.Add(new Room { RoomId = Guid.NewGuid(), DepartmentId = dept.DepartmentId, RoomName = $"{dept.Name.Substring(0, 3).ToUpper()}-02", RoomType = RoomType.ExaminationRoom, Location = "Ground Floor", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" });
        }
        return result;
    }

    private static List<AppointmentType> GetAppointmentTypes(List<Department> departments)
    {
        var result = new List<AppointmentType>();
        foreach (var dept in departments)
        {
            result.Add(new AppointmentType { AppointmentTypeId = Guid.NewGuid(), DepartmentId = dept.DepartmentId, Name = "New Patient", Description = "First consultation", DefaultDurationMinutes = 30, RequiresRoom = true, IsActive = true });
            result.Add(new AppointmentType { AppointmentTypeId = Guid.NewGuid(), DepartmentId = dept.DepartmentId, Name = "Follow-Up", Description = "Follow-up appointment", DefaultDurationMinutes = 15, RequiresRoom = true, IsActive = true });
            result.Add(new AppointmentType { AppointmentTypeId = Guid.NewGuid(), DepartmentId = dept.DepartmentId, Name = "Urgent", Description = "Urgent same-day", DefaultDurationMinutes = 20, RequiresRoom = true, RequiresPreparation = true, IsActive = true });
        }
        return result;
    }

    private static List<ClinicianAvailability> GetClinicianAvailabilities(List<Clinician> clinicians)
    {
        var result = new List<ClinicianAvailability>();
        foreach (var clinician in clinicians)
        {
            for (int day = 1; day <= 5; day++) // Mon-Fri
            {
                result.Add(new ClinicianAvailability { AvailabilityId = Guid.NewGuid(), ClinicianId = clinician.ClinicianId, DayOfWeek = day, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(12, 30, 0), IsAvailable = true, EffectiveFrom = DateTime.UtcNow.AddMonths(-6) });
                result.Add(new ClinicianAvailability { AvailabilityId = Guid.NewGuid(), ClinicianId = clinician.ClinicianId, DayOfWeek = day, StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsAvailable = true, EffectiveFrom = DateTime.UtcNow.AddMonths(-6) });
            }
        }
        return result;
    }

    // ============ APPOINTMENTS ============
    private static List<Appointment> GetAppointments(List<Patient> patients, List<Clinician> clinicians, List<Department> departments, List<Room> rooms, List<AppointmentType> appointmentTypes)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var genMed = departments.First(d => d.Name == "General Medicine");
        var clinician1 = clinicians[0];
        var clinician2 = clinicians[1];
        var room1 = rooms.First(r => r.DepartmentId == genMed.DepartmentId);
        var apptType = appointmentTypes.First(a => a.DepartmentId == genMed.DepartmentId && a.Name == "New Patient");
        var followUp = appointmentTypes.First(a => a.DepartmentId == genMed.DepartmentId && a.Name == "Follow-Up");

        return new List<Appointment>
        {
            // Today's appointments
            new() { AppointmentId = Guid.NewGuid(), PatientId = patients[0].PatientId, ClinicianId = clinician1.ClinicianId, DepartmentId = genMed.DepartmentId, RoomId = room1.RoomId, AppointmentTypeId = apptType.AppointmentTypeId, StartTime = today.AddHours(9), EndTime = today.AddHours(9).AddMinutes(30), Status = AppointmentStatus.Completed, Priority = AppointmentPriority.Normal, ReasonForVisit = "Chest pain review", CreatedAt = now.AddDays(-7), CreatedBy = "reception@claritycare.local", CompletedAt = today.AddHours(9).AddMinutes(25) },
            new() { AppointmentId = Guid.NewGuid(), PatientId = patients[1].PatientId, ClinicianId = clinician1.ClinicianId, DepartmentId = genMed.DepartmentId, RoomId = room1.RoomId, AppointmentTypeId = followUp.AppointmentTypeId, StartTime = today.AddHours(9).AddMinutes(30), EndTime = today.AddHours(9).AddMinutes(45), Status = AppointmentStatus.Arrived, Priority = AppointmentPriority.Normal, ReasonForVisit = "Blood pressure follow-up", ArrivedAt = today.AddHours(9).AddMinutes(20), CreatedAt = now.AddDays(-5), CreatedBy = "reception@claritycare.local" },
            new() { AppointmentId = Guid.NewGuid(), PatientId = patients[2].PatientId, ClinicianId = clinician1.ClinicianId, DepartmentId = genMed.DepartmentId, RoomId = room1.RoomId, AppointmentTypeId = apptType.AppointmentTypeId, StartTime = today.AddHours(10), EndTime = today.AddHours(10).AddMinutes(30), Status = AppointmentStatus.Booked, Priority = AppointmentPriority.Normal, ReasonForVisit = "Persistent cough - 3 weeks", CreatedAt = now.AddDays(-3), CreatedBy = "reception@claritycare.local" },
            new() { AppointmentId = Guid.NewGuid(), PatientId = patients[3].PatientId, ClinicianId = clinician1.ClinicianId, DepartmentId = genMed.DepartmentId, RoomId = room1.RoomId, AppointmentTypeId = apptType.AppointmentTypeId, StartTime = today.AddHours(10).AddMinutes(30), EndTime = today.AddHours(11), Status = AppointmentStatus.Booked, Priority = AppointmentPriority.Urgent, ReasonForVisit = "Severe headaches", CreatedAt = now.AddDays(-1), CreatedBy = "reception@claritycare.local" },
            new() { AppointmentId = Guid.NewGuid(), PatientId = patients[4].PatientId, ClinicianId = clinician1.ClinicianId, DepartmentId = genMed.DepartmentId, RoomId = room1.RoomId, AppointmentTypeId = followUp.AppointmentTypeId, StartTime = today.AddHours(11), EndTime = today.AddHours(11).AddMinutes(15), Status = AppointmentStatus.Booked, Priority = AppointmentPriority.Normal, ReasonForVisit = "Diabetes review", CreatedAt = now.AddDays(-10), CreatedBy = "reception@claritycare.local" },
            new() { AppointmentId = Guid.NewGuid(), PatientId = patients[5].PatientId, ClinicianId = clinician1.ClinicianId, DepartmentId = genMed.DepartmentId, RoomId = room1.RoomId, AppointmentTypeId = apptType.AppointmentTypeId, StartTime = today.AddHours(14), EndTime = today.AddHours(14).AddMinutes(30), Status = AppointmentStatus.Booked, Priority = AppointmentPriority.Normal, ReasonForVisit = "Abdominal pain", CreatedAt = now.AddDays(-2), CreatedBy = "reception@claritycare.local" },
            // Past completed appointments
            new() { AppointmentId = Guid.NewGuid(), PatientId = patients[0].PatientId, ClinicianId = clinician1.ClinicianId, DepartmentId = genMed.DepartmentId, RoomId = room1.RoomId, AppointmentTypeId = apptType.AppointmentTypeId, StartTime = now.AddDays(-14).Date.AddHours(9), EndTime = now.AddDays(-14).Date.AddHours(9).AddMinutes(30), Status = AppointmentStatus.Completed, Priority = AppointmentPriority.Normal, ReasonForVisit = "Initial chest pain assessment", CreatedAt = now.AddDays(-21), CreatedBy = "reception@claritycare.local", CompletedAt = now.AddDays(-14).Date.AddHours(9).AddMinutes(28) },
            new() { AppointmentId = Guid.NewGuid(), PatientId = patients[6].PatientId, ClinicianId = clinician2.ClinicianId, DepartmentId = clinician2.DepartmentId, AppointmentTypeId = appointmentTypes.First(a => a.DepartmentId == clinician2.DepartmentId).AppointmentTypeId, StartTime = now.AddDays(-7).Date.AddHours(10), EndTime = now.AddDays(-7).Date.AddHours(10).AddMinutes(30), Status = AppointmentStatus.Completed, Priority = AppointmentPriority.Normal, ReasonForVisit = "Heart palpitations", CreatedAt = now.AddDays(-14), CreatedBy = "reception@claritycare.local", CompletedAt = now.AddDays(-7).Date.AddHours(10).AddMinutes(25) },
            // Future appointments
            new() { AppointmentId = Guid.NewGuid(), PatientId = patients[7].PatientId, ClinicianId = clinician1.ClinicianId, DepartmentId = genMed.DepartmentId, RoomId = room1.RoomId, AppointmentTypeId = apptType.AppointmentTypeId, StartTime = now.AddDays(3).Date.AddHours(9), EndTime = now.AddDays(3).Date.AddHours(9).AddMinutes(30), Status = AppointmentStatus.Booked, Priority = AppointmentPriority.Normal, ReasonForVisit = "Growth check", CreatedAt = now.AddDays(-1), CreatedBy = "reception@claritycare.local" },
            new() { AppointmentId = Guid.NewGuid(), PatientId = patients[8].PatientId, ClinicianId = clinician1.ClinicianId, DepartmentId = genMed.DepartmentId, RoomId = room1.RoomId, AppointmentTypeId = followUp.AppointmentTypeId, StartTime = now.AddDays(5).Date.AddHours(14), EndTime = now.AddDays(5).Date.AddHours(14).AddMinutes(15), Status = AppointmentStatus.Booked, Priority = AppointmentPriority.Normal, ReasonForVisit = "Mobility assessment", CreatedAt = now, CreatedBy = "reception@claritycare.local" },
            // Cancelled / No-show
            new() { AppointmentId = Guid.NewGuid(), PatientId = patients[4].PatientId, ClinicianId = clinician1.ClinicianId, DepartmentId = genMed.DepartmentId, AppointmentTypeId = followUp.AppointmentTypeId, StartTime = now.AddDays(-3).Date.AddHours(11), EndTime = now.AddDays(-3).Date.AddHours(11).AddMinutes(15), Status = AppointmentStatus.NoShow, Priority = AppointmentPriority.Normal, ReasonForVisit = "Routine check", MarkedNoShowAt = now.AddDays(-3).Date.AddHours(11).AddMinutes(20), CreatedAt = now.AddDays(-10), CreatedBy = "reception@claritycare.local" },
        };
    }

    // ============ CONSULTATIONS ============
    private static List<Consultation> GetConsultations(List<Appointment> appointments, List<Patient> patients, List<Clinician> clinicians)
    {
        var completed = appointments.Where(a => a.Status == AppointmentStatus.Completed).ToList();
        return completed.Select(a => new Consultation
        {
            ConsultationId = Guid.NewGuid(), AppointmentId = a.AppointmentId, PatientId = a.PatientId, ClinicianId = a.ClinicianId, StartedAt = a.StartTime, StartedBy = "doctor@claritycare.local", CompletedAt = a.CompletedAt, CompletedBy = "doctor@claritycare.local", Status = ConsultationStatus.Completed, Summary = $"Consultation completed for {a.ReasonForVisit}", LockedAt = a.CompletedAt, CreatedAt = a.StartTime
        }).ToList();
    }

    // ============ OBSERVATIONS ============
    private static List<Observation> GetObservations(List<Patient> patients, List<Consultation> consultations)
    {
        return consultations.Select(c => new Observation
        {
            ObservationId = Guid.NewGuid(), PatientId = c.PatientId, ConsultationId = c.ConsultationId, RecordedBy = "nurse@claritycare.local", RecordedAt = c.StartedAt.AddMinutes(-5), BloodPressureSystolic = Random.Shared.Next(110, 160), BloodPressureDiastolic = Random.Shared.Next(65, 95), Pulse = Random.Shared.Next(60, 100), Temperature = 36.5m + (decimal)(Random.Shared.NextDouble() * 1.5), OxygenSaturation = Random.Shared.Next(95, 100), RespiratoryRate = Random.Shared.Next(12, 20), Weight = 60m + (decimal)(Random.Shared.Next(0, 40)), Height = 155m + (decimal)(Random.Shared.Next(0, 35))
        }).ToList();
    }

    // ============ CLINICAL NOTES ============
    private static List<ClinicalNote> GetClinicalNotes(List<Consultation> consultations, List<Patient> patients)
    {
        var notes = new List<ClinicalNote>();
        foreach (var c in consultations)
        {
            notes.Add(new ClinicalNote { ClinicalNoteId = Guid.NewGuid(), ConsultationId = c.ConsultationId, PatientId = c.PatientId, NoteType = ClinicalNoteType.Symptoms, NoteText = "Patient presents with reported symptoms. Duration approximately 2 weeks. No associated features.", CreatedBy = "doctor@claritycare.local", CreatedAt = c.StartedAt.AddMinutes(5), IsLocked = true });
            notes.Add(new ClinicalNote { ClinicalNoteId = Guid.NewGuid(), ConsultationId = c.ConsultationId, PatientId = c.PatientId, NoteType = ClinicalNoteType.Examination, NoteText = "General examination unremarkable. No tenderness on palpation. Cardiovascular and respiratory systems normal.", CreatedBy = "doctor@claritycare.local", CreatedAt = c.StartedAt.AddMinutes(10), IsLocked = true });
        }
        return notes;
    }

    // ============ DIAGNOSES ============
    private static List<Diagnosis> GetDiagnoses(List<Consultation> consultations, List<Patient> patients)
    {
        var diagnosisData = new[] { ("J06.9", "Acute upper respiratory infection"), ("I10", "Essential hypertension"), ("R07.9", "Chest pain, unspecified"), ("E11.9", "Type 2 diabetes mellitus") };
        return consultations.Select((c, i) => new Diagnosis
        {
            DiagnosisId = Guid.NewGuid(), ConsultationId = c.ConsultationId, PatientId = c.PatientId, DiagnosisCode = diagnosisData[i % diagnosisData.Length].Item1, DiagnosisDescription = diagnosisData[i % diagnosisData.Length].Item2, IsPrimary = true, CreatedBy = "doctor@claritycare.local", CreatedAt = c.StartedAt.AddMinutes(15)
        }).ToList();
    }

    // ============ CARE PLANS ============
    private static List<CarePlan> GetCarePlans(List<Consultation> consultations, List<Patient> patients)
    {
        return consultations.Select(c => new CarePlan
        {
            CarePlanId = Guid.NewGuid(), ConsultationId = c.ConsultationId, PatientId = c.PatientId, PlanSummary = "Continue current management. Review in 2 weeks.", AdviceGiven = "Rest, adequate fluids, paracetamol as needed. Return if symptoms worsen.", FollowUpRequired = true, FollowUpPeriodDays = 14, CreatedBy = "doctor@claritycare.local", CreatedAt = c.StartedAt.AddMinutes(20)
        }).ToList();
    }

    // ============ LAB REQUESTS ============
    private static List<LabRequest> GetLabRequests(List<Patient> patients, List<Consultation> consultations)
    {
        var result = new List<LabRequest>();
        if (consultations.Count >= 2)
        {
            result.Add(new LabRequest { LabRequestId = Guid.NewGuid(), PatientId = consultations[0].PatientId, ConsultationId = consultations[0].ConsultationId, RequestedBy = "doctor@claritycare.local", Priority = LabPriority.Normal, Status = LabRequestStatus.Completed, ClinicalReason = "Routine bloods for chest pain workup", RequestedAt = DateTime.UtcNow.AddDays(-14), CompletedAt = DateTime.UtcNow.AddDays(-13) });
            result.Add(new LabRequest { LabRequestId = Guid.NewGuid(), PatientId = consultations[1].PatientId, ConsultationId = consultations[1].ConsultationId, RequestedBy = "doctor@claritycare.local", Priority = LabPriority.Urgent, Status = LabRequestStatus.Processing, ClinicalReason = "Cardiac enzymes - query MI", RequestedAt = DateTime.UtcNow.AddDays(-1) });
        }
        // Add a pending request for today
        if (patients.Count >= 3)
        {
            result.Add(new LabRequest { LabRequestId = Guid.NewGuid(), PatientId = patients[2].PatientId, ConsultationId = consultations.FirstOrDefault()?.ConsultationId ?? Guid.Empty, RequestedBy = "doctor@claritycare.local", Priority = LabPriority.Normal, Status = LabRequestStatus.Requested, ClinicalReason = "FBC and CRP for persistent cough", RequestedAt = DateTime.UtcNow });
        }
        return result;
    }

    private static List<LabTestItem> GetLabTestItems(List<LabRequest> labRequests)
    {
        var result = new List<LabTestItem>();
        var tests = new[] { ("FBC", "Full Blood Count"), ("U&E", "Urea & Electrolytes"), ("LFT", "Liver Function Tests"), ("CRP", "C-Reactive Protein"), ("TropT", "Troponin T") };
        foreach (var req in labRequests)
        {
            var testCount = req.Priority == LabPriority.Urgent ? 3 : 2;
            for (int i = 0; i < testCount && i < tests.Length; i++)
            {
                result.Add(new LabTestItem { LabTestItemId = Guid.NewGuid(), LabRequestId = req.LabRequestId, TestCode = tests[i].Item1, TestName = tests[i].Item2, Status = req.Status == LabRequestStatus.Completed ? LabTestItemStatus.Completed : LabTestItemStatus.Pending });
            }
        }
        return result;
    }

    private static List<LabResult> GetLabResults(List<LabRequest> labRequests, List<Patient> patients)
    {
        var completed = labRequests.Where(r => r.Status == LabRequestStatus.Completed).ToList();
        return completed.Select(r => new LabResult
        {
            LabResultId = Guid.NewGuid(), LabRequestId = r.LabRequestId, PatientId = r.PatientId, ResultSummary = "Within normal limits", ResultValue = "Hb 140 g/L, WBC 7.2, Plt 250", Unit = "g/L", ReferenceRange = "130-170", IsAbnormal = false, IsCritical = false, RecordedBy = "lab@claritycare.local", RecordedAt = r.CompletedAt ?? DateTime.UtcNow
        }).ToList();
    }

    // ============ PRESCRIPTIONS ============
    private static List<Prescription> GetPrescriptions(List<Patient> patients, List<Consultation> consultations)
    {
        var result = new List<Prescription>();
        var now = DateTime.UtcNow;
        // Dispensed prescriptions (completed)
        foreach (var c in consultations.Take(2))
        {
            result.Add(new Prescription { PrescriptionId = Guid.NewGuid(), PatientId = c.PatientId, ConsultationId = c.ConsultationId, PrescribedBy = "doctor@claritycare.local", Status = PrescriptionStatus.Dispensed, CreatedAt = c.StartedAt.AddMinutes(18) });
        }
        // Multiple pending review prescriptions for pharmacy dashboard
        if (consultations.Count > 0)
        {
            result.Add(new Prescription { PrescriptionId = Guid.NewGuid(), PatientId = patients[2].PatientId, ConsultationId = consultations[0].ConsultationId, PrescribedBy = "doctor@claritycare.local", Status = PrescriptionStatus.Submitted, CreatedAt = now.AddHours(-2) });
            result.Add(new Prescription { PrescriptionId = Guid.NewGuid(), PatientId = patients[3].PatientId, ConsultationId = consultations[0].ConsultationId, PrescribedBy = "doctor@claritycare.local", Status = PrescriptionStatus.Submitted, CreatedAt = now.AddHours(-1) });
            result.Add(new Prescription { PrescriptionId = Guid.NewGuid(), PatientId = patients[5].PatientId, ConsultationId = consultations[0].ConsultationId, PrescribedBy = "doctor@claritycare.local", Status = PrescriptionStatus.Submitted, CreatedAt = now.AddMinutes(-30) });
            result.Add(new Prescription { PrescriptionId = Guid.NewGuid(), PatientId = patients[6].PatientId, ConsultationId = consultations[0].ConsultationId, PrescribedBy = "doctor@claritycare.local", Status = PrescriptionStatus.Approved, CreatedAt = now.AddHours(-4) });
            result.Add(new Prescription { PrescriptionId = Guid.NewGuid(), PatientId = patients[7].PatientId, ConsultationId = consultations[0].ConsultationId, PrescribedBy = "doctor@claritycare.local", Status = PrescriptionStatus.Approved, CreatedAt = now.AddHours(-3) });
        }
        return result;
    }

    private static List<PrescriptionItem> GetPrescriptionItems(List<Prescription> prescriptions)
    {
        var meds = new[] { ("Amoxicillin 500mg", "500mg", "Three times daily", 7, "Take with food"), ("Paracetamol 500mg", "1g", "Four times daily", 5, "Maximum 4g per day"), ("Omeprazole 20mg", "20mg", "Once daily", 28, "Take before breakfast"), ("Ramipril 5mg", "5mg", "Once daily", 28, "Monitor BP") };
        var result = new List<PrescriptionItem>();
        foreach (var rx in prescriptions)
        {
            var count = rx.Status == PrescriptionStatus.Dispensed ? 2 : 1;
            for (int i = 0; i < count; i++)
            {
                var med = meds[(prescriptions.IndexOf(rx) + i) % meds.Length];
                result.Add(new PrescriptionItem { PrescriptionItemId = Guid.NewGuid(), PrescriptionId = rx.PrescriptionId, MedicationName = med.Item1, Dosage = med.Item2, Frequency = med.Item3, DurationDays = med.Item4, Instructions = med.Item5 });
            }
        }
        return result;
    }

    // ============ BILLING ============
    private static List<ServiceCatalogue> GetServiceCatalogues(List<Department> departments)
    {
        return new List<ServiceCatalogue>
        {
            new() { ServiceId = Guid.NewGuid(), ServiceCode = "CONS-NEW", ServiceName = "New Patient Consultation", DepartmentId = departments[0].DepartmentId, Price = 150.00m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { ServiceId = Guid.NewGuid(), ServiceCode = "CONS-FU", ServiceName = "Follow-Up Consultation", DepartmentId = departments[0].DepartmentId, Price = 85.00m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { ServiceId = Guid.NewGuid(), ServiceCode = "LAB-FBC", ServiceName = "Full Blood Count", Price = 25.00m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { ServiceId = Guid.NewGuid(), ServiceCode = "LAB-BIO", ServiceName = "Biochemistry Panel", Price = 45.00m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { ServiceId = Guid.NewGuid(), ServiceCode = "XRAY-CHE", ServiceName = "Chest X-Ray", Price = 75.00m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { ServiceId = Guid.NewGuid(), ServiceCode = "ECG-STD", ServiceName = "Standard ECG", DepartmentId = departments[1].DepartmentId, Price = 60.00m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { ServiceId = Guid.NewGuid(), ServiceCode = "PHARM-DIS", ServiceName = "Pharmacy Dispensing Fee", Price = 12.50m, IsActive = true, CreatedAt = DateTime.UtcNow },
        };
    }

    private static List<Invoice> GetInvoices(List<Patient> patients)
    {
        var now = DateTime.UtcNow;
        return new List<Invoice>
        {
            new() { InvoiceId = Guid.NewGuid(), PatientId = patients[0].PatientId, InvoiceNumber = "INV-2026-000001", InvoiceDate = now.AddDays(-14), Status = InvoiceStatus.Paid, Subtotal = 175.00m, Tax = 0m, TotalAmount = 175.00m, BalanceDue = 0m, CreatedAt = now.AddDays(-14) },
            new() { InvoiceId = Guid.NewGuid(), PatientId = patients[1].PatientId, InvoiceNumber = "INV-2026-000002", InvoiceDate = now.AddDays(-7), Status = InvoiceStatus.Issued, Subtotal = 235.00m, Tax = 0m, TotalAmount = 235.00m, BalanceDue = 235.00m, CreatedAt = now.AddDays(-7) },
            new() { InvoiceId = Guid.NewGuid(), PatientId = patients[2].PatientId, InvoiceNumber = "INV-2026-000003", InvoiceDate = now.AddDays(-30), Status = InvoiceStatus.Overdue, Subtotal = 150.00m, Tax = 0m, TotalAmount = 150.00m, BalanceDue = 150.00m, CreatedAt = now.AddDays(-30) },
            new() { InvoiceId = Guid.NewGuid(), PatientId = patients[3].PatientId, InvoiceNumber = "INV-2026-000004", InvoiceDate = now.AddDays(-3), Status = InvoiceStatus.PartiallyPaid, Subtotal = 310.00m, Tax = 0m, TotalAmount = 310.00m, BalanceDue = 160.00m, CreatedAt = now.AddDays(-3) },
            new() { InvoiceId = Guid.NewGuid(), PatientId = patients[4].PatientId, InvoiceNumber = "INV-2026-000005", InvoiceDate = now.AddDays(-2), Status = InvoiceStatus.Issued, Subtotal = 85.00m, Tax = 0m, TotalAmount = 85.00m, BalanceDue = 85.00m, CreatedAt = now.AddDays(-2) },
            new() { InvoiceId = Guid.NewGuid(), PatientId = patients[5].PatientId, InvoiceNumber = "INV-2026-000006", InvoiceDate = now.AddDays(-1), Status = InvoiceStatus.Paid, Subtotal = 210.00m, Tax = 0m, TotalAmount = 210.00m, BalanceDue = 0m, CreatedAt = now.AddDays(-1) },
            new() { InvoiceId = Guid.NewGuid(), PatientId = patients[6].PatientId, InvoiceNumber = "INV-2026-000007", InvoiceDate = now.AddDays(-21), Status = InvoiceStatus.Overdue, Subtotal = 445.00m, Tax = 0m, TotalAmount = 445.00m, BalanceDue = 445.00m, CreatedAt = now.AddDays(-21) },
            new() { InvoiceId = Guid.NewGuid(), PatientId = patients[7].PatientId, InvoiceNumber = "INV-2026-000008", InvoiceDate = now, Status = InvoiceStatus.Draft, Subtotal = 120.00m, Tax = 0m, TotalAmount = 120.00m, BalanceDue = 120.00m, CreatedAt = now },
        };
    }

    private static List<InvoiceItem> GetInvoiceItems(List<Invoice> invoices, List<ServiceCatalogue> services)
    {
        var result = new List<InvoiceItem>();
        var consNew = services.First(s => s.ServiceCode == "CONS-NEW");
        var labFbc = services.First(s => s.ServiceCode == "LAB-FBC");
        var consFu = services.First(s => s.ServiceCode == "CONS-FU");

        result.Add(new InvoiceItem { InvoiceItemId = Guid.NewGuid(), InvoiceId = invoices[0].InvoiceId, ServiceId = consNew.ServiceId, Description = consNew.ServiceName, Quantity = 1, UnitPrice = 150.00m, LineTotal = 150.00m });
        result.Add(new InvoiceItem { InvoiceItemId = Guid.NewGuid(), InvoiceId = invoices[0].InvoiceId, ServiceId = labFbc.ServiceId, Description = labFbc.ServiceName, Quantity = 1, UnitPrice = 25.00m, LineTotal = 25.00m });
        result.Add(new InvoiceItem { InvoiceItemId = Guid.NewGuid(), InvoiceId = invoices[1].InvoiceId, ServiceId = consNew.ServiceId, Description = consNew.ServiceName, Quantity = 1, UnitPrice = 150.00m, LineTotal = 150.00m });
        result.Add(new InvoiceItem { InvoiceItemId = Guid.NewGuid(), InvoiceId = invoices[1].InvoiceId, ServiceId = consFu.ServiceId, Description = consFu.ServiceName, Quantity = 1, UnitPrice = 85.00m, LineTotal = 85.00m });
        result.Add(new InvoiceItem { InvoiceItemId = Guid.NewGuid(), InvoiceId = invoices[2].InvoiceId, ServiceId = consNew.ServiceId, Description = consNew.ServiceName, Quantity = 1, UnitPrice = 150.00m, LineTotal = 150.00m });
        result.Add(new InvoiceItem { InvoiceItemId = Guid.NewGuid(), InvoiceId = invoices[3].InvoiceId, ServiceId = consNew.ServiceId, Description = consNew.ServiceName, Quantity = 1, UnitPrice = 150.00m, LineTotal = 150.00m });
        result.Add(new InvoiceItem { InvoiceItemId = Guid.NewGuid(), InvoiceId = invoices[3].InvoiceId, ServiceId = services.First(s => s.ServiceCode == "ECG-STD").ServiceId, Description = "Standard ECG", Quantity = 1, UnitPrice = 60.00m, LineTotal = 60.00m });
        return result;
    }

    private static List<Payment> GetPayments(List<Invoice> invoices, List<Patient> patients)
    {
        return new List<Payment>
        {
            new() { PaymentId = Guid.NewGuid(), InvoiceId = invoices[0].InvoiceId, PatientId = invoices[0].PatientId, PaymentMethod = PaymentMethod.Card, Amount = 175.00m, PaymentDate = DateTime.UtcNow.AddDays(-13), ReferenceNumber = "PAY-001", Status = PaymentStatus.Completed },
            new() { PaymentId = Guid.NewGuid(), InvoiceId = invoices[3].InvoiceId, PatientId = invoices[3].PatientId, PaymentMethod = PaymentMethod.Cash, Amount = 150.00m, PaymentDate = DateTime.UtcNow.AddDays(-2), ReferenceNumber = "PAY-002", Status = PaymentStatus.Completed },
        };
    }

    // ============ WARDS & BEDS ============
    private static List<Ward> GetWards(List<Department> departments)
    {
        return new List<Ward>
        {
            new() { WardId = Guid.NewGuid(), DepartmentId = departments[0].DepartmentId, Name = "Maple Ward", WardType = WardType.General, Location = "Building A, Floor 2", Capacity = 20, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { WardId = Guid.NewGuid(), DepartmentId = departments[1].DepartmentId, Name = "Cedar Ward", WardType = WardType.Cardiology, Location = "Building A, Floor 3", Capacity = 12, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { WardId = Guid.NewGuid(), DepartmentId = departments[0].DepartmentId, Name = "ICU", WardType = WardType.IntensiveCare, Location = "Building B, Floor 1", Capacity = 8, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        };
    }

    private static List<Bed> GetBeds(List<Ward> wards)
    {
        var result = new List<Bed>();
        foreach (var ward in wards)
        {
            var count = ward.WardType == WardType.IntensiveCare ? 8 : ward.Capacity > 12 ? 12 : ward.Capacity;
            for (int i = 1; i <= count; i++)
            {
                var status = i <= 3 ? BedStatus.Occupied : i == 4 ? BedStatus.Cleaning : i == 5 ? BedStatus.Reserved : BedStatus.Available;
                result.Add(new Bed { BedId = Guid.NewGuid(), WardId = ward.WardId, BedNumber = $"{ward.Name.Substring(0, 3).ToUpper()}-{i:D2}", BedType = ward.WardType == WardType.IntensiveCare ? BedType.ICU : BedType.Standard, Status = status, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" });
            }
        }
        return result;
    }

    // ============ ADMISSIONS ============
    private static List<Admission> GetAdmissions(List<Patient> patients, List<Ward> wards, List<Bed> beds)
    {
        var mapleWard = wards.First(w => w.Name == "Maple Ward");
        var mapleBeds = beds.Where(b => b.WardId == mapleWard.WardId && b.Status == BedStatus.Occupied).Take(2).ToList();
        var result = new List<Admission>();
        if (mapleBeds.Count >= 2)
        {
            result.Add(new Admission { AdmissionId = Guid.NewGuid(), PatientId = patients[4].PatientId, RequestedBy = "doctor@claritycare.local", AdmittedBy = "nurse@claritycare.local", WardId = mapleWard.WardId, BedId = mapleBeds[0].BedId, AdmissionReason = "Poorly controlled diabetes requiring IV insulin", AdmissionPriority = AdmissionPriority.Urgent, Status = AdmissionStatus.Admitted, RequestedAt = DateTime.UtcNow.AddDays(-3), AdmittedAt = DateTime.UtcNow.AddDays(-3), ExpectedDischargeDate = DateTime.UtcNow.AddDays(2), CreatedAt = DateTime.UtcNow.AddDays(-3) });
            result.Add(new Admission { AdmissionId = Guid.NewGuid(), PatientId = patients[6].PatientId, RequestedBy = "doctor@claritycare.local", AdmittedBy = "nurse@claritycare.local", WardId = mapleWard.WardId, BedId = mapleBeds[1].BedId, AdmissionReason = "Post-operative monitoring following cardiac procedure", AdmissionPriority = AdmissionPriority.Normal, Status = AdmissionStatus.Admitted, RequestedAt = DateTime.UtcNow.AddDays(-1), AdmittedAt = DateTime.UtcNow.AddDays(-1), ExpectedDischargeDate = DateTime.UtcNow.AddDays(4), CreatedAt = DateTime.UtcNow.AddDays(-1) });
        }
        // Pending admission request
        result.Add(new Admission { AdmissionId = Guid.NewGuid(), PatientId = patients[8].PatientId, RequestedBy = "doctor@claritycare.local", AdmissionReason = "Falls assessment - requires 48hr observation", AdmissionPriority = AdmissionPriority.Normal, Status = AdmissionStatus.Requested, RequestedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
        return result;
    }

    // ============ MEDICATION SCHEDULES ============
    private static List<MedicationSchedule> GetMedicationSchedules(List<Patient> patients, List<Admission> admissions)
    {
        var admitted = admissions.Where(a => a.Status == AdmissionStatus.Admitted).ToList();
        var result = new List<MedicationSchedule>();
        if (admitted.Count > 0)
        {
            result.Add(new MedicationSchedule { MedicationScheduleId = Guid.NewGuid(), PatientId = admitted[0].PatientId, AdmissionId = admitted[0].AdmissionId, MedicationName = "Insulin Novorapid", Dose = "10 units", Route = "Subcutaneous", Frequency = "Three times daily with meals", StartDateTime = DateTime.UtcNow.AddDays(-3), NextDueAt = DateTime.UtcNow.AddHours(2), Status = MedicationScheduleStatus.Active, IsHighRisk = true, RequiresSecondCheck = true, CreatedAt = DateTime.UtcNow.AddDays(-3), CreatedBy = "doctor@claritycare.local" });
            result.Add(new MedicationSchedule { MedicationScheduleId = Guid.NewGuid(), PatientId = admitted[0].PatientId, AdmissionId = admitted[0].AdmissionId, MedicationName = "Metformin 500mg", Dose = "500mg", Route = "Oral", Frequency = "Twice daily", StartDateTime = DateTime.UtcNow.AddDays(-3), NextDueAt = DateTime.UtcNow.AddHours(4), Status = MedicationScheduleStatus.Active, IsHighRisk = false, RequiresSecondCheck = false, CreatedAt = DateTime.UtcNow.AddDays(-3), CreatedBy = "doctor@claritycare.local" });
        }
        if (admitted.Count > 1)
        {
            result.Add(new MedicationSchedule { MedicationScheduleId = Guid.NewGuid(), PatientId = admitted[1].PatientId, AdmissionId = admitted[1].AdmissionId, MedicationName = "Enoxaparin 40mg", Dose = "40mg", Route = "Subcutaneous", Frequency = "Once daily", StartDateTime = DateTime.UtcNow.AddDays(-1), NextDueAt = DateTime.UtcNow.AddHours(6), Status = MedicationScheduleStatus.Active, IsHighRisk = true, RequiresSecondCheck = true, CreatedAt = DateTime.UtcNow.AddDays(-1), CreatedBy = "doctor@claritycare.local" });
        }
        return result;
    }

    // ============ NURSING TASKS ============
    private static List<NursingTask> GetNursingTasks(List<Patient> patients, List<Admission> admissions, List<Ward> wards)
    {
        var admitted = admissions.Where(a => a.Status == AdmissionStatus.Admitted).ToList();
        var result = new List<NursingTask>();
        if (admitted.Count > 0 && wards.Count > 0)
        {
            var ward = wards[0];
            result.Add(new NursingTask { NursingTaskId = Guid.NewGuid(), PatientId = admitted[0].PatientId, AdmissionId = admitted[0].AdmissionId, WardId = ward.WardId, TaskType = "Observations", Title = "4-hourly observations due", Priority = NursingTaskPriority.High, DueAt = DateTime.UtcNow.AddHours(1), Status = NursingTaskStatus.Pending, CreatedAt = DateTime.UtcNow.AddHours(-3), CreatedBy = "nurse@claritycare.local" });
            result.Add(new NursingTask { NursingTaskId = Guid.NewGuid(), PatientId = admitted[0].PatientId, AdmissionId = admitted[0].AdmissionId, WardId = ward.WardId, TaskType = "Blood Glucose", Title = "Pre-meal blood glucose check", Priority = NursingTaskPriority.Urgent, DueAt = DateTime.UtcNow.AddMinutes(30), Status = NursingTaskStatus.Pending, CreatedAt = DateTime.UtcNow.AddHours(-1), CreatedBy = "nurse@claritycare.local" });
            result.Add(new NursingTask { NursingTaskId = Guid.NewGuid(), PatientId = admitted[0].PatientId, AdmissionId = admitted[0].AdmissionId, WardId = ward.WardId, TaskType = "Wound Care", Title = "Cannula site check and flush", Priority = NursingTaskPriority.Normal, DueAt = DateTime.UtcNow.AddHours(3), Status = NursingTaskStatus.Pending, CreatedAt = DateTime.UtcNow, CreatedBy = "nurse@claritycare.local" });
        }
        return result;
    }

    // ============ CLINICAL ALERTS ============
    private static List<ClinicalAlert> GetClinicalAlerts(List<Patient> patients, List<Admission> admissions, List<Ward> wards)
    {
        var admitted = admissions.Where(a => a.Status == AdmissionStatus.Admitted).ToList();
        var result = new List<ClinicalAlert>();
        if (admitted.Count > 0 && wards.Count > 0)
        {
            result.Add(new ClinicalAlert { AlertId = Guid.NewGuid(), PatientId = admitted[0].PatientId, AdmissionId = admitted[0].AdmissionId, WardId = wards[0].WardId, AlertType = ClinicalAlertType.AbnormalObservation, Severity = ClinicalAlertSeverity.Warning, Message = "Blood glucose reading 18.5 mmol/L - above target range", Status = ClinicalAlertStatus.Open, CreatedAt = DateTime.UtcNow.AddHours(-2), CreatedBy = "System" });
            result.Add(new ClinicalAlert { AlertId = Guid.NewGuid(), PatientId = admitted[0].PatientId, AdmissionId = admitted[0].AdmissionId, WardId = wards[0].WardId, AlertType = ClinicalAlertType.OverdueMedication, Severity = ClinicalAlertSeverity.High, Message = "Insulin dose overdue by 45 minutes", Status = ClinicalAlertStatus.Acknowledged, AcknowledgedBy = "nurse@claritycare.local", AcknowledgedAt = DateTime.UtcNow.AddMinutes(-30), CreatedAt = DateTime.UtcNow.AddHours(-1), CreatedBy = "System" });
        }
        return result;
    }

    // ============ INSURANCE PROVIDERS ============
    private static List<InsuranceProvider> GetInsuranceProviders()
    {
        return new List<InsuranceProvider>
        {
            new() { ProviderId = Guid.NewGuid(), Name = "BUPA", ContactName = "Claims Department", Email = "claims@bupa.co.uk", Phone = "0800 600 500", IsActive = true },
            new() { ProviderId = Guid.NewGuid(), Name = "AXA Health", ContactName = "Provider Services", Email = "providers@axahealth.co.uk", Phone = "0800 028 2825", IsActive = true },
            new() { ProviderId = Guid.NewGuid(), Name = "Vitality Health", ContactName = "Claims Team", Email = "claims@vitality.co.uk", Phone = "0345 602 0202", IsActive = true },
        };
    }

    // ============ MEDICATION CATALOG ============
    private static List<MedicationCatalog> GetMedicationCatalog()
    {
        return new List<MedicationCatalog>
        {
            new() { MedicationId = Guid.NewGuid(), Name = "Amoxicillin", Strength = "500mg", Route = "Oral", IsActive = true },
            new() { MedicationId = Guid.NewGuid(), Name = "Paracetamol", Strength = "500mg", Route = "Oral", IsActive = true },
            new() { MedicationId = Guid.NewGuid(), Name = "Ibuprofen", Strength = "400mg", Route = "Oral", IsActive = true },
            new() { MedicationId = Guid.NewGuid(), Name = "Omeprazole", Strength = "20mg", Route = "Oral", IsActive = true },
            new() { MedicationId = Guid.NewGuid(), Name = "Ramipril", Strength = "5mg", Route = "Oral", IsActive = true },
            new() { MedicationId = Guid.NewGuid(), Name = "Metformin", Strength = "500mg", Route = "Oral", IsActive = true },
            new() { MedicationId = Guid.NewGuid(), Name = "Insulin Novorapid", Strength = "100 units/mL", Route = "Subcutaneous", IsActive = true },
            new() { MedicationId = Guid.NewGuid(), Name = "Enoxaparin", Strength = "40mg", Route = "Subcutaneous", IsActive = true },
            new() { MedicationId = Guid.NewGuid(), Name = "Co-codamol", Strength = "30/500mg", Route = "Oral", IsActive = true },
            new() { MedicationId = Guid.NewGuid(), Name = "Amlodipine", Strength = "5mg", Route = "Oral", IsActive = true },
        };
    }

    // ============ GP PRACTICES ============
    private static List<GPPractice> GetGPPractices()
    {
        return new List<GPPractice>
        {
            new() { GPPracticeId = Guid.NewGuid(), PracticeCode = "GP001", PracticeName = "Riverside Medical Centre", LeadGPName = "Dr. Helen Carter", PhoneNumber = "020 7123 4001", Email = "admin@riverside-mc.nhs.uk", AddressLine1 = "45 River Lane", Town = "London", Postcode = "SE1 2AB", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { GPPracticeId = Guid.NewGuid(), PracticeCode = "GP002", PracticeName = "The Oak Surgery", LeadGPName = "Dr. Michael Green", PhoneNumber = "020 7123 4002", Email = "reception@oaksurgery.nhs.uk", AddressLine1 = "12 Oak Avenue", Town = "London", Postcode = "N1 3CD", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { GPPracticeId = Guid.NewGuid(), PracticeCode = "GP003", PracticeName = "Highgate Health Centre", LeadGPName = "Dr. Priya Sharma", PhoneNumber = "020 7123 4003", Email = "info@highgate-hc.nhs.uk", AddressLine1 = "88 High Street", Town = "London", Postcode = "N6 5EF", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { GPPracticeId = Guid.NewGuid(), PracticeCode = "GP004", PracticeName = "Elm Park Practice", LeadGPName = "Dr. James Wilson", PhoneNumber = "020 7123 4004", Email = "admin@elmpark.nhs.uk", AddressLine1 = "3 Elm Park Road", Town = "London", Postcode = "SW3 6GH", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { GPPracticeId = Guid.NewGuid(), PracticeCode = "GP005", PracticeName = "Victoria Park Surgery", LeadGPName = "Dr. Fatima Al-Rashid", PhoneNumber = "020 7123 4005", Email = "enquiries@vicpark.nhs.uk", AddressLine1 = "156 Victoria Park Road", Town = "London", Postcode = "E9 7JK", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { GPPracticeId = Guid.NewGuid(), PracticeCode = "GP006", PracticeName = "Meadow Lane Medical Group", LeadGPName = "Dr. Robert Chang", PhoneNumber = "020 7123 4006", Email = "contact@meadowlane.nhs.uk", AddressLine1 = "22 Meadow Lane", Town = "London", Postcode = "W4 2LM", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { GPPracticeId = Guid.NewGuid(), PracticeCode = "GP007", PracticeName = "St. Mary's Family Practice", LeadGPName = "Dr. Sarah O'Connor", PhoneNumber = "020 7123 4007", Email = "reception@stmarys-fp.nhs.uk", AddressLine1 = "1 Church Street", Town = "London", Postcode = "EC2 4NP", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { GPPracticeId = Guid.NewGuid(), PracticeCode = "GP008", PracticeName = "Greenfield Surgery", LeadGPName = "Dr. Ahmed Hassan", PhoneNumber = "020 7123 4008", Email = "admin@greenfield.nhs.uk", AddressLine1 = "67 Green Lane", Town = "London", Postcode = "NW3 8QR", IsActive = false, CreatedAt = DateTime.UtcNow },
        };
    }

    // ============ FOUNDATION DATA (unchanged) ============
    private static List<Permission> GetPermissions()
    {
        var permissionCodes = new[] {
            ("Patient.Create", "Create Patient", "Patients"), ("Patient.View", "View Patient", "Patients"), ("Patient.EditContactDetails", "Edit Patient Contact Details", "Patients"), ("Patient.Archive", "Archive Patient", "Patients"),
            ("Appointment.Book", "Book Appointment", "Appointments"), ("Appointment.View", "View Appointment", "Appointments"), ("Appointment.Cancel", "Cancel Appointment", "Appointments"), ("Appointment.Reschedule", "Reschedule Appointment", "Appointments"), ("Appointment.MarkArrived", "Mark Arrived", "Appointments"), ("Appointment.OverrideAvailability", "Override Availability", "Appointments"),
            ("Consultation.Start", "Start Consultation", "Clinical"), ("Consultation.View", "View Consultation", "Clinical"), ("Consultation.AddNote", "Add Clinical Note", "Clinical"), ("Consultation.Complete", "Complete Consultation", "Clinical"),
            ("Lab.Request", "Request Lab", "Labs"), ("Lab.View", "View Lab", "Labs"), ("Lab.EnterResult", "Enter Lab Result", "Labs"), ("Lab.ReviewResult", "Review Lab Result", "Labs"),
            ("Pharmacy.View", "View Pharmacy", "Pharmacy"), ("Pharmacy.ApprovePrescription", "Approve Prescription", "Pharmacy"), ("Pharmacy.DispensePrescription", "Dispense Prescription", "Pharmacy"),
            ("Invoice.Create", "Create Invoice", "Billing"), ("Invoice.Issue", "Issue Invoice", "Billing"), ("Billing.View", "View Billing", "Billing"), ("Payment.Record", "Record Payment", "Billing"), ("InsuranceClaim.Manage", "Manage Insurance Claims", "Billing"),
            ("Report.View", "View Reports", "Reports"), ("AuditLog.View", "View Audit Logs", "Admin"), ("Admin.UserManage", "Manage Users", "Admin"), ("Admin.RoleManage", "Manage Roles", "Admin"), ("Admin.PermissionManage", "Manage Permissions", "Admin"),
        };
        return permissionCodes.Select(p => new Permission { PermissionId = Guid.NewGuid(), Code = p.Item1, Name = p.Item2, Module = p.Item3, IsActive = true }).ToList();
    }

    private static List<Role> GetRoles() => new()
    {
        new() { RoleId = Guid.NewGuid(), Name = Roles.SystemAdmin, Description = "System Administrator", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        new() { RoleId = Guid.NewGuid(), Name = Roles.Doctor, Description = "Doctor / Clinician", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        new() { RoleId = Guid.NewGuid(), Name = Roles.Nurse, Description = "Nurse", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        new() { RoleId = Guid.NewGuid(), Name = Roles.Receptionist, Description = "Receptionist", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        new() { RoleId = Guid.NewGuid(), Name = Roles.Pharmacist, Description = "Pharmacist", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        new() { RoleId = Guid.NewGuid(), Name = Roles.BillingClerk, Description = "Billing Clerk", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        new() { RoleId = Guid.NewGuid(), Name = Roles.LabTechnician, Description = "Lab Technician", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
    };

    private static List<RolePermission> GetRolePermissions(List<Role> roles, List<Permission> permissions)
    {
        var result = new List<RolePermission>();
        var admin = roles.First(r => r.Name == Roles.SystemAdmin);
        foreach (var p in permissions) result.Add(new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = admin.RoleId, PermissionId = p.PermissionId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" });
        var doctor = roles.First(r => r.Name == Roles.Doctor);
        foreach (var code in new[] { "Patient.View", "Patient.Create", "Patient.EditContactDetails", "Appointment.View", "Appointment.Book", "Consultation.Start", "Consultation.View", "Consultation.AddNote", "Consultation.Complete", "Lab.Request", "Lab.View", "Pharmacy.View", "Report.View" })
        { var p = permissions.FirstOrDefault(x => x.Code == code); if (p != null) result.Add(new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = doctor.RoleId, PermissionId = p.PermissionId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" }); }
        var nurse = roles.First(r => r.Name == Roles.Nurse);
        foreach (var code in new[] { "Patient.View", "Appointment.View", "Appointment.MarkArrived", "Consultation.View", "Consultation.AddNote", "Lab.View", "Pharmacy.View" })
        { var p = permissions.FirstOrDefault(x => x.Code == code); if (p != null) result.Add(new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = nurse.RoleId, PermissionId = p.PermissionId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" }); }
        var receptionist = roles.First(r => r.Name == Roles.Receptionist);
        foreach (var code in new[] { "Patient.View", "Patient.Create", "Patient.EditContactDetails", "Appointment.View", "Appointment.Book", "Appointment.Cancel", "Appointment.Reschedule", "Appointment.MarkArrived" })
        { var p = permissions.FirstOrDefault(x => x.Code == code); if (p != null) result.Add(new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = receptionist.RoleId, PermissionId = p.PermissionId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" }); }
        var pharmacist = roles.First(r => r.Name == Roles.Pharmacist);
        foreach (var code in new[] { "Patient.View", "Pharmacy.View", "Pharmacy.ApprovePrescription", "Pharmacy.DispensePrescription" })
        { var p = permissions.FirstOrDefault(x => x.Code == code); if (p != null) result.Add(new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = pharmacist.RoleId, PermissionId = p.PermissionId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" }); }
        var billing = roles.First(r => r.Name == Roles.BillingClerk);
        foreach (var code in new[] { "Patient.View", "Billing.View", "Invoice.Create", "Invoice.Issue", "Payment.Record", "InsuranceClaim.Manage", "Report.View" })
        { var p = permissions.FirstOrDefault(x => x.Code == code); if (p != null) result.Add(new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = billing.RoleId, PermissionId = p.PermissionId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" }); }
        return result;
    }

    private static List<User> GetUsers() => new()
    {
        CreateUser("Admin", "User", "admin@claritycare.local", "Admin123!"),
        CreateUser("John", "Smith", "doctor@claritycare.local", "Doctor123!"),
        CreateUser("Sarah", "Johnson", "nurse@claritycare.local", "Nurse123!"),
        CreateUser("Emily", "Brown", "reception@claritycare.local", "Reception123!"),
        CreateUser("David", "Wilson", "pharmacist@claritycare.local", "Pharma123!"),
        CreateUser("Lisa", "Taylor", "billing@claritycare.local", "Billing123!"),
    };

    private static User CreateUser(string firstName, string lastName, string email, string password) => new()
    { UserId = Guid.NewGuid(), FirstName = firstName, LastName = lastName, Email = email, PasswordHash = HashPassword(password), IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" };

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        var combined = new byte[48]; Array.Copy(salt, 0, combined, 0, 16); Array.Copy(hash, 0, combined, 16, 32);
        return Convert.ToBase64String(combined);
    }

    private static List<UserRole> GetUserRoles(List<User> users, List<Role> roles)
    {
        var mappings = new Dictionary<string, string> { ["admin@claritycare.local"] = Roles.SystemAdmin, ["doctor@claritycare.local"] = Roles.Doctor, ["nurse@claritycare.local"] = Roles.Nurse, ["reception@claritycare.local"] = Roles.Receptionist, ["pharmacist@claritycare.local"] = Roles.Pharmacist, ["billing@claritycare.local"] = Roles.BillingClerk };
        return mappings.Select(m => new UserRole { UserRoleId = Guid.NewGuid(), UserId = users.First(u => u.Email == m.Key).UserId, RoleId = roles.First(r => r.Name == m.Value).RoleId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" }).ToList();
    }

    private static List<Department> GetDepartments() => new()
    {
        new() { DepartmentId = Guid.NewGuid(), Name = "General Medicine", Description = "General Medicine Department", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        new() { DepartmentId = Guid.NewGuid(), Name = "Cardiology", Description = "Cardiology Department", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        new() { DepartmentId = Guid.NewGuid(), Name = "Orthopaedics", Description = "Orthopaedics Department", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        new() { DepartmentId = Guid.NewGuid(), Name = "Paediatrics", Description = "Paediatrics Department", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        new() { DepartmentId = Guid.NewGuid(), Name = "Emergency", Description = "Emergency Department", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
    };
}
