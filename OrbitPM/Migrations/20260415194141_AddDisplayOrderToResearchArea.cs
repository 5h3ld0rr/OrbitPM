using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrbitPM.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayOrderToResearchArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "ResearchAreas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "ResearchAreas");
        }
    }
}
