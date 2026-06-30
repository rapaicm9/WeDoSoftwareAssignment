using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TrainingTracker.Common.Http;
using TrainingTracker.Features.Auth.Register;

namespace TrainingTracker.Features.Auth
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public AuthController(
            ISender sender,
            IMapper mapper)
        {
            _sender = sender;
            _mapper = mapper;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RegisterResponse>> Register(
            RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var command = _mapper.Map<RegisterCommand>(request);

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
