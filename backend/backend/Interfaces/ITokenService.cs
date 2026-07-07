using backend.Entities;

namespace backend.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        RefreshToken GenerateRefreshToken(int userId);
    }
}