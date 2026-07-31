using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSmsVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserVerificationCodes_UserId_Channel_IsActive",
                table: "UserVerificationCodes");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "UserVerificationCodes");

            migrationBuilder.DropColumn(
                name: "PhoneNumberConfirmed",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_UserVerificationCodes_UserId_IsActive",
                table: "UserVerificationCodes",
                columns: new[] { "UserId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserVerificationCodes_UserId_IsActive",
                table: "UserVerificationCodes");

            migrationBuilder.AddColumn<string>(
                name: "Channel",
                table: "UserVerificationCodes",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("85555555-5555-5555-5555-555555555555"),
                column: "PhoneNumberConfirmed",
                value: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVerificationCodes_UserId_Channel_IsActive",
                table: "UserVerificationCodes",
                columns: new[] { "UserId", "Channel", "IsActive" });
        }
    }
}
