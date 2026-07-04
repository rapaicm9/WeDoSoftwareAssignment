using Microsoft.EntityFrameworkCore;
using TrainingTracker.Api.Database;
using TrainingTracker.Common.CQRS;
using TrainingTracker.Common.Results;

namespace TrainingTracker.Features.Workouts.DeleteWorkout
{
    public sealed class DeleteWorkoutCommandHandler
        : ICommandHandler<DeleteWorkoutCommand, DeleteWorkoutResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<DeleteWorkoutCommandHandler> _logger;

        public DeleteWorkoutCommandHandler(
            AppDbContext dbContext,
            ILogger<DeleteWorkoutCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<DeleteWorkoutResponse>> Handle(
            DeleteWorkoutCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var workout = await _dbContext.Workouts
                    .Where(workout =>
                        workout.Id == request.WorkoutId &&
                        workout.UserId == request.UserId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (workout is null)
                {
                    _logger.LogWarning(
                        "Workout deletion failed. Workout ID {WorkoutId} was not found for User ID {UserId}.",
                        request.WorkoutId,
                        request.UserId);

                    return WorkoutNotFoundResult();
                }

                _dbContext.Workouts.Remove(workout);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result<DeleteWorkoutResponse>.Success(
                    new DeleteWorkoutResponse(workout.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while deleting Workout ID {WorkoutId} for User ID {UserId}",
                    request.WorkoutId,
                    request.UserId);

                throw;
            }
        }
        private static Result<DeleteWorkoutResponse> WorkoutNotFoundResult()
        {
            return Result<DeleteWorkoutResponse>.Failure(new Error(
                Code: "Workouts.NotFound",
                Message: "Workout was not found.",
                Type: ErrorType.NotFound));
        }
    }
}
