using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TrainingTracker.Api.Database;
using TrainingTracker.Common.Results;
using TrainingTracker.Models.Entities;


namespace TrainingTracker.Features.Auth.Register;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly IMapper _mapper;
    public RegisterCommandHandler(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        ILogger<RegisterCommandHandler> logger,
        IMapper mapper)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        var emailAlreadyExists = await _dbContext.Users
            .AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (emailAlreadyExists)
        {
            _logger.LogWarning(
                "Registration failed. Email already exists: {Email}",
                normalizedEmail);

            return CreateEmailAlreadyExistsResult();
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        try
        {
            _dbContext.Users.Add(user);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            _logger.LogWarning(
                exception,
                "Registration failed because of unique email constraint. Email: {Email}", normalizedEmail);

            return CreateEmailAlreadyExistsResult();
        }

        _logger.LogInformation(
            "User registered successfully. UserId: {UserId}", user.Id);

        var response = _mapper.Map<RegisterResponse>(user);

        return Result<RegisterResponse>.Success(response);
    }

    private static Result<RegisterResponse> CreateEmailAlreadyExistsResult()
    {
        return Result<RegisterResponse>.Failure(new Error(
            Code: "Users.EmailAlreadyExists",
            Message: "A user with this email already exists.",
            Type: ErrorType.Conflict));
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException && postgresException.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}
