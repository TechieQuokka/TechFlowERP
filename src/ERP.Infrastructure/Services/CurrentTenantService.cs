using ERP.SharedKernel.Abstractions;

namespace ERP.Infrastructure.Services
{
    public class CurrentTenantService : ICurrentTenantService
    {
        private string? _tenantId;

        public string? TenantId => _tenantId;
        public bool IsMultiTenant => !string.IsNullOrEmpty(_tenantId);

        public void SetTenant(string tenantId)
        {
            _tenantId = tenantId;
        }
    }
}