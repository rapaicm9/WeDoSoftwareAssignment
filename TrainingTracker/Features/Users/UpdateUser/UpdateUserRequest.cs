namespace TrainingTracker.Features.Users.UpdateUser
{
    public sealed record UpdateUserRequest(
        string? FirstName,
        string? LastName,
        string? Email,
        string? CurrentPassword,
        string? NewPassword);
}
