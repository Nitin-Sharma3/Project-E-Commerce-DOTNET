namespace OrderAPI.Common
{
   

    /// <summary>Base for all domain-specific exceptions.</summary>
    public abstract class DomainException : Exception
    {
        public int StatusCode { get; }
        protected DomainException(string message, int statusCode) : base(message)
            => StatusCode = statusCode;
    }

    /// <summary>404 — entity not found.</summary>
    public class NotFoundException : DomainException
    {
        public NotFoundException(string entity, object key)
            : base($"{entity} with identifier '{key}' was not found.", 404) { }
        public NotFoundException(string message)
            : base(message, 404) { }
    }

    /// <summary>400 — invalid request or business rule violation.</summary>
    public class BadRequestException : DomainException
    {
        public List<string> Errors { get; }
        public BadRequestException(string message, List<string>? errors = null)
            : base(message, 400) => Errors = errors ?? new();
    }

    /// <summary>409 — conflict / illegal state transition.</summary>
    public class ConflictException : DomainException
    {
        public ConflictException(string message) : base(message, 409) { }
    }

    /// <summary>422 — validation failure (FluentValidation).</summary>
    public class ValidationException : DomainException
    {
        public List<string> Errors { get; }
        public ValidationException(List<string> errors)
            : base("One or more validation errors occurred.", 422)
            => Errors = errors;
    }

}
