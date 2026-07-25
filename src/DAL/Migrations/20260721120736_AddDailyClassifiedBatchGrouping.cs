using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyClassifiedBatchGrouping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ProfileId",
                table: "ClassifiedBatches",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClassificationDate",
                table: "ClassifiedBatches",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ClothingType",
                table: "ClassifiedBatches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FabricType",
                table: "ClassifiedBatches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GarmentGroup",
                table: "ClassifiedBatches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "ClassifiedBatches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GroupKey",
                table: "ClassifiedBatches",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProcessingDirection",
                table: "ClassifiedBatches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "ClassifiedBatches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetUser",
                table: "ClassifiedBatches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ClassifiedBatches_GroupKey",
                table: "ClassifiedBatches",
                column: "GroupKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClassifiedBatches_GroupKey",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "ClassificationDate",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "ClothingType",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "FabricType",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "GarmentGroup",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "GroupKey",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "ProcessingDirection",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "TargetUser",
                table: "ClassifiedBatches");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProfileId",
                table: "ClassifiedBatches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
