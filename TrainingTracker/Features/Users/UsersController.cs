using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingTracker.Common.Http;
using TrainingTracker.Features.Users.GetAllUsers;
using TrainingTracker.Features.Users.GetUserById;
using TrainingTracker.Features.Users.CreateUser;

namespace TrainingTracker.Features.Users
{
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly ISender _sender;

        public UsersController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponse>> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery(id);

            var result = await _sender.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Ok(result.Value);

            return result.ToActionResult(
                controller: this,
                successStatusCode: StatusCodes.Status200OK);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<UserResponse>>> GetAllUsers(CancellationToken cancellationToken)
        {
            var query = new GetAllUsersQuery();

            var result = await _sender.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Ok(result.Value);

            return result.ToActionResult(
                controller: this,
                successStatusCode: StatusCodes.Status200OK);
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponse>> CreateUser(
            CreateUserRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.IsActive);

            var result = await _sender.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    actionName: nameof(GetUserById),
                    routeValues: new { id = result.Value.Id },
                    value: result.Value);
            }

            return result.ToActionResult(
                controller: this,
                successStatusCode: StatusCodes.Status201Created);
        }
    }
}
