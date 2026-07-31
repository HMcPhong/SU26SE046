using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDistributionApprovalAuditRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DistributionRequests_ApprovedByManagerId",
                table: "DistributionRequests",
                column: "ApprovedByManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionRequests_WarehouseIssuedByStaffId",
                table: "DistributionRequests",
                column: "WarehouseIssuedByStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_DistributionRequests_Users_ApprovedByManagerId",
                table: "DistributionRequests",
                column: "ApprovedByManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DistributionRequests_Users_WarehouseIssuedByStaffId",
                table: "DistributionRequests",
                column: "WarehouseIssuedByStaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DistributionRequests_Users_ApprovedByManagerId",
                table: "DistributionRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_DistributionRequests_Users_WarehouseIssuedByStaffId",
                table: "DistributionRequests");

            migrationBuilder.DropIndex(
                name: "IX_DistributionRequests_ApprovedByManagerId",
                table: "DistributionRequests");

            migrationBuilder.DropIndex(
                name: "IX_DistributionRequests_WarehouseIssuedByStaffId",
                table: "DistributionRequests");
        }
    }
}
