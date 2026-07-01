using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TrainingTracker.Models.Entities;

namespace TrainingTracker.Features.Auth
{
    public sealed class JwtGenerator : IJwtGenerator
    {
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger<JwtGenerator> _logger;

        public JwtGenerator(
            IOptions<JwtOptions> jwtOptions,
            ILogger<JwtGenerator> logger)
        {
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
        }

        public string GenerateAccessToken(User user)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_jwtOptions.Secret))
                    throw new InvalidOperationException("JWT Secret is not configured.");

                if (_jwtOptions.Secret.Length < 32)
                    throw new InvalidOperationException("JWT Secret must be at least 32 characters long.");

                var signingKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtOptions.Secret));

                var credentials = new SigningCredentials(
                    signingKey,
                    SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new(JwtRegisteredClaimNames.Email, user.Email),
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Email, user.Email),
                    new("firstName", user.FirstName),
                    new("lastName", user.LastName)
                };

                var expiresAtUtc = DateTime.UtcNow.AddMinutes(
                    _jwtOptions.AccessTokenExpirationMinutes);

                var token = new JwtSecurityToken(
                    issuer: _jwtOptions.Issuer,
                    audience: _jwtOptions.Audience,
                    claims: claims,
                    expires: expiresAtUtc,
                    signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch(Exception exception)
            {
                _logger.LogError(exception, "Failed to generate JWT access token for user ID {UserId}", user.Id);
                throw;
            }
        }
    }
}
