using TrainingTracker.Api.Database;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
