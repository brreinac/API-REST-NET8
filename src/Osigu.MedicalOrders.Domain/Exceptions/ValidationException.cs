namespace Osigu.MedicalOrders.Domain.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(IReadOnlyList<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}
