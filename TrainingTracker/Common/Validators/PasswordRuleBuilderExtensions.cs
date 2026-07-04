using FluentValidation;

namespace TrainingTracker.Common.Validators
{
    public static class PasswordRuleBuilderExtensions
    {
        public static IRuleBuilderOptions<T, string?> ValidPassword<T>(
            this IRuleBuilderInitial<T, string?> ruleBuilder,
            string fieldName = "Password")
        {
            return ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage($"{fieldName} is required")
                .MinimumLength(8)
                .WithMessage($"{fieldName} must be at least 8 characters long")
                .MaximumLength(100)
                .WithMessage($"{fieldName} must not exceed 100 characters")
                .Matches("[A-Z]")
                .WithMessage($"{fieldName} must contain at least one uppercase letter")
                .Matches("[a-z]")
                .WithMessage($"{fieldName} must contain at least one lowercase letter")
                .Matches("[0-9]")
                .WithMessage($"{fieldName} must contain at least one number")
                .Matches("[^a-zA-Z0-9]")
                .WithMessage($"{fieldName} must contain at least one special character.");
        }
    }
}
