namespace ERP.SharedKernel.Abstractions
{
    /// <summary>
    /// Multi-tenant 지원을 위한 현재 테넌트 정보 서비스
    /// </summary>
    public interface ICurrentTenantService
    {
        string? TenantId { get; }
        bool IsMultiTenant { get; }
        void SetTenant(string tenantId);
    }
}