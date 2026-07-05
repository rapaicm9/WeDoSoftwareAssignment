using Microsoft.EntityFrameworkCore;
using TrainingTracker.Database;
using TrainingTracker.Common.CQRS;
using TrainingTracker.Common.Results;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace TrainingTracker.Features.Workouts.GetWorkouts
{
    public sealed class GetWorkoutsQueryHandler
        : IQueryHandler<GetWorkoutsQuery, IReadOnlyCollection<WorkoutResponse>>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<GetWorkoutsQueryHandler> _logger;
        private readonly IMapper _mapper;

        public GetWorkoutsQueryHandler(
            AppDbContext dbContext,
            ILogger<GetWorkoutsQueryHandler> logger,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<IReadOnlyCollection<WorkoutResponse>>> Handle(
            GetWorkoutsQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .Where(user => user.Id == request.UserId)
                .Select(user => new
                {
                    user.Id,
                    user.IsActive
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                _logger.LogWarning(
                    "Workout retrieval failed. User ID {UserId} was not found.",
                    request.UserId);

                return UserNotFoundResult();
            }

            if (!user.IsActive)
            {
                _logger.LogWarning(
                    "Workout retrieval failed. User ID {UserId} is inactive.",
                    request.UserId);

                return UserInactiveResult();
            }

            var workouts = await _dbContext.Workouts
                .AsNoTracking()
                .Where(workout => workout.UserId == request.UserId)
                .OrderByDescending(workout => workout.TrainingDateTimeUtc)
                .ThenByDescending(workout => workout.CreatedAtUtc)
                .ProjectTo<WorkoutResponse>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyCollection<WorkoutResponse>>.Success(workouts);
        }

        private static Result<IReadOnlyCollection<WorkoutResponse>> UserNotFoundResult()
        {
            return Result<IReadOnlyCollection<WorkoutResponse>>.Failure(new Error(
                Code: "Auth.AuthenticatedUserNotFound",
                Message: "Authenticated user could not be found.",
                Type: ErrorType.Unauthorized));
        }

        private static Result<IReadOnlyCollection<WorkoutResponse>> UserInactiveResult()
        {
            return Result<IReadOnlyCollection<WorkoutResponse>>.Failure(new Error(
                Code: "Users.UserInactive",
                Message: "User is inactive.",
                Type: ErrorType.Inactive));
        }
    }
}
