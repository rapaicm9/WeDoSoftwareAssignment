using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingTracker.Common.Http;
using TrainingTracker.Features.Workouts.AddWorkout;
using TrainingTracker.Features.Workouts.DeleteWorkout;
using TrainingTracker.Features.Workouts.GetMonthlyProgress;
using TrainingTracker.Features.Workouts.GetWorkouts;

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

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<WorkoutResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<WorkoutResponse>>> GetWorkouts(
        CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId))
                return Unauthorized();

            var query = new GetWorkoutsQuery(userId);

            var result = await _sender.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Ok(result.Value);

            return result.ToActionResult(
                controller: this,
                successStatusCode: StatusCodes.Status200OK);
        }

        [HttpGet("progress/monthly")]
        [ProducesResponseType(typeof(GetMonthlyProgressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetMonthlyProgressResponse>> GetMonthlyProgress(
            [FromQuery] int year,
            [FromQuery] int month,
            CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId))
                return Unauthorized();

            var query = new GetMonthlyProgressQuery(
                UserId: userId,
                Year: year,
                Month: month);

            var result = await _sender.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Ok(result.Value);

            return result.ToActionResult(
                controller: this,
                successStatusCode: StatusCodes.Status200OK);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DeleteWorkoutResponse>> DeleteWorkout(
        Guid id,
        CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId))
                return Unauthorized();

            var command = new DeleteWorkoutCommand(
                UserId: userId,
                WorkoutId: id);

            var result = await _sender.Send(command, cancellationToken);

            if (result.IsSuccess)
                return NoContent();

            return result.ToActionResult(
                controller: this,
                successStatusCode: StatusCodes.Status204NoContent);
        }

        private bool TryGetAuthenticatedUserId(out Guid userId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdClaim, out userId);
        }
    }
}
