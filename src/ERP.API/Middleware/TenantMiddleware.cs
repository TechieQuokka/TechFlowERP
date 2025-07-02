using ERP.SharedKernel.Abstractions;

namespace ERP.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICurrentTenantService tenantService)
        {
            // Extract tenant ID from header, JWT claim, or subdomain
            var tenantId = ExtractTenantId(context);

            if (!string.IsNullOrEmpty(tenantId))
            {
                tenantService.SetTenant(tenantId);
            }

            await _next(context);
        }

        private static string? ExtractTenantId(HttpContext context)
        {
            // Priority 1: Header
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue))
            {
                return headerValue.FirstOrDefault();
            }

            // Priority 2: JWT Claim
            var tenantClaim = context.User?.FindFirst("tenant_id");
            if (tenantClaim != null)
            {
                return tenantClaim.Value;
            }

            // Priority 3: Subdomain (e.g., tenant1.api.example.com)
            var host = context.Request.Host.Host;
            if (host.Contains('.'))
            {
                var subdomain = host.Split('.')[0];
                if (subdomain != "api" && subdomain != "www")
                {
                    return subdomain;
                }
            }

            return null;
        }
    }
}