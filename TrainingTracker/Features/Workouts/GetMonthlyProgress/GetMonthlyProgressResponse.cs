namespace TrainingTracker.Features.Workouts.GetMonthlyProgress
{
    public sealed class GetMonthlyProgressResponse
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public IReadOnlyList<WeeklyProgressResponse> Weeks { get; set; } = [];
    }

    public sealed class WeeklyProgressResponse
    {
        public int WeekNumber { get; set; }
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
        public int TotalDurationMinutes { get; set; }
        public int WorkoutCount { get; set; }
        public double? AverageTrainingIntensity { get; set; }
        public double? AverageFatigue {  get; set; }
    }
}
