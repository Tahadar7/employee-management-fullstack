using backend.Data;
using backend.DTOs.Auth;
using backend.Entities;
using backend.Exceptions;
using backend.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Implementations
{
    public class AuthService(ApplicationDbContext context,
        ITokenService tokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator) : IAuthService
    {
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            await registerValidator.ValidateAndThrowAsync(request, ct);

            var email = request.Email.Trim().ToLowerInvariant();

            var exists = await context.Users.AnyAsync(u => u.Email == email, ct);
            if (exists)
                throw new ConflictException("User with this email already exists.");

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            context.Users.Add(user);
            await context.SaveChangesAsync(ct);

            return await BuildAuthResponseAsync(user, ct);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            await loginValidator.ValidateAndThrowAsync(request, ct);

            var email = request.Email.Trim().ToLowerInvariant();
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

            // same message for both cases — don't reveal which emails exist
            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedException("Invalid email or password.");

            return await BuildAuthResponseAsync(user, ct);
        }

        public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
        {
            var existing = await context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

            if (existing is null || !existing.IsActive)
                throw new UnauthorizedException("Invalid or expired refresh token.");

            // revoke the old token, issue a brand new one
            existing.IsRevoked = true;

            return await BuildAuthResponseAsync(existing.User, ct);
        }

        // shared: create tokens, persist refresh token, map to response
        private async Task<AuthResponse> BuildAuthResponseAsync(User user, CancellationToken ct)
        {
            // remove this user's revoked/expired refresh tokens
            var staleTokens = await context.RefreshTokens
                .Where(rt => rt.UserId == user.Id &&
                             (rt.IsRevoked || rt.ExpiresAt < DateTime.UtcNow))
                .ToListAsync(ct);

            if (staleTokens.Count > 0)
                context.RefreshTokens.RemoveRange(staleTokens);

            var accessToken = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken(user.Id);

            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync(ct);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }
    }
}