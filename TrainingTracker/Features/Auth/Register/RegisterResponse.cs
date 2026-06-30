namespace TrainingTracker.Features.Auth.Register
{
    public sealed record RegisterResponse
    {
        public Guid Id { get; init; }

        public string Email { get; init; } = string.Empty;

        public string FirstName { get; init; } = string.Empty;

        public string LastName { get; init; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
