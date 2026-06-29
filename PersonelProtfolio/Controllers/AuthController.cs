using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

// The following namespaces are used for JWT token creation and password hashing.
using PersonalProtfolioDataTier;
using PersonalProtfolioBusniessTier;
//

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace PersonelProtfolio.Controllers
{
    [Route("api/PersonelProtfolio")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // This endpoint handles user login.
        // It verifies credentials and returns a JWT token if login succeeds.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] PersonalProtfolioDataTier.LoginRequest request)
        {
            // Step 1: Find the student by email from the in-memory data store.
            // Email acts as the unique login identifier.
            PersonalProtfolioDataTier.UserDataDTO? LoginUser = await PersonalProtfolioBusniessTier.Users.LoginUserByUserNameAndPassword(request.UserName, request.Password);
              
            if (LoginUser == null)
                return Unauthorized("Invalid credentials");

            var claims = new[]
            {                
                new Claim(ClaimTypes.NameIdentifier, LoginUser.UserID.ToString()),
                new Claim(ClaimTypes.Name, LoginUser.UserName),
                new Claim(ClaimTypes.Role, LoginUser.Role)
            };


            // Step 3: Create the symmetric security key used to sign the JWT.
            // This key must match the key used in JWT validation middleware.
            // This key will stored in save file and read from there in real application, but for simplicity we hardcode it here. 
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("THIS_IS_A_VERY_SECRET_KEY_123456"));

            // Step 4: Define the signing credentials.
            // This specifies the algorithm used to sign the token.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            // Step 5: Create the JWT token.
            // The token includes issuer, audience, claims, expiration, and signature.
            var token = new JwtSecurityToken(
                issuer: "PersonelProtfolio",
                audience: "PersonelProtfolioUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            // Step 6: Return the serialized JWT token to the client.
            // The client will send this token with future requests.
            return Ok ( new{token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
