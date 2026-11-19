namespace HypertrophyApp.Application;

public enum ErrorType { Validation, NotFound }

public sealed record Error(ErrorType Type, string Message);

/// <summary>Represents the outcome of a use case without using exceptions for expected failures.</summary>
public sealed class Result<T>
{
    private Result(T? value, Error? error) => (Value, Error) = (value, error);

    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(ErrorType type, string message) => new(default, new Error(type, message));

    public static implicit operator Result<T>(T value) => Success(value);
}
