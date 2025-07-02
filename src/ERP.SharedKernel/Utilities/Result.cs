namespace ERP.SharedKernel.Utilities
{
    /// <summary>
    /// 함수형 프로그래밍 스타일의 Result 패턴
    /// 예외 대신 성공/실패를 명시적으로 표현
    /// </summary>
    public class Result
    {
        protected Result(bool isSuccess, IEnumerable<string> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors?.ToList() ?? new List<string>();
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public IReadOnlyList<string> Errors { get; }

        public static Result Success() => new(true, Enumerable.Empty<string>());
        public static Result Failure(string error) => new(false, new[] { error });
        public static Result Failure(IEnumerable<string> errors) => new(false, errors);

        public static Result<T> Success<T>(T value) => new(value, true, Enumerable.Empty<string>());
        public static Result<T> Failure<T>(string error) => new(default!, false, new[] { error });
        public static Result<T> Failure<T>(IEnumerable<string> errors) => new(default!, false, errors);
    }

    /// <summary>
    /// 값을 포함하는 Result
    /// </summary>
    public class Result<T> : Result
    {
        private readonly T _value;

        internal Result(T value, bool isSuccess, IEnumerable<string> errors) : base(isSuccess, errors)
        {
            _value = value;
        }

        public T Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot access value of failed result");

        public static implicit operator Result<T>(T value) => Success(value);
    }
}