using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TrainingTracker.Api.Database;
using TrainingTracker.Common.CQRS;
using TrainingTracker.Common.Results;
using TrainingTracker.Models.Entities;

namespace TrainingTracker.Features.Users.UpdateUser
{
    public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<UpdateUserCommandHandler> _logger;

        public UpdateUserCommandHandler(
            AppDbContext dbContext,
            IPasswordHasher<User> passwordHasher,
            ILogger<UpdateUserCommandHandler> logger)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<Result<UserResponse>> Handle(
            UpdateUserCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(user => user.Id == request.UserId, cancellationToken);

                if (user is null)
                {
                    _logger.LogWarning(
                        "User update failed. User ID {UserId} was not found.",
                        request.UserId);

                    return UserNotFoundResult();
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning(
                        "User update failed. User ID {UserId} is inactive.",
                        request.UserId);

                    return UserInactiveResult();
                }

                if (request.Email is not null)
                {
                    var normalizedEmail = request.Email.Trim().ToUpperInvariant();

                    var emailAlreadyExists = await _dbContext.Users
                        .AnyAsync(
                            existingUser =>
                                existingUser.NormalizedEmail == normalizedEmail &&
                                existingUser.Id != request.UserId,
                            cancellationToken);

                    if (emailAlreadyExists)
                    {
                        return EmailAlreadyExistsResult();
                    }

                    user.Email = request.Email.Trim();
                    user.NormalizedEmail = normalizedEmail;
                }

                if (request.FirstName is not null)
                {
                    user.FirstName = request.FirstName.Trim();
                }

                if (request.LastName is not null)
                {
                    user.LastName = request.LastName.Trim();
                }

                if (request.NewPassword is not null)
                {
                    var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                        user,
                        user.PasswordHash,
                        request.CurrentPassword!);

                    if (passwordVerificationResult == PasswordVerificationResult.Failed)
                    {
                        _logger.LogWarning(
                            "User update failed. Invalid current password for User ID {UserId}.",
                            request.UserId);

                        return InvalidCurrentPasswordResult();
                    }

                    user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
                }

                user.UpdatedAtUtc = DateTime.UtcNow;

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
                _logger.LogWarning(
                    ex,
                    "User update failed because of unique email constraint. User ID {UserId}.",
                    request.UserId);

                return EmailAlreadyExistsResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while updating user with ID {UserId}.",
                    request.UserId);

                return Result<UserResponse>.Failure(new Error(
                    Code: "Users.UpdateFailed",
                    Message: "An error occurred while updating the user",
                    Type: ErrorType.Failure));
            }
        }

        private static Result<UserResponse> UserNotFoundResult()
        {
            return Result<UserResponse>.Failure(new Error(
                Code: "Users.NotFound",
                Message: "User was not found",
                Type: ErrorType.NotFound));
        }

        private static Result<UserResponse> UserInactiveResult()
        {
            return Result<UserResponse>.Failure(new Error(
                Code: "Users.UserInactive",
                Message: "User is inactive",
                Type: ErrorType.Inactive));
        }

        private static Result<UserResponse> EmailAlreadyExistsResult()
        {
            return Result<UserResponse>.Failure(new Error(
                Code: "Users.EmailAlreadyExists",
                Message: "A user with this email already exists",
                Type: ErrorType.Conflict));
        }

        private static Result<UserResponse> InvalidCurrentPasswordResult()
        {
            return Result<UserResponse>.Failure(new Error(
                Code: "Users.InvalidCurrentPassword",
                Message: "Current password is invalid",
                Type: ErrorType.Unauthorized));
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is PostgresException postgresException &&
                   postgresException.SqlState == PostgresErrorCodes.UniqueViolation;
        }
    }
}