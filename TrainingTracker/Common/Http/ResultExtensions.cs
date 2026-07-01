using Microsoft.AspNetCore.Mvc;
using TrainingTracker.Common.Results;

namespace TrainingTracker.Common.Http
{
    public static class ResultExtensions
    {
        public static ActionResult<TValue> ToActionResult<TValue>(
            this Result<TValue> result,
            ControllerBase controller,
            int successStatusCode = StatusCodes.Status200OK)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(controller);

            if (result.IsSuccess)
            {
                return controller.StatusCode(successStatusCode, result.Value);
            }

            return result.Error is null
                ? CreateUnexpectedErrorResponse<TValue>(controller)
                : CreateErrorResponse<TValue>(controller, result.Error);
        }

        private static ActionResult<TValue> CreateErrorResponse<TValue>(
            ControllerBase controller,
            Error error)
        {
            var statusCode = error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Inactive => StatusCodes.Status403Forbidden,
                ErrorType.Failure => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };

            var title = error.Type switch
            {
                ErrorType.Validation => "Validation Failed",
                ErrorType.Conflict => "Conflict",
                ErrorType.NotFound => "Not Found",
                ErrorType.Unauthorized => "Unauthorized",
                ErrorType.Inactive => "Forbidden",
                ErrorType.Failure => "Request Failed",
                _ => "Request Failed"
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = error.Message,
                Instance = controller.HttpContext.Request.Path
            };

            problemDetails.Extensions["errorCode"] = error.Code;
            problemDetails.Extensions["traceId"] = controller.HttpContext.TraceIdentifier;

            return statusCode switch
            {
                StatusCodes.Status400BadRequest => controller.BadRequest(problemDetails),
                StatusCodes.Status409Conflict => controller.Conflict(problemDetails),
                StatusCodes.Status404NotFound => controller.NotFound(problemDetails),
                StatusCodes.Status401Unauthorized => controller.Unauthorized(problemDetails),
                StatusCodes.Status403Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, problemDetails),
                _ => controller.StatusCode(statusCode, problemDetails)
            };
        }

        private static ActionResult<TValue> CreateUnexpectedErrorResponse<TValue>(
            ControllerBase controller)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Unexpected error",
                Detail = "An unexpected error occurred.",
                Instance = controller.HttpContext.Request.Path
            };

            problemDetails.Extensions["errorCode"] = "Errors.Unexpected";
            problemDetails.Extensions["traceId"] = controller.HttpContext.TraceIdentifier;

            return controller.StatusCode(
                StatusCodes.Status500InternalServerError,
                problemDetails);
        }
    }
}
