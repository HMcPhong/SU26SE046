using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDistributionWorkflowAndGhnTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByManagerId",
                table: "DistributionRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhnOrderCode",
                table: "DistributionRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhnStatus",
                table: "DistributionRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GhnUpdatedAt",
                table: "DistributionRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssueSlipCode",
                table: "DistributionRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "DistributionRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecipientPhone",
                table: "DistributionRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "WarehouseIssuedAt",
                table: "DistributionRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseIssuedByStaffId",
                table: "DistributionRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProfileId",
                table: "DistributionItems",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "ApprovedQuantity",
                table: "DistributionItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "InventoryId",
                table: "DistributionItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IssuedQuantity",
                table: "DistributionItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "IssuedWeight",
                table: "DistributionItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RequestedWeight",
                table: "DistributionItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ShipmentStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DistributionRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentStatusHistories_DistributionRequests_DistributionRequestId",
                        column: x => x.DistributionRequestId,
                        principalTable: "DistributionRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DistributionRequests_GhnOrderCode",
                table: "DistributionRequests",
                column: "GhnOrderCode",
                unique: true,
                filter: "[GhnOrderCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionRequests_IssueSlipCode",
                table: "DistributionRequests",
                column: "IssueSlipCode",
                unique: true,
                filter: "[IssueSlipCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionItems_InventoryId",
                table: "DistributionItems",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentStatusHistories_DistributionRequestId",
                table: "ShipmentStatusHistories",
                column: "DistributionRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_DistributionItems_Inventories_InventoryId",
                table: "DistributionItems",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DistributionItems_Inventories_InventoryId",
                table: "DistributionItems");

            migrationBuilder.DropTable(
                name: "ShipmentStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_DistributionRequests_GhnOrderCode",
                table: "DistributionRequests");

            migrationBuilder.DropIndex(
                name: "IX_DistributionRequests_IssueSlipCode",
                table: "DistributionRequests");

            migrationBuilder.DropIndex(
                name: "IX_DistributionItems_InventoryId",
                table: "DistributionItems");

            migrationBuilder.DropColumn(
                name: "ApprovedByManagerId",
                table: "DistributionRequests");

            migrationBuilder.DropColumn(
                name: "GhnOrderCode",
                table: "DistributionRequests");

            migrationBuilder.DropColumn(
                name: "GhnStatus",
                table: "DistributionRequests");

            migrationBuilder.DropColumn(
                name: "GhnUpdatedAt",
                table: "DistributionRequests");

            migrationBuilder.DropColumn(
                name: "IssueSlipCode",
                table: "DistributionRequests");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "DistributionRequests");

            migrationBuilder.DropColumn(
                name: "RecipientPhone",
                table: "DistributionRequests");

            migrationBuilder.DropColumn(
                name: "WarehouseIssuedAt",
                table: "DistributionRequests");

            migrationBuilder.DropColumn(
                name: "WarehouseIssuedByStaffId",
                table: "DistributionRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedQuantity",
                table: "DistributionItems");

            migrationBuilder.DropColumn(
                name: "InventoryId",
                table: "DistributionItems");

            migrationBuilder.DropColumn(
                name: "IssuedQuantity",
                table: "DistributionItems");

            migrationBuilder.DropColumn(
                name: "IssuedWeight",
                table: "DistributionItems");

            migrationBuilder.DropColumn(
                name: "RequestedWeight",
                table: "DistributionItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProfileId",
                table: "DistributionItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
