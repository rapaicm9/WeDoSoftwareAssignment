using TrainingTracker.Common.CQRS;
using Microsoft.EntityFrameworkCore;
using TrainingTracker.Api.Database;
using TrainingTracker.Common.Results;

namespace TrainingTracker.Features.Workouts.GetMonthlyProgress
{
    public sealed class GetMonthlyProgressQueryHandler
        : IQueryHandler<GetMonthlyProgressQuery, GetMonthlyProgressResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<GetMonthlyProgressQueryHandler> _logger;

        public GetMonthlyProgressQueryHandler(AppDbContext dbContext, ILogger<GetMonthlyProgressQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<GetMonthlyProgressResponse>> Handle(
            GetMonthlyProgressQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (request.Year is < 1 or > 9999)
                    return InvalidYearResult();

                if (request.Month is < 1 or > 12)
                    return InvalidMonthResult();

                var monthStartUtc = new DateTime(
                    request.Year,
                    request.Month,
                    day: 1,
                    hour: 0,
                    minute: 0,
                    second: 0,
                    kind: DateTimeKind.Utc);

                var monthEndUtc = monthStartUtc.AddMonths(1);

                var workouts = await _dbContext.Workouts
                    .AsNoTracking()
                    .Where(workout =>
                        workout.UserId == request.UserId &&
                        workout.TrainingDateTimeUtc >= monthStartUtc &&
                        workout.TrainingDateTimeUtc < monthEndUtc)
                    .Select(workout => new WorkoutProgressData(
                        workout.TrainingDateTimeUtc,
                        workout.DurationMinutes,
                        workout.TrainingIntensity,
                        workout.Fatigue))
                    .ToListAsync(cancellationToken);

                var weekBuckets = CreateWeekBuckets(request.Year, request.Month);

                var weeklyProgress = weekBuckets
                    .Select(bucket =>
                    {

                        var workoutsInWeek = workouts
                            .Where(workouts =>
                            {
                                var trainingDate = DateOnly.FromDateTime(workouts.TrainingDateTimeUtc);

                                return trainingDate >= bucket.StartDate &&
                                       trainingDate <= bucket.EndDate;
                            })
                            .ToList();

                        return new WeeklyProgressResponse
                        {
                            WeekNumber = bucket.WeekNumber,
                            WeekStartDate = bucket.StartDate,
                            WeekEndDate = bucket.EndDate,
                            TotalDurationMinutes = workoutsInWeek.Sum(workout => workout.DurationMinutes),
                            WorkoutCount = workoutsInWeek.Count,
                            AverageTrainingIntensity = workoutsInWeek.Count == 0
                                ? null
                                : Math.Round(workoutsInWeek.Average(workout => workout.TrainingIntensity), 2),
                            AverageFatigue = workoutsInWeek.Count == 0
                                ? null
                                : Math.Round(workoutsInWeek.Average(workout => workout.Fatigue), 2)
                        };

                    })
                    .ToList();

                var response = new GetMonthlyProgressResponse
                {
                    Year = request.Year,
                    Month = request.Month,
                    Weeks = weeklyProgress
                };

                return Result<GetMonthlyProgressResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving monthly progress for User ID {UserId}, Year {Year}, Month {Month}", request.UserId, request.Year, request.Month);

                throw;
            }
        }

        private sealed record WeekBucket(
            int WeekNumber,
            DateOnly StartDate,
            DateOnly EndDate);

        private sealed record WorkoutProgressData(
            DateTime TrainingDateTimeUtc,
            int DurationMinutes,
            int TrainingIntensity,
            int Fatigue);

        private static IReadOnlyList<WeekBucket> CreateWeekBuckets(int year, int month)
        {
            var firstDayOfMonth = new DateOnly(year, month, day: 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var buckets = new List<WeekBucket>();

            var currentStartDate = firstDayOfMonth;
            var weekNumber = 1;

            while(currentStartDate <= lastDayOfMonth)
            {
                var daysUntilSunday = GetDaysUntilSunday(currentStartDate.DayOfWeek);
                var currentEndDate = currentStartDate.AddDays(daysUntilSunday);

                if (currentEndDate > lastDayOfMonth)
                    currentEndDate = lastDayOfMonth;

                buckets.Add(new WeekBucket(
                    WeekNumber: weekNumber,
                    StartDate: currentStartDate,
                    EndDate: currentEndDate));

                currentStartDate = currentEndDate.AddDays(1);
                weekNumber++;
            }

            return buckets;
        }

        private static int GetDaysUntilSunday(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => 6,
                DayOfWeek.Tuesday => 5,
                DayOfWeek.Wednesday => 4,
                DayOfWeek.Thursday => 3,
                DayOfWeek.Friday => 2,
                DayOfWeek.Saturday => 1,
                DayOfWeek.Sunday => 0,
                _ => 0
            };
        }

        private static Result<GetMonthlyProgressResponse> InvalidYearResult()
        {
            return Result<GetMonthlyProgressResponse>.Failure(new Error(
                Code: "Progress.InvalidYear",
                Message: "Year must be between 1 and 9999",
                Type: ErrorType.Validation));
        }

        private static Result<GetMonthlyProgressResponse> InvalidMonthResult()
        {
            return Result<GetMonthlyProgressResponse>.Failure(new Error(
                Code: "Progress.InvalidMonth",
                Message: "Month must be between 1 and 12",
                Type: ErrorType.Validation));
        }
    }
}
