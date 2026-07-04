using FluentValidation;

namespace TrainingTracker.Features.Workouts.DeleteWorkout
{
    public sealed class DeleteWorkoutCommandValidator : AbstractValidator<DeleteWorkoutCommand>
    {
        public DeleteWorkoutCommandValidator()
        {
            RuleFor(command => command.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(command => command.WorkoutId)
                .NotEmpty()
                .WithMessage("Workout ID is required");
        }
    }
}
