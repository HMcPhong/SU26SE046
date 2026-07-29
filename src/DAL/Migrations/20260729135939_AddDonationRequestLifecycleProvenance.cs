using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDonationRequestLifecycleProvenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestCode",
                table: "DonationRequests",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ClassifiedBatchDonationRequests",
                columns: table => new
                {
                    ClassifiedBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonationRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IntakeBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_ClassifiedBatchDonationRequests", x => new { x.ClassifiedBatchId, x.DonationRequestId, x.IntakeBatchId });
                    table.ForeignKey(
                        name: "FK_ClassifiedBatchDonationRequests_ClassifiedBatches_ClassifiedBatchId",
                        column: x => x.ClassifiedBatchId,
                        principalTable: "ClassifiedBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassifiedBatchDonationRequests_DonationRequests_DonationRequestId",
                        column: x => x.DonationRequestId,
                        principalTable: "DonationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassifiedBatchDonationRequests_IntakeBatches_IntakeBatchId",
                        column: x => x.IntakeBatchId,
                        principalTable: "IntakeBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(
                """
                UPDATE DonationRequests
                SET RequestCode = CONCAT(
                    'DR-',
                    COALESCE(YEAR(CreateAt), YEAR(GETUTCDATE())),
                    '-',
                    UPPER(LEFT(REPLACE(CONVERT(varchar(36), Id), '-', ''), 8))
                )
                WHERE RequestCode = '';
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO ClassifiedBatchDonationRequests
                    (ClassifiedBatchId, DonationRequestId, IntakeBatchId, LinkedAt,
                     Id, CreateAt, IsActive)
                SELECT
                    provenance.ClassifiedBatchId,
                    provenance.DonationRequestId,
                    provenance.IntakeBatchId,
                    GETUTCDATE(),
                    NEWID(),
                    GETUTCDATE(),
                    CAST(1 AS bit)
                FROM (
                    SELECT
                        item.ClassifiedBatchId,
                        source.DonationRequestId,
                        item.BatchId AS IntakeBatchId
                    FROM ClassifiedItems item
                    INNER JOIN IntakeBatchDonationRequests source
                        ON source.IntakeBatchId = item.BatchId
                    WHERE item.ClassifiedBatchId IS NOT NULL
                        AND (item.IsActive = 1 OR item.IsActive IS NULL)
                        AND (source.IsActive = 1 OR source.IsActive IS NULL)
                    GROUP BY item.ClassifiedBatchId, source.DonationRequestId, item.BatchId
                ) provenance;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_DonationRequests_RequestCode",
                table: "DonationRequests",
                column: "RequestCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassifiedBatchDonationRequests_DonationRequestId",
                table: "ClassifiedBatchDonationRequests",
                column: "DonationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassifiedBatchDonationRequests_IntakeBatchId",
                table: "ClassifiedBatchDonationRequests",
                column: "IntakeBatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassifiedBatchDonationRequests");

            migrationBuilder.DropIndex(
                name: "IX_DonationRequests_RequestCode",
                table: "DonationRequests");

            migrationBuilder.DropColumn(
                name: "RequestCode",
                table: "DonationRequests");
        }
    }
}
