namespace TrainingTracker.Features.Auth.Login
{
    public sealed record LoginResponse(
        string AccessToken,
        string TokenType,
        int ExpiresInSeconds,
        Guid UserId,
        string Email,
        string FirstName,
        string LastName,
        string FullName);
}
