namespace ClarityCare.Domain.Common;

/// <summary>
/// Exception thrown when a domain invariant is violated.
/// Mapped to 422 Unprocessable Entity by global exception middleware.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
