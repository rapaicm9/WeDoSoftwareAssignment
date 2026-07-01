using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingTracker.Common.Http;
using TrainingTracker.Features.Workouts.AddWorkout;

namespace TrainingTracker.Features.Workouts
{
    [ApiController]
    [Authorize]
    [Route("api/workouts")]
    public sealed class WorkoutsController : ControllerBase
    {
        private readonly ISender _sender;

        public WorkoutsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        [ProducesResponseType(typeof(AddWorkoutResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AddWorkoutResponse>> AddWorkout(
        AddWorkoutRequest request,
        CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var command = new AddWorkoutCommand(
                UserId: userId,
                Title: request.Title,
                WorkoutType: request.WorkoutType,
                DurationMinutes: request.DurationMinutes,
                CaloriesBurned: request.CaloriesBurned,
                TrainingIntensity: request.TrainingIntensity,
                Fatigue: request.Fatigue,
                Notes: request.Notes,
                TrainingDateTimeUtc: request.TrainingDateTimeUtc);

            var result = await _sender.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status201Created, result.Value);
            }

            return result.ToActionResult(
                controller: this,
                successStatusCode: StatusCodes.Status201Created);
        }
    }
}
