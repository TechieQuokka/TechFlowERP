namespace ERP.SharedKernel.Utilities
{
    /// <summary>
    /// 도메인 로직 위반 시 발생하는 예외
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception innerException) : base(message, innerException) { }

        public string ErrorCode { get; init; } = "DOMAIN_ERROR";
    }

    /// <summary>
    /// 비즈니스 규칙 위반 예외
    /// </summary>
    public class BusinessRuleViolationException : DomainException
    {
        public BusinessRuleViolationException(string rule, string message)
            : base($"Business rule violated: {rule}. {message}")
        {
            ErrorCode = "BUSINESS_RULE_VIOLATION";
            Rule = rule;
        }

        public string Rule { get; }
    }
}
