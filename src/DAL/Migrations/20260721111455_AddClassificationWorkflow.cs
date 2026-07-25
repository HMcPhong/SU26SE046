using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddClassificationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InspectionAnswers_ClassifiedItemId",
                table: "InspectionAnswers");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProfileId",
                table: "ClassifiedItems",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClassifiedAt",
                table: "ClassifiedItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ClassifiedByStaffId",
                table: "ClassifiedItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ClothingType",
                table: "ClassifiedItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FabricType",
                table: "ClassifiedItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GarmentGroup",
                table: "ClassifiedItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "ClassifiedItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ItemCode",
                table: "ClassifiedItems",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ClassifiedItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessingDirection",
                table: "ClassifiedItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "ClassifiedItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ClassifiedItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetUser",
                table: "ClassifiedItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionAnswers_ClassifiedItemId_ConditionQuestionId",
                table: "InspectionAnswers",
                columns: new[] { "ClassifiedItemId", "ConditionQuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassifiedItems_ClassifiedByStaffId",
                table: "ClassifiedItems",
                column: "ClassifiedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassifiedItems_ItemCode",
                table: "ClassifiedItems",
                column: "ItemCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassifiedItems_Users_ClassifiedByStaffId",
                table: "ClassifiedItems",
                column: "ClassifiedByStaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = '86666666-6666-6666-6666-666666666666')
                INSERT INTO Users (Id, FullName, UserName, Email, PasswordHash, PhoneNumber, Address, RoleId, UserStatus, CreateAt, IsActive)
                VALUES ('86666666-6666-6666-6666-666666666666', N'Classification Staff Demo', 'classification.staff',
                    'classification.staff@greenthread.local', '$2a$11$TCC0aSnsg3xBXrySfOn18OsY5Bme6jTvPnd6kVhAfR/XJIFODASVa',
                    '0900000003', N'Ho Chi Minh City', '66666666-6666-6666-6666-666666666666', 'Active', SYSUTCDATETIME(), 1);

                INSERT INTO ConditionQuestions (Id, QuestionText, DisplayOrder, Weight, IsCritical, CreateAt, IsActive)
                SELECT v.Id, v.Text, v.Ord, 1, CASE WHEN v.Ord = 4 THEN 1 ELSE 0 END, SYSUTCDATETIME(), 1
                FROM (VALUES
                    (CAST('91000000-0000-0000-0000-000000000001' AS uniqueidentifier), N'Tình trạng vải', 1),
                    (CAST('91000000-0000-0000-0000-000000000002' AS uniqueidentifier), N'Bề mặt / Màu sắc', 2),
                    (CAST('91000000-0000-0000-0000-000000000003' AS uniqueidentifier), N'Phụ kiện (Khóa, cúc)', 3),
                    (CAST('91000000-0000-0000-0000-000000000004' AS uniqueidentifier), N'Vệ sinh / Mùi', 4)
                ) v(Id, Text, Ord)
                WHERE NOT EXISTS (SELECT 1 FROM ConditionQuestions q WHERE q.Id = v.Id);

                INSERT INTO ConditionAnswers (Id, ConditionQuestionId, AnswerText, ConditionRating, CreateAt, IsActive)
                SELECT v.Id, v.QuestionId, v.Text, v.Rating, SYSUTCDATETIME(), 1
                FROM (VALUES
                    (CAST('92000000-0000-0000-0000-000000000011' AS uniqueidentifier), CAST('91000000-0000-0000-0000-000000000001' AS uniqueidentifier), N'Nguyên vẹn, giữ đúng form dáng.', 1),
                    (CAST('92000000-0000-0000-0000-000000000012' AS uniqueidentifier), CAST('91000000-0000-0000-0000-000000000001' AS uniqueidentifier), N'Sờn xước nhẹ, xù lông, rách nhỏ có thể vá.', 2),
                    (CAST('92000000-0000-0000-0000-000000000013' AS uniqueidentifier), CAST('91000000-0000-0000-0000-000000000001' AS uniqueidentifier), N'Rách mảng lớn, mục nát, tơi sợi.', 3),
                    (CAST('92000000-0000-0000-0000-000000000021' AS uniqueidentifier), CAST('91000000-0000-0000-0000-000000000002' AS uniqueidentifier), N'Sạch sẽ, màu sắc tươi mới.', 1),
                    (CAST('92000000-0000-0000-0000-000000000022' AS uniqueidentifier), CAST('91000000-0000-0000-0000-000000000002' AS uniqueidentifier), N'Phai màu nhẹ, có vết ố nhỏ có thể giặt.', 2),
                    (CAST('92000000-0000-0000-0000-000000000023' AS uniqueidentifier), CAST('91000000-0000-0000-0000-000000000002' AS uniqueidentifier), N'Ố vàng nặng, loang lổ hóa chất.', 3),
                    (CAST('92000000-0000-0000-0000-000000000031' AS uniqueidentifier), CAST('91000000-0000-0000-0000-000000000003' AS uniqueidentifier), N'Đầy đủ, hoạt động trơn tru.', 1),
                    (CAST('92000000-0000-0000-0000-000000000032' AS uniqueidentifier), CAST('91000000-0000-0000-0000-000000000003' AS uniqueidentifier), N'Thiếu cúc, kẹt khóa kéo nhưng thay được.', 2),
                    (CAST('92000000-0000-0000-0000-000000000033' AS uniqueidentifier), CAST('91000000-0000-0000-0000-000000000003' AS uniqueidentifier), N'Hỏng hoàn toàn, rách nát phần cổ/tay áo.', 3),
                    (CAST('92000000-0000-0000-0000-000000000041' AS uniqueidentifier), CAST('91000000-0000-0000-0000-000000000004' AS uniqueidentifier), N'Thơm tho, không có mùi lạ.', 1),
                    (CAST('92000000-0000-0000-0000-000000000042' AS uniqueidentifier), CAST('91000000-0000-0000-0000-000000000004' AS uniqueidentifier), N'Hơi bụi bặm do để lâu ngày.', 2),
                    (CAST('92000000-0000-0000-0000-000000000043' AS uniqueidentifier), CAST('91000000-0000-0000-0000-000000000004' AS uniqueidentifier), N'Ẩm mốc nặng, có mùi hôi thối / độc hại.', 3)
                ) v(Id, QuestionId, Text, Rating)
                WHERE NOT EXISTS (SELECT 1 FROM ConditionAnswers a WHERE a.Id = v.Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM ConditionAnswers WHERE CONVERT(varchar(36), Id) LIKE '92000000-0000-0000-0000-%';
                DELETE FROM ConditionQuestions WHERE CONVERT(varchar(36), Id) LIKE '91000000-0000-0000-0000-%';
                DELETE FROM Users WHERE Id = '86666666-6666-6666-6666-666666666666';
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_ClassifiedItems_Users_ClassifiedByStaffId",
                table: "ClassifiedItems");

            migrationBuilder.DropIndex(
                name: "IX_InspectionAnswers_ClassifiedItemId_ConditionQuestionId",
                table: "InspectionAnswers");

            migrationBuilder.DropIndex(
                name: "IX_ClassifiedItems_ClassifiedByStaffId",
                table: "ClassifiedItems");

            migrationBuilder.DropIndex(
                name: "IX_ClassifiedItems_ItemCode",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "ClassifiedAt",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "ClassifiedByStaffId",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "ClothingType",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "FabricType",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "GarmentGroup",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "ItemCode",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "ProcessingDirection",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "TargetUser",
                table: "ClassifiedItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProfileId",
                table: "ClassifiedItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionAnswers_ClassifiedItemId",
                table: "InspectionAnswers",
                column: "ClassifiedItemId");
        }
    }
}
