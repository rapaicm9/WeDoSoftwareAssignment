using AutoMapper;
using TrainingTracker.Models.Entities;

namespace TrainingTracker.Features.Workouts
{
    public sealed class WorkoutsMappingProfile : Profile
    {
        public WorkoutsMappingProfile()
        {
            CreateMap<Workout, WorkoutResponse>()
                .ForCtorParam(
                    nameof(WorkoutResponse.WorkoutType),
                    options => options.MapFrom(workout => workout.Type));
        }
    }
}
