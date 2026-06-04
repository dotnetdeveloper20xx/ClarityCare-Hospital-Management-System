using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class NursingTasksController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public NursingTasksController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("nursing-tasks")]
    public async Task<IActionResult> CreateNursingTask([FromBody] CreateNursingTaskRequest request, CancellationToken cancellationToken)
    {
        var task = new NursingTask
        {
            NursingTaskId = Guid.NewGuid(),
            PatientId = request.PatientId,
            AdmissionId = request.AdmissionId,
            WardId = request.WardId,
            TaskType = request.TaskType,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueAt = request.DueAt,
            AssignedTo = request.AssignedTo,
            Status = NursingTaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "system"
        };

        _dbContext.NursingTasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Nursing", "NursingTask", task.NursingTaskId.ToString(),
            "Created", null, new { task.Title, task.Priority, task.DueAt }, cancellationToken: cancellationToken);

        return Created($"/api/nursing-tasks/{task.NursingTaskId}", new { nursingTaskId = task.NursingTaskId });
    }

    [HttpPost("nursing-tasks/{id:guid}/complete")]
    public async Task<IActionResult> CompleteNursingTask(Guid id, [FromBody] CompleteNursingTaskRequest? request, CancellationToken cancellationToken)
    {
        var task = await _dbContext.NursingTasks.FindAsync(new object[] { id }, cancellationToken);
        if (task == null) return NotFound();

        if (task.Status == NursingTaskStatus.Completed)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Task is already completed." });

        task.Status = NursingTaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;
        task.CompletedBy = _currentUser.Email;
        task.UpdatedAt = DateTime.UtcNow;
        task.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Nursing", "NursingTask", id.ToString(),
            "Completed", null, new { task.CompletedAt, task.CompletedBy }, cancellationToken: cancellationToken);

        return Ok(new { message = "Nursing task completed." });
    }

    [HttpGet("wards/{wardId:guid}/nursing-task-board")]
    public async Task<IActionResult> GetNursingTaskBoard(Guid wardId, CancellationToken cancellationToken)
    {
        var tasks = await _dbContext.NursingTasks
            .Include(t => t.Patient)
            .Where(t => t.WardId == wardId && t.Status != NursingTaskStatus.Cancelled)
            .OrderBy(t => t.Priority).ThenBy(t => t.DueAt)
            .Select(t => new
            {
                t.NursingTaskId, t.Title, t.TaskType, t.Description, t.Priority,
                t.DueAt, t.AssignedTo, t.Status, t.CreatedAt,
                PatientName = t.Patient.FirstName + " " + t.Patient.LastName,
                t.PatientId
            })
            .ToListAsync(cancellationToken);

        var summary = new
        {
            pending = tasks.Count(t => t.Status == NursingTaskStatus.Pending),
            inProgress = tasks.Count(t => t.Status == NursingTaskStatus.InProgress),
            completed = tasks.Count(t => t.Status == NursingTaskStatus.Completed),
            overdue = tasks.Count(t => t.DueAt.HasValue && t.DueAt.Value < DateTime.UtcNow && t.Status != NursingTaskStatus.Completed)
        };

        return Ok(new { summary, tasks });
    }
}

public record CreateNursingTaskRequest(Guid PatientId, Guid AdmissionId, Guid WardId, string TaskType, string Title, string? Description, NursingTaskPriority Priority, DateTime? DueAt, string? AssignedTo);
public record CompleteNursingTaskRequest(string? Notes);
