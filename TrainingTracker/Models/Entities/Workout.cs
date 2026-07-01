using TrainingTracker.Models.Enums;

namespace TrainingTracker.Models.Entities
{
    public class Workout
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public WorkoutType Type { get; set; }
        public int DurationMinutes { get; set; }
        public int CaloriesBurned { get; set; }
        public int TrainingIntensity { get; set; }
        public int Fatigue {  get; set; }
        public string? Notes { get; set; }
        public DateTime TrainingDateTimeUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

    }
}
