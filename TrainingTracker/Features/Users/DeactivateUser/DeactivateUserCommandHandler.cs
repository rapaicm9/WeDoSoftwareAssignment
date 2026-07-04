using Microsoft.EntityFrameworkCore;
using TrainingTracker.Database;
using TrainingTracker.Common.CQRS;
using TrainingTracker.Common.Results;

namespace TrainingTracker.Features.Users.DeactivateUser
{
    public sealed class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand, UserResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<DeactivateUserCommandHandler> _logger;

        public DeactivateUserCommandHandler(
            AppDbContext dbContext,
            ILogger<DeactivateUserCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<UserResponse>> Handle(
            DeactivateUserCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(user => user.Id == request.UserId, cancellationToken);

                if (user is null)
                {
                    _logger.LogWarning(
                        "User deactivation failed. User ID {UserId} was not found.",
                        request.UserId);

                    return Result<UserResponse>.Failure(new Error(
                        Code: "Users.NotFound",
                        Message: "User was not found",
                        Type: ErrorType.NotFound));
                }

                if (!user.IsActive)
                {
                    _logger.LogInformation(
                        "User deactivation skipped. User ID {UserId} is already inactive.",
                        request.UserId);

                    return Result<UserResponse>.Failure(new Error(
                        Code: "Users.UserInactive",
                        Message: "User is already inactive",
                        Type: ErrorType.Inactive));
                }

                user.IsActive = false;
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
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while deactivating user with ID {UserId}.",
                    request.UserId);

                return Result<UserResponse>.Failure(new Error(
                    Code: "Users.DeactivateFailed",
                    Message: "An error occurred while deactivating the user",
                    Type: ErrorType.Failure));
            }
        }
    }
}
