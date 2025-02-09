using MediatR;

namespace ESS.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    public bool IsFailure => !IsSuccess;

    protected internal Result(bool isSuccess, T value, string error)
    {
        if (isSuccess && error != string.Empty)
            throw new InvalidOperationException();
        if (!isSuccess && error == string.Empty)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
}

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && error != string.Empty)
            throw new InvalidOperationException();
        if (!isSuccess && error == string.Empty)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(true, value, string.Empty);
    public static Result<T> Failure<T>(string error) => new(false, default!, error);

    public static implicit operator Result(Result<Unit> result)
        => new(result.IsSuccess, result.Error);
}