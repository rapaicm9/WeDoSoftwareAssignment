using Microsoft.EntityFrameworkCore;
using TrainingTracker.Api.Database;
using TrainingTracker.Common.CQRS;
using TrainingTracker.Common.Results;

namespace TrainingTracker.Features.Users.GetAllUsers
{
    public sealed class GetAllUsersQueryHandler
        : IQueryHandler<GetAllUsersQuery, IReadOnlyCollection<UserResponse>>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<GetAllUsersQueryHandler> _logger;

        public GetAllUsersQueryHandler(AppDbContext dbContext, ILogger<GetAllUsersQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyCollection<UserResponse>>> Handle(
            GetAllUsersQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var users = await _dbContext.Users
                    .AsNoTracking()
                    .OrderBy(user => user.Email)
                    .Select(user => new UserResponse(
                        user.Id,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.IsActive,
                        user.CreatedAtUtc,
                        user.UpdatedAtUtc))
                    .ToListAsync(cancellationToken);

                return Result<IReadOnlyCollection<UserResponse>>.Success(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all users.");

                return Result<IReadOnlyCollection<UserResponse>>.Failure(new Error(
                    Code: "Users.GetAllFailed",
                    Message: "An error occurred while getting all users",
                    Type: ErrorType.Failure));
            }
        }
    }
}
