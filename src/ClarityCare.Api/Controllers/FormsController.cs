using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api/forms")]
[Authorize]
public class FormsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public FormsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("templates")]
    public async Task<IActionResult> CreateFormTemplate([FromBody] CreateFormTemplateRequest request, CancellationToken cancellationToken)
    {
        var template = new FormTemplate
        {
            TemplateId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            JsonDefinition = request.JsonDefinition,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "system"
        };

        _dbContext.FormTemplates.Add(template);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Forms", "FormTemplate", template.TemplateId.ToString(),
            "Created", null, new { template.Name }, cancellationToken: cancellationToken);

        return Created($"/api/forms/templates/{template.TemplateId}", new { templateId = template.TemplateId });
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignForm([FromBody] AssignFormRequest request, CancellationToken cancellationToken)
    {
        var patientForm = new PatientForm
        {
            PatientFormId = Guid.NewGuid(),
            PatientId = request.PatientId,
            TemplateId = request.TemplateId,
            Status = PatientFormStatus.Assigned
        };

        _dbContext.PatientForms.Add(patientForm);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Forms", "PatientForm", patientForm.PatientFormId.ToString(),
            "Assigned", null, new { request.PatientId, request.TemplateId }, cancellationToken: cancellationToken);

        return Created($"/api/forms/{patientForm.PatientFormId}", new { patientFormId = patientForm.PatientFormId });
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> SubmitForm(Guid id, [FromBody] SubmitFormRequest request, CancellationToken cancellationToken)
    {
        var patientForm = await _dbContext.PatientForms.FindAsync(new object[] { id }, cancellationToken);
        if (patientForm == null) return NotFound();

        if (patientForm.Status == PatientFormStatus.Submitted)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Form has already been submitted." });

        patientForm.JsonResponse = request.JsonResponse;
        patientForm.Status = PatientFormStatus.Submitted;
        patientForm.SubmittedAt = DateTime.UtcNow;
        patientForm.SubmittedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Forms", "PatientForm", id.ToString(),
            "Submitted", null, new { patientForm.SubmittedAt }, cancellationToken: cancellationToken);

        return Ok(new { message = "Form submitted." });
    }

    [HttpGet("/api/patients/{patientId:guid}/forms")]
    public async Task<IActionResult> GetPatientForms(Guid patientId, CancellationToken cancellationToken)
    {
        var forms = await _dbContext.PatientForms
            .Include(pf => pf.Template)
            .Where(pf => pf.PatientId == patientId)
            .OrderByDescending(pf => pf.SubmittedAt)
            .Select(pf => new
            {
                pf.PatientFormId, pf.Status, pf.SubmittedAt, pf.SubmittedBy,
                TemplateName = pf.Template.Name
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = forms });
    }
}

public record CreateFormTemplateRequest(string Name, string? Description, string JsonDefinition);
public record AssignFormRequest(Guid PatientId, Guid TemplateId);
public record SubmitFormRequest(string JsonResponse);
