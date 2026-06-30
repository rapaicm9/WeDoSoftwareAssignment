using Microsoft.EntityFrameworkCore;

namespace TrainingTracker.Api.Database;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
