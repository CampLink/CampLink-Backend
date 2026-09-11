namespace CampLink.Application.Exceptions;

/// <summary>Базовое доменное исключение.</summary>
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message)
    {
    }
}

/// <summary>Объект не найден (HTTP 404).</summary>
public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message)
    {
    }
}

/// <summary>Конфликт бизнес-правил, например недостаточно остатка (HTTP 409).</summary>
public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message)
    {
    }
}

/// <summary>Некорректный вход (HTTP 400).</summary>
public sealed class ValidationException : AppException
{
    public ValidationException(string message) : base(message)
    {
    }
}
