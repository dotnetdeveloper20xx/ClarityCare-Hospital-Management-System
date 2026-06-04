namespace ClarityCare.Application.Common.Interfaces;

public interface IInvoiceNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
