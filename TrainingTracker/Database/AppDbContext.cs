using Microsoft.EntityFrameworkCore;
using TrainingTracker.Models.Entities;

namespace TrainingTracker.Api.Database;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(user => user.Id);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(user => user.NormalizedEmail)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasIndex(user => user.NormalizedEmail)
                .IsUnique();

            entity.Property(user => user.PasswordHash)
                .IsRequired();

            entity.Property(user => user.FirstName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(user => user.LastName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(user => user.IsActive)
                .IsRequired();

            entity.Property(user => user.CreatedAtUtc)
                .IsRequired();

            entity.Property(user => user.UpdatedAtUtc);
        });
    }
}
