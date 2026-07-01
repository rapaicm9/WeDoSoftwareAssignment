using AutoMapper;
using TrainingTracker.Models.Entities;

namespace TrainingTracker.Features.Workouts.AddWorkout;

public sealed class AddWorkoutMappingProfile : Profile
{
    public AddWorkoutMappingProfile()
    {
        CreateMap<Workout, AddWorkoutResponse>()
            .ConstructUsing(workout => new AddWorkoutResponse(
                workout.Id,
                workout.UserId,
                workout.Title,
                workout.Type,
                workout.DurationMinutes,
                workout.CaloriesBurned,
                workout.TrainingIntensity,
                workout.Fatigue,
                workout.Notes,
                workout.TrainingDateTimeUtc,
                workout.CreatedAtUtc));
    }
}
