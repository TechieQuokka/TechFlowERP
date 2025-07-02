namespace ERP.SharedKernel.Constants
{
    /// <summary>
    /// 시스템 전반에서 사용하는 에러 코드
    /// </summary>
    public static class ErrorCodes
    {
        // 일반적인 에러
        public const string VALIDATION_ERROR = "VALIDATION_ERROR";
        public const string NOT_FOUND = "NOT_FOUND";
        public const string UNAUTHORIZED = "UNAUTHORIZED";
        public const string FORBIDDEN = "FORBIDDEN";

        // 도메인 에러
        public const string DOMAIN_ERROR = "DOMAIN_ERROR";
        public const string BUSINESS_RULE_VIOLATION = "BUSINESS_RULE_VIOLATION";

        // 프로젝트 관련
        public const string PROJECT_NOT_FOUND = "PROJECT_NOT_FOUND";
        public const string PROJECT_ALREADY_COMPLETED = "PROJECT_ALREADY_COMPLETED";
        public const string INVALID_PROJECT_STATUS = "INVALID_PROJECT_STATUS";

        // 직원 관련
        public const string EMPLOYEE_NOT_FOUND = "EMPLOYEE_NOT_FOUND";
        public const string EMPLOYEE_ALREADY_ASSIGNED = "EMPLOYEE_ALREADY_ASSIGNED";

        // 클라이언트 관련
        public const string CLIENT_NOT_FOUND = "CLIENT_NOT_FOUND";
        public const string CLIENT_HAS_ACTIVE_PROJECTS = "CLIENT_HAS_ACTIVE_PROJECTS";
    }
}