using AutoMapper;
using TrainingTracker.Models.Entities;

namespace TrainingTracker.Features.Auth.Register
{
    public sealed class RegisterMappingProfile : Profile
    {
        public RegisterMappingProfile()
        {
            CreateMap<RegisterRequest, RegisterCommand>()
                .ConstructUsing(request => new RegisterCommand(
                    request.Email,
                    request.Password,
                    request.FirstName,
                    request.LastName));

            CreateMap<User, RegisterResponse>()
                .ConstructUsing(user => new RegisterResponse(
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    $"{user.FirstName} {user.LastName}".Trim()));
        }
    }
}
