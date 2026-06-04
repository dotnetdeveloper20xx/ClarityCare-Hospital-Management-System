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
public class DocumentsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public DocumentsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("documents")]
    public async Task<IActionResult> UploadDocument([FromBody] UploadDocumentRequest request, CancellationToken cancellationToken)
    {
        var document = new Document
        {
            DocumentId = Guid.NewGuid(),
            PatientId = request.PatientId,
            DocumentType = request.DocumentType,
            FileName = request.FileName,
            StoragePath = $"documents/{request.PatientId}/{Guid.NewGuid()}/{request.FileName}",
            Version = 1,
            Status = DocumentStatus.Active,
            UploadedBy = _currentUser.Email ?? "system",
            UploadedAt = DateTime.UtcNow
        };

        _dbContext.Documents.Add(document);

        var version = new DocumentVersion
        {
            VersionId = Guid.NewGuid(),
            DocumentId = document.DocumentId,
            VersionNumber = 1,
            StoragePath = document.StoragePath,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.DocumentVersions.Add(version);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Documents", "Document", document.DocumentId.ToString(),
            "Uploaded", null, new { document.FileName, document.DocumentType }, cancellationToken: cancellationToken);

        return CreatedAtAction(nameof(GetDocument), new { id = document.DocumentId }, new { documentId = document.DocumentId });
    }

    [HttpGet("documents/{id:guid}")]
    public async Task<IActionResult> GetDocument(Guid id, CancellationToken cancellationToken)
    {
        var document = await _dbContext.Documents
            .Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.DocumentId == id, cancellationToken);

        if (document == null) return NotFound();

        return Ok(new
        {
            data = new
            {
                document.DocumentId, document.PatientId, document.DocumentType, document.FileName,
                document.Version, document.Status, document.UploadedBy, document.UploadedAt,
                Versions = document.Versions.OrderByDescending(v => v.VersionNumber).Select(v => new
                {
                    v.VersionId, v.VersionNumber, v.StoragePath, v.CreatedAt
                })
            }
        });
    }

    [HttpGet("patients/{patientId:guid}/documents")]
    public async Task<IActionResult> GetPatientDocuments(Guid patientId, [FromQuery] DocumentType? type, CancellationToken cancellationToken)
    {
        var query = _dbContext.Documents
            .Where(d => d.PatientId == patientId && d.Status == DocumentStatus.Active);

        if (type.HasValue) query = query.Where(d => d.DocumentType == type.Value);

        var documents = await query
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new
            {
                d.DocumentId, d.DocumentType, d.FileName, d.Version, d.UploadedBy, d.UploadedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = documents });
    }
}

public record UploadDocumentRequest(Guid PatientId, DocumentType DocumentType, string FileName);
