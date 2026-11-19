using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HypertrophyApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLogsAndCustomProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "profiles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "custom_program",
                table: "profiles",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "food_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Meal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Calories = table.Column<int>(type: "integer", nullable: false),
                    ProteinGrams = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: true),
                    CarbGrams = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: true),
                    FatGrams = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_food_entries_profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workout_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Title = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workout_sessions_profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workout_sets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExerciseName = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SetNumber = table.Column<int>(type: "integer", nullable: false),
                    Reps = table.Column<int>(type: "integer", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_sets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workout_sets_workout_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "workout_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_food_entries_ProfileId_Date",
                table: "food_entries",
                columns: new[] { "ProfileId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_ProfileId_Date",
                table: "workout_sessions",
                columns: new[] { "ProfileId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workout_sets_SessionId",
                table: "workout_sets",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "food_entries");

            migrationBuilder.DropTable(
                name: "workout_sets");

            migrationBuilder.DropTable(
                name: "workout_sessions");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "custom_program",
                table: "profiles");
        }
    }
}
