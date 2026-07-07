using backend.DTOs.Auth;

namespace backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default);
    }
}