using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeCategorySortOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ;WITH OrderedCategories AS
                (
                    SELECT Id,
                        ROW_NUMBER() OVER
                        (PARTITION BY [Type] ORDER BY SortOrder, CreateAt, Id) AS NewSortOrder
                    FROM Categories
                    WHERE IsActive = 1
                )
                UPDATE category
                SET SortOrder = ordered.NewSortOrder
                FROM Categories AS category
                INNER JOIN OrderedCategories AS ordered ON ordered.Id = category.Id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Categories
                SET SortOrder = SortOrder * 10
                WHERE IsActive = 1;
                """);
        }
    }
}
