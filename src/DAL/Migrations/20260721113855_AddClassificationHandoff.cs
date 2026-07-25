using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddClassificationHandoff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClassificationReceivedAt",
                table: "IntakeBatches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClassificationReceivedByStaffId",
                table: "IntakeBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentToClassificationAt",
                table: "IntakeBatches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntakeBatches_ClassificationReceivedByStaffId",
                table: "IntakeBatches",
                column: "ClassificationReceivedByStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeBatches_Users_ClassificationReceivedByStaffId",
                table: "IntakeBatches",
                column: "ClassificationReceivedByStaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntakeBatches_Users_ClassificationReceivedByStaffId",
                table: "IntakeBatches");

            migrationBuilder.DropIndex(
                name: "IX_IntakeBatches_ClassificationReceivedByStaffId",
                table: "IntakeBatches");

            migrationBuilder.DropColumn(
                name: "ClassificationReceivedAt",
                table: "IntakeBatches");

            migrationBuilder.DropColumn(
                name: "ClassificationReceivedByStaffId",
                table: "IntakeBatches");

            migrationBuilder.DropColumn(
                name: "SentToClassificationAt",
                table: "IntakeBatches");
        }
    }
}
