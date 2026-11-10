
namespace Roshetta.API.Models.DTOs
{
    public enum ErrorType { Validation, NotFound, Conflict, Unauthorized, Forbidden, Unexpected }
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public ErrorType? ErrorType { get; private set; }
        public IEnumerable<string> Errors { get; private set; } = [];

        private Result(bool isSuccess, IEnumerable<string>? errors, ErrorType? errorType)
        {
            IsSuccess = isSuccess;
            Errors = errors ?? [];
            ErrorType = errorType;
        }

        public static Result Success()
            => new(true, null, null);

        public static Result Failure(IEnumerable<string> errors, ErrorType errorType)
            => new(false, errors, errorType);
    }

    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public ErrorType? ErrorType { get; private set; }

        public IEnumerable<string> Errors { get; private set; } = [];

        private Result(bool isSuccess, T? data, IEnumerable<string>? errors, ErrorType? errorType)
        {
            IsSuccess = isSuccess;
            Data = data;
            Errors = errors ?? [];
            ErrorType = errorType;
        }

        public static Result<T> Success(T data)
            => new(true, data, null, null);

        public static Result<T> Failure(IEnumerable<string> errors, ErrorType errorType)
            => new(false, default, errors, errorType);
    }
}
