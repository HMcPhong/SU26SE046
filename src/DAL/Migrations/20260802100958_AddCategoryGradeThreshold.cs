using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryGradeThreshold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinimumMatchCount",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Categories SET MinimumMatchCount = 2 WHERE Code = 'GRADE_B';
                UPDATE Categories SET MinimumMatchCount = 1 WHERE Code = 'GRADE_C';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumMatchCount",
                table: "Categories");
        }
    }
}
