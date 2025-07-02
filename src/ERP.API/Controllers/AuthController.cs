using ERP.Infrastructure.Identity;

using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _tokenService;

        public AuthController(IJwtTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        /// Login endpoint (simplified for demo)
        /// </summary>
        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            // In a real application, validate credentials against database
            // This is a simplified example
            if (request.Email == "admin@erp.com" && request.Password == "admin123")
            {
                var token = _tokenService.GenerateToken(
                    "admin-user-id",
                    request.Email,
                    request.TenantId ?? "default-tenant",
                    new[] { "Admin" });

                return Ok(new LoginResponse
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(8)
                });
            }

            return Unauthorized("Invalid credentials");
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string? TenantId { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
    }
}