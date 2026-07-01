using FluentValidation;

namespace TrainingTracker.Features.Workouts.AddWorkout
{
    public sealed class AddWorkoutCommandValidator : AbstractValidator<AddWorkoutCommand>
    {
        public AddWorkoutCommandValidator() 
        {
            RuleFor(command => command.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(command => command.Title)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Title is required")
                .MaximumLength(200)
                .WithMessage("Title must not exceed 200 characters");

            RuleFor(command => command.WorkoutType)
                .IsInEnum()
                .WithMessage("Workout Type is invalid");

            RuleFor(command => command.DurationMinutes)
                .GreaterThan(0)
                .WithMessage("Duration must be greater than zero");

            RuleFor(command => command.CaloriesBurned)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Calories burned must not be negative");

            RuleFor(command => command.TrainingIntensity)
                .InclusiveBetween(1, 10)
                .WithMessage("Training intensity must be between 1 and 10");

            RuleFor(command => command.Fatigue)
                .InclusiveBetween(1, 10)
                .WithMessage("Fatigue must be between 1 and 10");

            RuleFor(command => command.Notes)
                .MaximumLength(2000)
                .WithMessage("Notes must not exceed 2000 characters")
                .When(command => !string.IsNullOrWhiteSpace(command.Notes));

            RuleFor(command => command.TrainingDateTimeUtc)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Training date is required")
                .Must(date => date.Kind == DateTimeKind.Utc)
                .WithMessage("Training date must be in UTC");
        }
    }
}
