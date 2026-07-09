using backend.DTOs.Auth;

namespace backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct = default);
    }
}