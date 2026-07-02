namespace TrainingTracker.Features.Users.CreateUser
{
    public sealed record CreateUserRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        bool IsActive = true);
}
