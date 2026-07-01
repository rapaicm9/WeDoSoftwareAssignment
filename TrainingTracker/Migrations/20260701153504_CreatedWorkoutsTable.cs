using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingTracker.Migrations
{
    /// <inheritdoc />
    public partial class CreatedWorkoutsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Workouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    CaloriesBurned = table.Column<int>(type: "integer", nullable: false),
                    TrainingIntensity = table.Column<int>(type: "integer", nullable: false),
                    Fatigue = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    TrainingDateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workouts", x => x.Id);
                    table.CheckConstraint("CK_Workouts_CaloriesBurned_NonNegative", "\"CaloriesBurned\" >= 0");
                    table.CheckConstraint("CK_Workouts_DurationMinutes_Positive", "\"DurationMinutes\" > 0");
                    table.CheckConstraint("CK_Workouts_Fatigue_Range", "\"Fatigue\" BETWEEN 1 AND 10");
                    table.CheckConstraint("CK_Workouts_TrainingIntensity_Range", "\"TrainingIntensity\" BETWEEN 1 AND 10");
                    table.ForeignKey(
                        name: "FK_Workouts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_UserId_TrainingDateTimeUtc",
                table: "Workouts",
                columns: new[] { "UserId", "TrainingDateTimeUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Workouts");
        }
    }
}
