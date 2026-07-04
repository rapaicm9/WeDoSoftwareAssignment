using TrainingTracker.Common.CQRS;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TrainingTracker.Database;
using TrainingTracker.Common.Results;
using TrainingTracker.Models.Entities;

namespace TrainingTracker.Features.Auth.Login
{
    public sealed class LoginCommandHandler
        : ICommandHandler<LoginCommand, LoginResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            AppDbContext dbContext,
            IPasswordHasher<User> passwordHasher,
            IJwtGenerator jwtGenerator,
            IOptions<JwtOptions> jwtOptions,
            ILogger<LoginCommandHandler> logger)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _jwtGenerator = jwtGenerator;
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
        }

        public async Task<Result<LoginResponse>> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToUpperInvariant();

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    user => user.NormalizedEmail == normalizedEmail,
                    cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("Login failed. User with email {Email} was not found.", normalizedEmail);

                return InvalidCredentialsResult();
            }

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Login failed. Invalid password for User ID {UserId}", user.Id);

                return InvalidCredentialsResult();
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed. User ID {UserId} is inactive.", user.Id);

                return UserInactiveResult();
            }

            var accessToken = _jwtGenerator.GenerateAccessToken(user);

            var response = new LoginResponse(
                AccessToken: accessToken,
                TokenType: "Bearer",
                ExpiresInSeconds: _jwtOptions.AccessTokenExpirationMinutes * 60,
                UserId: user.Id,
                Email: user.Email,
                FirstName: user.FirstName,
                LastName: user.LastName,
                FullName: $"{user.FirstName} {user.LastName}".Trim());

            return Result<LoginResponse>.Success(response);
        }

        private static Result<LoginResponse> InvalidCredentialsResult()
        {
            return Result<LoginResponse>.Failure(new Error(
                Code: "Auth.InvalidCredentials",
                Message: "Invalid email or password.",
                Type: ErrorType.Unauthorized));
        }

        private static Result<LoginResponse> UserInactiveResult()
        {
            return Result<LoginResponse>.Failure(new Error(
                Code: "Users.UserInactive",
                Message: "User is inactive",
                Type: ErrorType.Inactive));
        }
    }
}
