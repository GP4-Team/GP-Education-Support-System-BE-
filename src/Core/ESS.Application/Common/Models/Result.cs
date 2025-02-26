// src/Core/ESS.Application/Common/Models/Result.cs
namespace ESS.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public string[] Error { get; protected set; }
    public bool Succeeded => IsSuccess;
    public string[] Errors => Error;

    protected Result(bool isSuccess, string[] error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Array.Empty<string>());
    public static Result<T> Success<T>(T value) => new(true, Array.Empty<string>(), value);
    public static Result Failure(params string[] errors) => new(false, errors);
    public static Result<T> Failure<T>(params string[] errors) => new(false, errors, default);
}

public class Result<T> : Result
{
    public T Value { get; private set; }
    public T Data => Value;

    internal Result(bool isSuccess, string[] error, T value)
        : base(isSuccess, error)
    {
        Value = value;
    }
}