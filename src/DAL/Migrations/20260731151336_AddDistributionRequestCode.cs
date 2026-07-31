using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDistributionRequestCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestCode",
                table: "DistributionRequests",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [DistributionRequests]
                SET [RequestCode] = 'DIST-' + UPPER(LEFT(CONVERT(varchar(36), [Id]), 8))
                WHERE [RequestCode] IS NULL OR [RequestCode] = '';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "RequestCode",
                table: "DistributionRequests",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DistributionRequests_RequestCode",
                table: "DistributionRequests",
                column: "RequestCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DistributionRequests_RequestCode",
                table: "DistributionRequests");

            migrationBuilder.DropColumn(
                name: "RequestCode",
                table: "DistributionRequests");
        }
    }
}
