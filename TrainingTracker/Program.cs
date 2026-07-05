using TrainingTracker.Database;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Identity;
using TrainingTracker.Models.Entities;
using FluentValidation;
using TrainingTracker.Common.Behaviors;
using TrainingTracker.Common.Exceptions;
using AutoMapper;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TrainingTracker.Features.Auth;
using System.Text.Json.Serialization;
using TrainingTracker.Common.Middleware;

const string angularFrontendCorsPolicy = "AngularFrontendCorsPolicy";


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
    configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddAutoMapper(
    configuration => { },
    typeof(Program).Assembly); 

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>();

if (jwtOptions is null)
{
    throw new InvalidOperationException("JWT configuration section is missing.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
{
    throw new InvalidOperationException("JWT issuer is missing.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
{
    throw new InvalidOperationException("JWT audience is missing.");
}

if (jwtOptions.AccessTokenExpirationMinutes <= 0)
{
    throw new InvalidOperationException("JWT access token expiration must be greater than zero.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Secret))
{
    throw new InvalidOperationException("JWT secret is missing.");
}

if (jwtOptions.Secret.Length < 32)
{
    throw new InvalidOperationException("JWT secret must be at least 32 characters long.");
}

builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(angularFrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Secret)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.Services.GetRequiredService<IMapper>()
    .ConfigurationProvider
    .AssertConfigurationIsValid();

app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();


app.UseHttpsRedirection();

app.UseCors(angularFrontendCorsPolicy);

app.UseAuthentication();

app.UseMiddleware<ActiveUserMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
