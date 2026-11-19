using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HypertrophyApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "profiles",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                // Existing rows must hold a valid enum name, otherwise reading them would fail.
                defaultValue: "Male");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sex",
                table: "profiles");
        }
    }
}
