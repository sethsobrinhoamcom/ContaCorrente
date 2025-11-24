using ContaCorrente.Domain.Enums;

namespace ContaCorrente.Domain.Exceptions;

public class DomainException : Exception
{
    public ErrorType ErrorType { get; }

    public DomainException(string message, ErrorType errorType) : base(message)
    {
        ErrorType = errorType;
    }
}