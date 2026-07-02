using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TrainingTracker.Api.Database;
using TrainingTracker.Common.CQRS;
using TrainingTracker.Common.Results;
using TrainingTracker.Models.Entities;


namespace TrainingTracker.Features.Users.CreateUser
{
    public sealed class CreateUserCommandHandler
        : ICommandHandler<CreateUserCommand, UserResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<CreateUserCommandHandler> _logger;

        public CreateUserCommandHandler(
            AppDbContext dbContext,
            IPasswordHasher<User> passwordHasher,
            ILogger<CreateUserCommandHandler> logger)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<Result<UserResponse>> Handle(
            CreateUserCommand request,
            CancellationToken cancellationToken)
        {
            if (HasInvalidRequiredFields(request))
            {
                return Result<UserResponse>.Failure(new Error(
                    Code: "Users.InvalidCreateRequest",
                    Message: "Email, password, first name and last name are required",
                    Type: ErrorType.Validation));
            }

            var normalizedEmail = request.Email.Trim().ToUpperInvariant();

            try
            {
                var emailAlreadyExists = await _dbContext.Users
                    .AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

                if (emailAlreadyExists)
                    return EmailAlreadyExistsResult();

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email.Trim(),
                    NormalizedEmail = normalizedEmail,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    IsActive = request.IsActive,
                    CreatedAtUtc = DateTime.UtcNow
                };

                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

                _dbContext.Users.Add(user);

                await _dbContext.SaveChangesAsync(cancellationToken);

                var response = new UserResponse(
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.IsActive,
                    user.CreatedAtUtc,
                    user.UpdatedAtUtc);

                return Result<UserResponse>.Success(response);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _logger.LogWarning(ex, "User creation failed because of unique email constraint. Email: {Email}", normalizedEmail);

                return EmailAlreadyExistsResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating user with email {Email}", normalizedEmail);

                return Result<UserResponse>.Failure(new Error(
                    Code: "Users.CreateFailed",
                    Message: "An error occurred while creating the user",
                    Type: ErrorType.Failure));
            }
        }

        private static bool HasInvalidRequiredFields(CreateUserCommand request)
        {
            return string.IsNullOrWhiteSpace(request.Email)
                || string.IsNullOrWhiteSpace(request.Password)
                || string.IsNullOrWhiteSpace(request.FirstName)
                || string.IsNullOrWhiteSpace(request.LastName);
        }

        private static Result<UserResponse> EmailAlreadyExistsResult()
        {
            return Result<UserResponse>.Failure(new Error(
                Code: "Users.EmailAlreadyExists",
                Message: "A user with this email already exists",
                Type: ErrorType.Conflict));
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is PostgresException postgresException
                && postgresException.SqlState == PostgresErrorCodes.UniqueViolation;
        }
    }
}
