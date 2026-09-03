using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PersonalProtfolioDataTier;
using PersonalProtfolioBusniessTier;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using PersonelProtfolio.DTOs.Auth;

namespace PersonelProtfolio.Controllers
{
    [Route("api/PersonelProtfolio")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // This endpoint handles user login.
        // It verifies credentials and returns a JWT token if login succeeds.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] PersonalProtfolioDataTier.LoginRequest request)
        {
            // Step 1: Find the student by email from the in-memory data store.
            // Email acts as the unique login identifier.
            LoginUserDataDTO? LoginUser = await PersonalProtfolioBusniessTier.Users.LoginUserByUserNameAndPassword(request.UserName, request.Password);

            if (LoginUser == null)
                return Unauthorized("Invalid credentials");

            // المطالبات
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, LoginUser.UserID.ToString()),
                new Claim(ClaimTypes.Name, LoginUser.UserName),
                new Claim(ClaimTypes.Role, LoginUser.Role)
            };


            // Step 3: Create the symmetric security key used to sign the JWT.
            // This key must match the key used in JWT validation middleware.
            // This key will stored in save file and read from there in real application, but for simplicity we hardcode it here. 

            // var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("THIS_IS_A_VERY_SECRET_KEY_123456"));
            var secretKey = _configuration["JwtSettings:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // Step 4: Define the signing credentials.
            // This specifies the algorithm used to sign the token.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            // Step 5: Create the JWT token.
            // The token includes issuer, audience, claims, expiration, and signature.
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(10),
                signingCredentials: creds
            );


            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            //6
            var refreshToken = RefreshTokenGenerator();
            LoginUser.RefreshToken = BCrypt.Net.BCrypt.HashPassword(refreshToken);
            LoginUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(30);
            LoginUser.RefreshTokenRevokedAt = null; 
            await Users.UpdateLastLogin(LoginUser.UserID, LoginUser.RefreshToken, LoginUser.RefreshTokenExpiryTime, LoginUser.RefreshTokenRevokedAt);
            // Step 6/7: Return the serialized JWT token to the client.
            // The client will send this token with future requests.

            return Ok(new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }
            );
            /* new{token = new JwtSecurityTokenHandler().WriteToken(token) }*/
        }


        private static string RefreshTokenGenerator()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);

        }

        // Add Refresh Endpoint (Rotation)
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> Refresh([FromBody] RefreshToken request)
        {
            LoginUserDataDTO? LoginUser = await PersonalProtfolioBusniessTier.Users.LoginUserByUserNameAndPassword(request.Username, request.Password);

            if (LoginUser == null)
                return Unauthorized("Invalid Refresh Token");

            // Check if the refresh token has been revoked this mean when the user logout or the refresh token has been compromised, we should not allow the user to use it anymore.
            if (LoginUser.RefreshTokenRevokedAt != null)
                return Unauthorized("Refresh Token has been revoked");

            if (LoginUser.RefreshTokenExpiryTime == null || LoginUser.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized("Refresh Token has expired");

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.ResreshToken, LoginUser.RefreshToken);
            if (!refreshValid)
                return Unauthorized("Invalid Refresh Token");



            // Issue NEW access token (same claims & signing settings as login)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, LoginUser.UserID.ToString()),
                new Claim(ClaimTypes.Email, LoginUser.Email),
                new Claim(ClaimTypes.Role, LoginUser.Role)
            };

            var secretKey = _configuration["JwtSettings:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(10),
                signingCredentials: creds
            );

            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            // Rotation: replace refresh token
            var newRefreshToken = RefreshTokenGenerator();
            LoginUser.RefreshToken = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
            LoginUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            LoginUser.RefreshTokenRevokedAt = null;

            return Ok(new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            LoginUserDataDTO? LoginUser = await Users.LoginUserByUserNameAndPassword(request.Username, request.Password);

            if (LoginUser == null)
                return Ok();

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, LoginUser.RefreshToken);
            if (!refreshValid)
                return Ok();

            LoginUser.RefreshTokenRevokedAt = DateTime.UtcNow;
            await Users.RevokeRefreshToken(LoginUser.UserName);
            return Ok("Logged out successfully");
        }

    }


}
