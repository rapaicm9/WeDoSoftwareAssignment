using FluentValidation;

namespace TrainingTracker.Features.Workouts.GetWorkouts
{
    public sealed class GetWorkoutsQueryValidator : AbstractValidator<GetWorkoutsQuery>
    {
        public GetWorkoutsQueryValidator()
        {
            RuleFor(query => query.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");
        }
    }
}
