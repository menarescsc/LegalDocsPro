namespace LegalDocsPro.Application.Common.Models
{
    /// <summary>
    /// Represents the result of an operation that can succeed or fail.
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T? Value { get; }
        public string? Error { get; }
        public string? ErrorCode { get; }

        private Result(bool isSuccess, T? value, string? error, string? errorCode)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Creates a successful result with a value.
        /// </summary>
        public static Result<T> Success(T value)
            => new(true, value, null, null);

        /// <summary>
        /// Creates a failed result with an error message.
        /// </summary>
        public static Result<T> Failure(string error, string? errorCode = null)
            => new(false, default, error, errorCode);

        /// <summary>
        /// Maps the value of a successful result to a new type.
        /// </summary>
        public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
            => IsSuccess
                ? Result<TNew>.Success(mapper(Value!))
                : Result<TNew>.Failure(Error!, ErrorCode);

        /// <summary>
        /// Executes an action on the value if the result is successful.
        /// </summary>
        public Result<T> OnSuccess(Action<T> action)
        {
            if (IsSuccess)
                action(Value!);
            return this;
        }

        /// <summary>
        /// Executes an action if the result is a failure.
        /// </summary>
        public Result<T> OnFailure(Action<string> action)
        {
            if (IsFailure)
                action(Error!);
            return this;
        }
    }

    /// <summary>
    /// Non-generic Result for operations that don't return a value.
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; }
        public string? ErrorCode { get; }

        private Result(bool isSuccess, string? error, string? errorCode)
        {
            IsSuccess = isSuccess;
            Error = error;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static Result Success()
            => new(true, null, null);

        /// <summary>
        /// Creates a failed result with an error message.
        /// </summary>
        public static Result Failure(string error, string? errorCode = null)
            => new(false, error, errorCode);

        /// <summary>
        /// Creates a result from a boolean condition.
        /// </summary>
        public static Result From(bool isSuccess, string error, string? errorCode = null)
            => isSuccess ? Success() : Failure(error, errorCode);
    }
}
