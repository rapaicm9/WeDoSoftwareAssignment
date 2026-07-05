using AutoMapper;
using TrainingTracker.Models.Entities;

namespace TrainingTracker.Features.Workouts.AddWorkout
{

    public sealed class AddWorkoutMappingProfile : Profile
    {
        public AddWorkoutMappingProfile()
        {
            CreateMap<Workout, AddWorkoutResponse>()
                .ForCtorParam(
                    nameof(AddWorkoutResponse.WorkoutType),
                    options => options.MapFrom(workout => workout.Type));
        }
    }
}