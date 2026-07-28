using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class EnableMultipleTeamsPerShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IntakeBatches_ShiftId",
                table: "IntakeBatches");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeBatches_ShiftId_ReceivingTeamId",
                table: "IntakeBatches",
                columns: new[] { "ShiftId", "ReceivingTeamId" },
                unique: true,
                filter: "[ReceivingTeamId] IS NOT NULL AND [IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IntakeBatches_ShiftId_ReceivingTeamId",
                table: "IntakeBatches");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeBatches_ShiftId",
                table: "IntakeBatches",
                column: "ShiftId",
                unique: true);
        }
    }
}
