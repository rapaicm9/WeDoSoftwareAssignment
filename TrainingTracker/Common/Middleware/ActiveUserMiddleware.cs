using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingTracker.Database;

namespace TrainingTracker.Common.Middleware
{
    public sealed class ActiveUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ActiveUserMiddleware> _logger;

        public ActiveUserMiddleware(
            RequestDelegate next,
            ILogger<ActiveUserMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            AppDbContext dbContext)
        {
            try
            {
                var isAuthenticated = context.User.Identity?.IsAuthenticated == true;

                if (!isAuthenticated)
                {
                    await _next(context);
                    return;
                }

                var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning(
                        "Authenticated request rejected because the user ID claim is missing or invalid.");

                    await WriteProblemDetailsAsync(
                        context,
                        StatusCodes.Status401Unauthorized,
                        title: "Unauthorized",
                        detail: "Authenticated user identifier is invalid.",
                        errorCode: "Auth.InvalidUserIdClaim");

                    return;
                }

                var userStatus = await dbContext.Users
                    .AsNoTracking()
                    .Where(user => user.Id == userId)
                    .Select(user => new
                    {
                        user.IsActive
                    })
                    .FirstOrDefaultAsync(context.RequestAborted);

                if (userStatus is null)
                {
                    _logger.LogWarning(
                        "Authenticated request rejected because User ID {UserId} was not found.",
                        userId);

                    await WriteProblemDetailsAsync(
                        context,
                        StatusCodes.Status401Unauthorized,
                        title: "Unauthorized",
                        detail: "Authenticated user was not found.",
                        errorCode: "Auth.UserNotFound");

                    return;
                }

                if (!userStatus.IsActive)
                {
                    _logger.LogWarning(
                        "Authenticated request rejected because User ID {UserId} is inactive.",
                        userId);

                    await WriteProblemDetailsAsync(
                        context,
                        StatusCodes.Status403Forbidden,
                        title: "Forbidden",
                        detail: "User is inactive.",
                        errorCode: "Users.UserInactive");

                    return;
                }

                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Active user validation was canceled by the client.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while validating authenticated user status.");

                await WriteProblemDetailsAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    title: "Request Failed",
                    detail: "An error occurred while validating the authenticated user.",
                    errorCode: "Auth.ActiveUserValidationFailed");
            }
        }

        private static async Task WriteProblemDetailsAsync(
            HttpContext context,
            int statusCode,
            string title,
            string detail,
            string errorCode)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["errorCode"] = errorCode;
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken: context.RequestAborted);
        }
    }
}
