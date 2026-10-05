namespace GameLending.Core.Domain;

public sealed class DomainException(string message) : Exception(message);
