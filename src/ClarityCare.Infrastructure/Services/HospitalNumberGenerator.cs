using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Infrastructure.Services;

public class HospitalNumberGenerator : IHospitalNumberGenerator
{
    private readonly ApplicationDbContext _dbContext;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public HospitalNumberGenerator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"HOSP-{year}-";
            var lastPatient = await _dbContext.Patients
                .Where(p => p.HospitalNumber.StartsWith(prefix))
                .OrderByDescending(p => p.HospitalNumber)
                .FirstOrDefaultAsync(cancellationToken);

            int nextNumber = 1;
            if (lastPatient != null)
            {
                var lastNumberStr = lastPatient.HospitalNumber.Replace(prefix, "");
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
