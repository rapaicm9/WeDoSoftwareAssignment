using AutoMapper;
using TrainingTracker.Models.Entities;

namespace TrainingTracker.Features.Auth.Register
{
    public sealed class RegisterMappingProfile : Profile
    {
        public RegisterMappingProfile() 
        {
            CreateMap<RegisterRequest, RegisterCommand>();

            CreateMap<User, RegisterResponse>();
        }
    }
}
