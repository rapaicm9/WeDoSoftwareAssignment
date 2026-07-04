using Microsoft.EntityFrameworkCore;
using TrainingTracker.Models.Entities;

namespace TrainingTracker.Database;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Workout> Workouts => Set<Workout>();

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

        modelBuilder.Entity<Workout>(entity =>
        {
            entity.ToTable("Workouts", table =>
            {
                table.HasCheckConstraint(
                    "CK_Workouts_DurationMinutes_Positive",
                    "\"DurationMinutes\" > 0");

                table.HasCheckConstraint(
                    "CK_Workouts_CaloriesBurned_NonNegative",
                    "\"CaloriesBurned\" >= 0");

                table.HasCheckConstraint(
                    "CK_Workouts_TrainingIntensity_Range",
                    "\"TrainingIntensity\" BETWEEN 1 AND 10");

                table.HasCheckConstraint(
                    "CK_Workouts_Fatigue_Range",
                    "\"Fatigue\" BETWEEN 1 AND 10");
            });

            entity.HasKey(workout => workout.Id);

            entity.Property(workout => workout.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(workout => workout.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(workout => workout.DurationMinutes)
                .IsRequired();

            entity.Property(workout => workout.CaloriesBurned)
                .IsRequired();

            entity.Property(workout => workout.TrainingIntensity)
                .IsRequired();

            entity.Property(workout => workout.Fatigue)
                .IsRequired();

            entity.Property(workout => workout.Notes)
                .HasColumnType("text");

            entity.Property(workout => workout.TrainingDateTimeUtc)
                .IsRequired();

            entity.Property(workout => workout.CreatedAtUtc)
                .IsRequired();

            entity.Property(workout => workout.UpdatedAtUtc);

            entity.HasOne(workout => workout.User)
                .WithMany(user => user.Workouts)
                .HasForeignKey(workout => workout.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(workout => new
            {
                workout.UserId,
                workout.TrainingDateTimeUtc
            });
        });
    }
}
