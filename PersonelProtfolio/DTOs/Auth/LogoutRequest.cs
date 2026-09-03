namespace PersonelProtfolio.DTOs.Auth
{
    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
