using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseWarehouseWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = '87777777-7777-7777-7777-777777777777')
                INSERT INTO Users (Id, FullName, UserName, Email, PasswordHash, PhoneNumber, Address, RoleId, WarehouseId, UserStatus, CreateAt, IsActive)
                VALUES ('87777777-7777-7777-7777-777777777777', N'Warehouse Staff Demo', 'warehouse.staff',
                    'warehouse.staff@greenthread.local', '$2a$11$slzALYN9LknwEmsEjYEiA.1.N/qK/4P6F.IYTwZm/xPVHFwZly.ne',
                    '0900000004', N'Ho Chi Minh City', '77777777-7777-7777-7777-777777777777',
                    'B17468FF-CBE1-46A0-8375-890B50CD2F99', 'Active', SYSUTCDATETIME(), 1);
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "DestinationLocationId",
                table: "TransactionItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantityAfter",
                table: "TransactionItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityBefore",
                table: "TransactionItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceLocationId",
                table: "TransactionItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightAfter",
                table: "TransactionItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightBefore",
                table: "TransactionItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PerformedAt",
                table: "InventoryTransactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "PerformedByStaffId",
                table: "InventoryTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "ProfileId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "ClassifiedBatchId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClothingType",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FabricType",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GarmentGroup",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProcessingDirection",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ReservedQuantity",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ReservedWeight",
                table: "Inventories",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "Inventories",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "StorageLocationId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetUser",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ReceivedItemCount",
                table: "ClassifiedBatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedWeight",
                table: "ClassifiedBatches",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentToWarehouseAt",
                table: "ClassifiedBatches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SentToWarehouseByStaffId",
                table: "ClassifiedBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StoredAt",
                table: "ClassifiedBatches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoredByStaffId",
                table: "ClassifiedBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarehouseReceiptNotes",
                table: "ClassifiedBatches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarehouseReceivedAt",
                table: "ClassifiedBatches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseReceivedByStaffId",
                table: "ClassifiedBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StorageLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AreaGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LocationCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AisleCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RackCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShelfCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BinCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferredGarmentGroup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredProcessingDirection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CapacityKg = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentWeightKg = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_StorageLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageLocations_AreaGroups_AreaGroupId",
                        column: x => x.AreaGroupId,
                        principalTable: "AreaGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StorageLocations_WarehouseAreas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "WarehouseAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StorageLocations_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_DestinationLocationId",
                table: "TransactionItems",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_SourceLocationId",
                table: "TransactionItems",
                column: "SourceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_PerformedByStaffId",
                table: "InventoryTransactions",
                column: "PerformedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ClassifiedBatchId",
                table: "Inventories",
                column: "ClassifiedBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_Sku",
                table: "Inventories",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_StorageLocationId",
                table: "Inventories",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassifiedBatches_SentToWarehouseByStaffId",
                table: "ClassifiedBatches",
                column: "SentToWarehouseByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassifiedBatches_StoredByStaffId",
                table: "ClassifiedBatches",
                column: "StoredByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassifiedBatches_WarehouseReceivedByStaffId",
                table: "ClassifiedBatches",
                column: "WarehouseReceivedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_AreaGroupId",
                table: "StorageLocations",
                column: "AreaGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_AreaId",
                table: "StorageLocations",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_WarehouseId_LocationCode",
                table: "StorageLocations",
                columns: new[] { "WarehouseId", "LocationCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassifiedBatches_Users_SentToWarehouseByStaffId",
                table: "ClassifiedBatches",
                column: "SentToWarehouseByStaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassifiedBatches_Users_StoredByStaffId",
                table: "ClassifiedBatches",
                column: "StoredByStaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassifiedBatches_Users_WarehouseReceivedByStaffId",
                table: "ClassifiedBatches",
                column: "WarehouseReceivedByStaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_ClassifiedBatches_ClassifiedBatchId",
                table: "Inventories",
                column: "ClassifiedBatchId",
                principalTable: "ClassifiedBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_StorageLocations_StorageLocationId",
                table: "Inventories",
                column: "StorageLocationId",
                principalTable: "StorageLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Users_PerformedByStaffId",
                table: "InventoryTransactions",
                column: "PerformedByStaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionItems_StorageLocations_DestinationLocationId",
                table: "TransactionItems",
                column: "DestinationLocationId",
                principalTable: "StorageLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionItems_StorageLocations_SourceLocationId",
                table: "TransactionItems",
                column: "SourceLocationId",
                principalTable: "StorageLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM Users
                WHERE Id = '87777777-7777-7777-7777-777777777777'
                  AND UserName = 'warehouse.staff';
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_ClassifiedBatches_Users_SentToWarehouseByStaffId",
                table: "ClassifiedBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassifiedBatches_Users_StoredByStaffId",
                table: "ClassifiedBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassifiedBatches_Users_WarehouseReceivedByStaffId",
                table: "ClassifiedBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_ClassifiedBatches_ClassifiedBatchId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_StorageLocations_StorageLocationId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Users_PerformedByStaffId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionItems_StorageLocations_DestinationLocationId",
                table: "TransactionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionItems_StorageLocations_SourceLocationId",
                table: "TransactionItems");

            migrationBuilder.DropTable(
                name: "StorageLocations");

            migrationBuilder.DropIndex(
                name: "IX_TransactionItems_DestinationLocationId",
                table: "TransactionItems");

            migrationBuilder.DropIndex(
                name: "IX_TransactionItems_SourceLocationId",
                table: "TransactionItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_PerformedByStaffId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_ClassifiedBatchId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_Sku",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_StorageLocationId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_ClassifiedBatches_SentToWarehouseByStaffId",
                table: "ClassifiedBatches");

            migrationBuilder.DropIndex(
                name: "IX_ClassifiedBatches_StoredByStaffId",
                table: "ClassifiedBatches");

            migrationBuilder.DropIndex(
                name: "IX_ClassifiedBatches_WarehouseReceivedByStaffId",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "DestinationLocationId",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "QuantityAfter",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "QuantityBefore",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "SourceLocationId",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "WeightAfter",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "WeightBefore",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "PerformedAt",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "PerformedByStaffId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "ClassifiedBatchId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ClothingType",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "FabricType",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "GarmentGroup",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ProcessingDirection",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ReservedQuantity",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ReservedWeight",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "StorageLocationId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "TargetUser",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ReceivedItemCount",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "ReceivedWeight",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "SentToWarehouseAt",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "SentToWarehouseByStaffId",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "StoredAt",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "StoredByStaffId",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "WarehouseReceiptNotes",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "WarehouseReceivedAt",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "WarehouseReceivedByStaffId",
                table: "ClassifiedBatches");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProfileId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
