using ClarityCare.Domain.Entities;
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
    }

    private static List<Permission> GetPermissions()
    {
        var permissionCodes = new[]
        {
            ("Patient.Create", "Create Patient", "Patients"),
            ("Patient.View", "View Patient", "Patients"),
            ("Patient.EditContactDetails", "Edit Patient Contact Details", "Patients"),
            ("Patient.Archive", "Archive Patient", "Patients"),
            ("Appointment.Book", "Book Appointment", "Appointments"),
            ("Appointment.View", "View Appointment", "Appointments"),
            ("Appointment.Cancel", "Cancel Appointment", "Appointments"),
            ("Appointment.Reschedule", "Reschedule Appointment", "Appointments"),
            ("Appointment.MarkArrived", "Mark Arrived", "Appointments"),
            ("Appointment.OverrideAvailability", "Override Availability", "Appointments"),
            ("Consultation.Start", "Start Consultation", "Clinical"),
            ("Consultation.View", "View Consultation", "Clinical"),
            ("Consultation.AddNote", "Add Clinical Note", "Clinical"),
            ("Consultation.Complete", "Complete Consultation", "Clinical"),
            ("Lab.Request", "Request Lab", "Labs"),
            ("Lab.View", "View Lab", "Labs"),
            ("Lab.EnterResult", "Enter Lab Result", "Labs"),
            ("Lab.ReviewResult", "Review Lab Result", "Labs"),
            ("Pharmacy.View", "View Pharmacy", "Pharmacy"),
            ("Pharmacy.ApprovePrescription", "Approve Prescription", "Pharmacy"),
            ("Pharmacy.DispensePrescription", "Dispense Prescription", "Pharmacy"),
            ("Invoice.Create", "Create Invoice", "Billing"),
            ("Invoice.Issue", "Issue Invoice", "Billing"),
            ("Billing.View", "View Billing", "Billing"),
            ("Payment.Record", "Record Payment", "Billing"),
            ("InsuranceClaim.Manage", "Manage Insurance Claims", "Billing"),
            ("Report.View", "View Reports", "Reports"),
            ("AuditLog.View", "View Audit Logs", "Admin"),
            ("Admin.UserManage", "Manage Users", "Admin"),
            ("Admin.RoleManage", "Manage Roles", "Admin"),
            ("Admin.PermissionManage", "Manage Permissions", "Admin"),
        };

        return permissionCodes.Select(p => new Permission
        {
            PermissionId = Guid.NewGuid(),
            Code = p.Item1,
            Name = p.Item2,
            Module = p.Item3,
            IsActive = true
        }).ToList();
    }

    private static List<Role> GetRoles()
    {
        return new List<Role>
        {
            new() { RoleId = Guid.NewGuid(), Name = Roles.SystemAdmin, Description = "System Administrator", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { RoleId = Guid.NewGuid(), Name = Roles.Doctor, Description = "Doctor / Clinician", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { RoleId = Guid.NewGuid(), Name = Roles.Nurse, Description = "Nurse", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { RoleId = Guid.NewGuid(), Name = Roles.Receptionist, Description = "Receptionist", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { RoleId = Guid.NewGuid(), Name = Roles.Pharmacist, Description = "Pharmacist", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { RoleId = Guid.NewGuid(), Name = Roles.BillingClerk, Description = "Billing Clerk", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { RoleId = Guid.NewGuid(), Name = Roles.LabTechnician, Description = "Lab Technician", IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        };
    }

    private static List<RolePermission> GetRolePermissions(List<Role> roles, List<Permission> permissions)
    {
        var result = new List<RolePermission>();
        var admin = roles.First(r => r.Name == Roles.SystemAdmin);
        foreach (var p in permissions)
        {
            result.Add(new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = admin.RoleId, PermissionId = p.PermissionId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" });
        }

        var doctor = roles.First(r => r.Name == Roles.Doctor);
        var doctorPerms = new[] { "Patient.View", "Patient.Create", "Patient.EditContactDetails", "Appointment.View", "Appointment.Book", "Consultation.Start", "Consultation.View", "Consultation.AddNote", "Consultation.Complete", "Lab.Request", "Lab.View", "Pharmacy.View", "Report.View" };
        foreach (var code in doctorPerms)
        {
            var perm = permissions.FirstOrDefault(p => p.Code == code);
            if (perm != null) result.Add(new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = doctor.RoleId, PermissionId = perm.PermissionId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" });
        }

        var nurse = roles.First(r => r.Name == Roles.Nurse);
        var nursePerms = new[] { "Patient.View", "Appointment.View", "Appointment.MarkArrived", "Consultation.View", "Consultation.AddNote", "Lab.View", "Pharmacy.View" };
        foreach (var code in nursePerms)
        {
            var perm = permissions.FirstOrDefault(p => p.Code == code);
            if (perm != null) result.Add(new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = nurse.RoleId, PermissionId = perm.PermissionId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" });
        }

        var receptionist = roles.First(r => r.Name == Roles.Receptionist);
        var receptionistPerms = new[] { "Patient.View", "Patient.Create", "Patient.EditContactDetails", "Appointment.View", "Appointment.Book", "Appointment.Cancel", "Appointment.Reschedule", "Appointment.MarkArrived" };
        foreach (var code in receptionistPerms)
        {
            var perm = permissions.FirstOrDefault(p => p.Code == code);
            if (perm != null) result.Add(new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = receptionist.RoleId, PermissionId = perm.PermissionId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" });
        }

        var pharmacist = roles.First(r => r.Name == Roles.Pharmacist);
        var pharmacistPerms = new[] { "Patient.View", "Pharmacy.View", "Pharmacy.ApprovePrescription", "Pharmacy.DispensePrescription" };
        foreach (var code in pharmacistPerms)
        {
            var perm = permissions.FirstOrDefault(p => p.Code == code);
            if (perm != null) result.Add(new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = pharmacist.RoleId, PermissionId = perm.PermissionId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" });
        }

        var billing = roles.First(r => r.Name == Roles.BillingClerk);
        var billingPerms = new[] { "Patient.View", "Billing.View", "Invoice.Create", "Invoice.Issue", "Payment.Record", "InsuranceClaim.Manage", "Report.View" };
        foreach (var code in billingPerms)
        {
            var perm = permissions.FirstOrDefault(p => p.Code == code);
            if (perm != null) result.Add(new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = billing.RoleId, PermissionId = perm.PermissionId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" });
        }

        return result;
    }

    private static List<User> GetUsers()
    {
        return new List<User>
        {
            CreateUser("Admin", "User", "admin@claritycare.local", "Admin123!"),
            CreateUser("John", "Smith", "doctor@claritycare.local", "Doctor123!"),
            CreateUser("Sarah", "Johnson", "nurse@claritycare.local", "Nurse123!"),
            CreateUser("Emily", "Brown", "reception@claritycare.local", "Reception123!"),
            CreateUser("David", "Wilson", "pharmacist@claritycare.local", "Pharma123!"),
            CreateUser("Lisa", "Taylor", "billing@claritycare.local", "Billing123!"),
        };
    }

    private static User CreateUser(string firstName, string lastName, string email, string password)
    {
        return new User
        {
            UserId = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = HashPassword(password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        var combined = new byte[48];
        Array.Copy(salt, 0, combined, 0, 16);
        Array.Copy(hash, 0, combined, 16, 32);
        return Convert.ToBase64String(combined);
    }

    private static List<UserRole> GetUserRoles(List<User> users, List<Role> roles)
    {
        var result = new List<UserRole>();
        var mappings = new Dictionary<string, string>
        {
            ["admin@claritycare.local"] = Roles.SystemAdmin,
            ["doctor@claritycare.local"] = Roles.Doctor,
            ["nurse@claritycare.local"] = Roles.Nurse,
            ["reception@claritycare.local"] = Roles.Receptionist,
            ["pharmacist@claritycare.local"] = Roles.Pharmacist,
            ["billing@claritycare.local"] = Roles.BillingClerk,
        };

        foreach (var mapping in mappings)
        {
            var user = users.First(u => u.Email == mapping.Key);
            var role = roles.First(r => r.Name == mapping.Value);
            result.Add(new UserRole { UserRoleId = Guid.NewGuid(), UserId = user.UserId, RoleId = role.RoleId, AssignedAt = DateTime.UtcNow, AssignedBy = "System" });
        }

        return result;
    }

    private static List<Department> GetDepartments()
    {
        return new List<Department>
        {
            new() { DepartmentId = Guid.NewGuid(), Name = "General Medicine", Description = "General Medicine Department", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { DepartmentId = Guid.NewGuid(), Name = "Cardiology", Description = "Cardiology Department", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { DepartmentId = Guid.NewGuid(), Name = "Orthopaedics", Description = "Orthopaedics Department", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { DepartmentId = Guid.NewGuid(), Name = "Paediatrics", Description = "Paediatrics Department", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new() { DepartmentId = Guid.NewGuid(), Name = "Emergency", Description = "Emergency Department", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
        };
    }
}
