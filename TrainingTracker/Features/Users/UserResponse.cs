namespace TrainingTracker.Features.Users
{
    public sealed record UserResponse(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        bool IsActive,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);
}
