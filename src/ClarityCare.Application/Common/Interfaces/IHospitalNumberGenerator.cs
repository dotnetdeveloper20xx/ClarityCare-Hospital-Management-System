namespace ClarityCare.Application.Common.Interfaces;

public interface IHospitalNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
