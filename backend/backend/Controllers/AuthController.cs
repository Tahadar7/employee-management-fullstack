using backend.DTOs.Auth;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private const string RefreshCookieName = "refreshToken";

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
        {
            var result = await authService.RegisterAsync(request, ct);
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(result.Response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
        {
            var result = await authService.LoginAsync(request, ct);
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(result.Response);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken ct)
        {
             // read the refresh token from the HttpOnly cookie
            var refreshToken = Request.Cookies[RefreshCookieName] ?? string.Empty;

            var result = await authService.RefreshAsync(refreshToken, ct);
            SetRefreshTokenCookie(result.RefreshToken);  // rotate: set the new one
            return Ok(result.Response);
        }

         [HttpPost("logout")]
        public IActionResult Logout()
        {
            // clear the cookie
            Response.Cookies.Delete(RefreshCookieName);
            return Ok(new { message = "Logged out." });
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,                       // JS can't read it 
                Secure = true,                         // HTTPS only
                SameSite = SameSiteMode.None,          // needed for cross-origin
                Expires = DateTimeOffset.UtcNow.AddDays(7)  // matches refresh token lifetime
            });
        }
    }
}