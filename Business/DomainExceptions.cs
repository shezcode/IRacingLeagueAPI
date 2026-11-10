namespace IRacingLeague.Business;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

public sealed class NotFoundException(string message) : DomainException(message) { }
public sealed class ForbiddenException(string message) : DomainException(message) { }
public sealed class ConflictException(string message) : DomainException(message) { }
public sealed class DomainValidationException(string message) : DomainException(message) { }
public sealed class PreconditionFailedException(string message) : DomainException(message) { }
