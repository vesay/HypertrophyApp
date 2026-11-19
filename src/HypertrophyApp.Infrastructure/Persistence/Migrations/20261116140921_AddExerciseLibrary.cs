using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HypertrophyApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exercises",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Category = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Equipment = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Force = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Mechanic = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PrimaryMuscles = table.Column<string[]>(type: "text[]", nullable: false),
                    SecondaryMuscles = table.Column<string[]>(type: "text[]", nullable: false),
                    Instructions = table.Column<string[]>(type: "text[]", nullable: false),
                    ImagePaths = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercises", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exercises_Name",
                table: "exercises",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exercises");
        }
    }
}
