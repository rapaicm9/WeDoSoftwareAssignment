using FluentValidation;
using TrainingTracker.Common.Validators;

namespace TrainingTracker.Features.Users.UpdateUser
{
    public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(command => command.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(command => command)
                .Must(HasAtLeastOneUpdate)
                .OverridePropertyName("Update")
                .WithMessage("At least one user detail must be provided for update");

            RuleFor(command => command.FirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("First name must not be empty")
                .MaximumLength(200)
                .WithMessage("First name must not exceed 200 characters")
                .When(command => command.FirstName is not null);

            RuleFor(command => command.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Last name must not be empty")
                .MaximumLength(200)
                .WithMessage("Last name must not exceed 200 characters")
                .When(command => command.LastName is not null);

            RuleFor(command => command.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email must not be empty")
                .EmailAddress()
                .WithMessage("Email format is invalid")
                .MaximumLength(256)
                .WithMessage("Email must not exceed 256 characters")
                .When(command => command.Email is not null);

            RuleFor(command => command.CurrentPassword)
                .NotEmpty()
                .WithMessage("Current password is required when changing password")
                .When(command => command.NewPassword is not null);

            RuleFor(command => command.NewPassword)
                .ValidPassword("New password")
                .When(command => command.NewPassword is not null);

            RuleFor(command => command)
                .Must(command => command.CurrentPassword is null || command.NewPassword is not null)
                .OverridePropertyName(nameof(UpdateUserCommand.NewPassword))
                .WithMessage("New password is required when current password is provided");
        }

        private static bool HasAtLeastOneUpdate(UpdateUserCommand command)
        {
            return command.FirstName is not null ||
                   command.LastName is not null ||
                   command.Email is not null ||
                   command.NewPassword is not null;
        }
    }
}
