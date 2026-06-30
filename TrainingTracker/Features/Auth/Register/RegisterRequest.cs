namespace TrainingTracker.Features.Auth.Register
{
    public sealed record RegisterRequest
    {
        public string Email { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;

        public string FirstName { get; init; } = string.Empty;

        public string LastName { get; init; } = string.Empty;
    }
}
