namespace backend.DTOs.Auth
{
    // The controller puts RefreshToken into an HttpOnly cookie
    // and returns only the AuthResponse (access token) in the body.
    public class AuthResult
    {
        public AuthResponse Response { get; set; } = new();
        public string RefreshToken { get; set; } = string.Empty;
    }
}