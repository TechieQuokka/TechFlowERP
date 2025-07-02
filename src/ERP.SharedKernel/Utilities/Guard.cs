namespace ERP.SharedKernel.Utilities
{
    /// <summary>
    /// 가드 클래스 - 입력 유효성 검사
    /// </summary>
    public static class Guard
    {
        public static void Against(bool condition, string message)
        {
            if (condition)
                throw new DomainException(message);
        }

        public static void AgainstNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);
        }

        public static void AgainstNullOrEmpty(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or empty", parameterName);
        }

        public static void AgainstNegative(decimal value, string parameterName)
        {
            if (value < 0)
                throw new ArgumentException("Value cannot be negative", parameterName);
        }

        public static void AgainstNegative(int value, string parameterName)
        {
            if (value < 0)
                throw new ArgumentException("Value cannot be negative", parameterName);
        }

        public static void AgainstEmpty<T>(IEnumerable<T> value, string parameterName)
        {
            if (value == null || !value.Any())
                throw new ArgumentException("Collection cannot be null or empty", parameterName);
        }

        public static void AgainstInvalidGuid(Guid value, string parameterName)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("Guid cannot be empty", parameterName);
        }
    }
}