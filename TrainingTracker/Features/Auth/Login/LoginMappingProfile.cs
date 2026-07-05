using AutoMapper;

namespace TrainingTracker.Features.Auth.Login
{
    public sealed class LoginMappingProfile : Profile
    {
        public LoginMappingProfile() 
        {
            CreateMap<LoginRequest, LoginCommand>();
        }
    }
}
