using AutoMapper;
using TrainingTracker.Models.Entities;

namespace TrainingTracker.Features.Users
{
    public sealed class UsersMappingProfile : Profile
    {
        public UsersMappingProfile()
        {
            CreateMap<User, UserResponse>();
        }
    }
}
