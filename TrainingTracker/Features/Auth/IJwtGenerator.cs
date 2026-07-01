using TrainingTracker.Models.Entities;

namespace TrainingTracker.Features.Auth
{
    public interface IJwtGenerator
    {
        string GenerateAccessToken(User user);
    }
}
