using FluentValidation;
using TrainingTracker.Common.Validators;

namespace TrainingTracker.Features.Auth.Register
{
    public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator() 
        {
            RuleFor(command => command.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Email must be a valid email address")
                .MaximumLength(256)
                .WithMessage("Email must not exceed 256 characters");

            RuleFor(command => command.Password)
                .ValidPassword();

            RuleFor(command => command.FirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("First name is required")
                .MaximumLength(200)
                .WithMessage("First name must not exceed 200 characters");

            RuleFor(command => command.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Last name is required")
                .MaximumLength(200)
                .WithMessage("Last name must not exceed 200 characters");
        }
    }
}
