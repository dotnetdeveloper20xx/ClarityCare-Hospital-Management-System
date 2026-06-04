using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Infrastructure.Services;

public class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly ApplicationDbContext _dbContext;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public InvoiceNumberGenerator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"INV-{year}-";
            var lastInvoice = await _dbContext.Invoices
                .Where(i => i.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(i => i.InvoiceNumber)
                .FirstOrDefaultAsync(cancellationToken);

            int nextNumber = 1;
            if (lastInvoice != null)
            {
                var lastNumberStr = lastInvoice.InvoiceNumber.Replace(prefix, "");
                if (int.TryParse(lastNumberStr, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D6}";
        }
        finally
        {
            _lock.Release();
        }
    }
}
