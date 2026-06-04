using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public ReportsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpGet("operational-dashboard")]
    public async Task<IActionResult> GetOperationalDashboard(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todayAppointments = await _dbContext.Appointments.CountAsync(a => a.StartTime >= today && a.StartTime < tomorrow, cancellationToken);
        var activeConsultations = await _dbContext.Consultations.CountAsync(c => c.Status == ConsultationStatus.InProgress, cancellationToken);
        var pendingLabRequests = await _dbContext.LabRequests.CountAsync(lr => lr.Status == LabRequestStatus.Requested, cancellationToken);
        var pendingPrescriptions = await _dbContext.Prescriptions.CountAsync(p => p.Status == PrescriptionStatus.Submitted, cancellationToken);
        var activeAdmissions = await _dbContext.Admissions.CountAsync(a => a.Status == AdmissionStatus.Admitted, cancellationToken);
        var totalPatients = await _dbContext.Patients.CountAsync(cancellationToken);

        return Ok(new
        {
            todayAppointments, activeConsultations, pendingLabRequests,
            pendingPrescriptions, activeAdmissions, totalPatients, generatedAt = DateTime.UtcNow
        });
    }

    [HttpGet("clinical-dashboard")]
    public async Task<IActionResult> GetClinicalDashboard(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var thisMonth = new DateTime(today.Year, today.Month, 1);

        var consultationsThisMonth = await _dbContext.Consultations.CountAsync(c => c.StartedAt >= thisMonth, cancellationToken);
        var diagnosesThisMonth = await _dbContext.Diagnoses.CountAsync(d => d.CreatedAt >= thisMonth, cancellationToken);
        var labRequestsThisMonth = await _dbContext.LabRequests.CountAsync(lr => lr.RequestedAt >= thisMonth, cancellationToken);
        var prescriptionsThisMonth = await _dbContext.Prescriptions.CountAsync(p => p.CreatedAt >= thisMonth, cancellationToken);
        var abnormalResults = await _dbContext.LabResults.CountAsync(r => r.IsAbnormal && r.RecordedAt >= thisMonth, cancellationToken);

        return Ok(new
        {
            consultationsThisMonth, diagnosesThisMonth, labRequestsThisMonth,
            prescriptionsThisMonth, abnormalResults, generatedAt = DateTime.UtcNow
        });
    }

    [HttpGet("financial-dashboard")]
    public async Task<IActionResult> GetFinancialDashboard(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var thisMonth = new DateTime(today.Year, today.Month, 1);

        var totalRevenue = await _dbContext.Payments
            .Where(p => p.PaymentDate >= thisMonth && p.Status == PaymentStatus.Completed)
            .SumAsync(p => p.Amount, cancellationToken);

        var totalInvoiced = await _dbContext.Invoices
            .Where(i => i.InvoiceDate >= thisMonth)
            .SumAsync(i => i.TotalAmount, cancellationToken);

        var outstanding = await _dbContext.Invoices
            .Where(i => i.BalanceDue > 0)
            .SumAsync(i => i.BalanceDue, cancellationToken);

        var insurancePending = await _dbContext.InsuranceClaims
            .CountAsync(c => c.Status == InsuranceClaimStatus.Submitted, cancellationToken);

        return Ok(new { totalRevenue, totalInvoiced, outstanding, insurancePending, generatedAt = DateTime.UtcNow });
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var startDate = from ?? DateTime.UtcNow.Date.AddMonths(-1);
        var endDate = to ?? DateTime.UtcNow.Date.AddDays(1);

        var payments = await _dbContext.Payments
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate < endDate && p.Status == PaymentStatus.Completed)
            .GroupBy(p => p.PaymentDate.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(p => p.Amount), Count = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync(cancellationToken);

        var byMethod = await _dbContext.Payments
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate < endDate && p.Status == PaymentStatus.Completed)
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new { Method = g.Key, Total = g.Sum(p => p.Amount), Count = g.Count() })
            .ToListAsync(cancellationToken);

        return Ok(new { daily = payments, byMethod, from = startDate, to = endDate });
    }

    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancyReport(CancellationToken cancellationToken)
    {
        var wards = await _dbContext.Wards
            .Include(w => w.Beds)
            .Where(w => w.IsActive)
            .Select(w => new
            {
                w.WardId, w.Name, w.WardType, w.Capacity,
                TotalBeds = w.Beds.Count,
                OccupiedBeds = w.Beds.Count(b => b.Status == BedStatus.Occupied),
                AvailableBeds = w.Beds.Count(b => b.Status == BedStatus.Available)
            })
            .ToListAsync(cancellationToken);

        var totalBeds = wards.Sum(w => w.TotalBeds);
        var totalOccupied = wards.Sum(w => w.OccupiedBeds);
        var occupancyRate = totalBeds > 0 ? (double)totalOccupied / totalBeds * 100 : 0;

        return Ok(new { overall = new { totalBeds, totalOccupied, occupancyRate }, wards });
    }
}
