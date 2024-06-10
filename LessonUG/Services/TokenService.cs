using LessonUG.Interfaces;
using LessonUG.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LessonUG.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public TokenService(UserManager<User> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        async public Task<string> GenerateJwtToken(User user, TimeSpan expiration)
        {
            // Define the token claims (email and unique guid)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, user.SchoolIndex)
            };

            // Add the roles to the token
            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                claims.Add(new Claim("role", role));
            }

            // Encode our private JWT key
            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Put everything together
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.UtcNow.Add(expiration),
                claims: claims,
                signingCredentials: creds
            );

            // Build the token as a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task<string> GenerateJwtToken(User user)
        {
            throw new NotImplementedException();
        }
    }
}
