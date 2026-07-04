using AutoMapper;
using TrainingTracker.Common.CQRS;
using TrainingTracker.Api.Database;
using TrainingTracker.Common.Results;
using TrainingTracker.Models.Entities;

namespace TrainingTracker.Features.Workouts.AddWorkout
{
    public sealed class AddWorkoutCommandHandler
        : ICommandHandler<AddWorkoutCommand, AddWorkoutResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<AddWorkoutCommandHandler> _logger;

        public AddWorkoutCommandHandler(
            AppDbContext dbContext,
            IMapper mapper,
            ILogger<AddWorkoutCommandHandler> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<AddWorkoutResponse>> Handle(
            AddWorkoutCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var workout = new Workout
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Title = request.Title.Trim(),
                    Type = request.WorkoutType,
                    DurationMinutes = request.DurationMinutes,
                    CaloriesBurned = request.CaloriesBurned,
                    TrainingIntensity = request.TrainingIntensity,
                    Fatigue = request.Fatigue,
                    Notes = string.IsNullOrWhiteSpace(request.Notes)
                        ? null
                        : request.Notes.Trim(),
                    TrainingDateTimeUtc = request.TrainingDateTimeUtc,
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _dbContext.Workouts.AddAsync(workout, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var response = _mapper.Map<AddWorkoutResponse>(workout);

                return Result<AddWorkoutResponse>.Success(response);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating workout for User ID {UserId}", request.UserId);

                throw;
            }

        }
    }
}
