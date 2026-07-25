using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurableClassificationCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClothingTypeId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConditionGradeId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FabricTypeId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GarmentGroupId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GenderId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SizeId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetUserId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClothingTypeId",
                table: "ClassifiedItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConditionGradeId",
                table: "ClassifiedItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FabricTypeId",
                table: "ClassifiedItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GarmentGroupId",
                table: "ClassifiedItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GenderId",
                table: "ClassifiedItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SizeId",
                table: "ClassifiedItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetUserId",
                table: "ClassifiedItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClothingTypeId",
                table: "ClassifiedBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConditionGradeId",
                table: "ClassifiedBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FabricTypeId",
                table: "ClassifiedBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GarmentGroupId",
                table: "ClassifiedBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GenderId",
                table: "ClassifiedBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SizeId",
                table: "ClassifiedBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetUserId",
                table: "ClassifiedBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Categories",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Categories",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE Categories
                SET Code = CONCAT('LEGACY_', REPLACE(CONVERT(varchar(36), Id), '-', '')),
                    Type = 'Legacy'
                WHERE Code = '';

                DECLARE @Now datetime2 = SYSUTCDATETIME();
                DECLARE @Top uniqueidentifier = 'ca000001-0000-0000-0000-000000000001';
                DECLARE @Bottom uniqueidentifier = 'ca000001-0000-0000-0000-000000000002';

                INSERT INTO Categories (Id, Code, Name, Type, ParentId, SortOrder, Description, CreateAt, IsActive)
                SELECT v.Id, v.Code, v.Name, v.Type, v.ParentId, v.SortOrder, v.Description, @Now, 1
                FROM (VALUES
                  ('ca000001-0000-0000-0000-000000000001','GARMENT_TOP',N'Áo','GarmentGroup',NULL,10,N'Nhóm áo'),
                  ('ca000001-0000-0000-0000-000000000002','GARMENT_BOTTOM',N'Quần','GarmentGroup',NULL,20,N'Nhóm quần và váy'),
                  ('ca000002-0000-0000-0000-000000000001','FABRIC_COTTON',N'Vải cotton','FabricType',NULL,10,N''),
                  ('ca000002-0000-0000-0000-000000000002','FABRIC_LINEN',N'Vải lanh','FabricType',NULL,20,N''),
                  ('ca000002-0000-0000-0000-000000000003','FABRIC_SILK',N'Vải lụa','FabricType',NULL,30,N''),
                  ('ca000002-0000-0000-0000-000000000004','FABRIC_WOOL',N'Vải len','FabricType',NULL,40,N''),
                  ('ca000002-0000-0000-0000-000000000005','FABRIC_NYLON',N'Vải nylon','FabricType',NULL,50,N''),
                  ('ca000002-0000-0000-0000-000000000006','FABRIC_PARACHUTE',N'Vải dù','FabricType',NULL,60,N''),
                  ('ca000002-0000-0000-0000-000000000007','FABRIC_LEATHER',N'Da','FabricType',NULL,70,N''),
                  ('ca000002-0000-0000-0000-000000000008','FABRIC_DENIM',N'Vải jean','FabricType',NULL,80,N''),
                  ('ca000003-0000-0000-0000-000000000001','CLOTHING_TSHIRT_SHORT',N'Áo phông tay ngắn','ClothingType',@Top,10,N''),
                  ('ca000003-0000-0000-0000-000000000002','CLOTHING_TSHIRT_LONG',N'Áo phông tay dài','ClothingType',@Top,20,N''),
                  ('ca000003-0000-0000-0000-000000000003','CLOTHING_TANK_TOP',N'Áo ba lỗ','ClothingType',@Top,30,N''),
                  ('ca000003-0000-0000-0000-000000000004','CLOTHING_SHIRT_SHORT',N'Áo sơ mi tay ngắn','ClothingType',@Top,40,N''),
                  ('ca000003-0000-0000-0000-000000000005','CLOTHING_SHIRT_LONG',N'Áo sơ mi tay dài','ClothingType',@Top,50,N''),
                  ('ca000003-0000-0000-0000-000000000006','CLOTHING_JACKET',N'Áo khoác','ClothingType',@Top,60,N''),
                  ('ca000003-0000-0000-0000-000000000007','CLOTHING_VEST',N'Áo vest','ClothingType',@Top,70,N''),
                  ('ca000003-0000-0000-0000-000000000008','CLOTHING_BLAZER',N'Áo blazer','ClothingType',@Top,80,N''),
                  ('ca000003-0000-0000-0000-000000000009','CLOTHING_SWEATER',N'Áo sweater','ClothingType',@Top,90,N''),
                  ('ca000003-0000-0000-0000-000000000010','CLOTHING_POLO',N'Áo polo','ClothingType',@Top,100,N''),
                  ('ca000003-0000-0000-0000-000000000011','CLOTHING_AO_DAI',N'Áo dài','ClothingType',@Top,110,N''),
                  ('ca000003-0000-0000-0000-000000000012','CLOTHING_TROUSERS',N'Quần tây','ClothingType',@Bottom,10,N''),
                  ('ca000003-0000-0000-0000-000000000013','CLOTHING_SHORTS',N'Quần ngắn','ClothingType',@Bottom,20,N''),
                  ('ca000003-0000-0000-0000-000000000014','CLOTHING_KHAKI',N'Quần kaki','ClothingType',@Bottom,30,N''),
                  ('ca000003-0000-0000-0000-000000000015','CLOTHING_PANTS',N'Quần dài','ClothingType',@Bottom,40,N''),
                  ('ca000003-0000-0000-0000-000000000016','CLOTHING_WIDE_LEG',N'Quần ống rộng','ClothingType',@Bottom,50,N''),
                  ('ca000003-0000-0000-0000-000000000017','CLOTHING_SKIRT',N'Váy','ClothingType',@Bottom,60,N''),
                  ('ca000004-0000-0000-0000-000000000001','GENDER_MALE',N'Nam','Gender',NULL,10,N''),
                  ('ca000004-0000-0000-0000-000000000002','GENDER_FEMALE',N'Nữ','Gender',NULL,20,N''),
                  ('ca000004-0000-0000-0000-000000000003','GENDER_UNISEX',N'Unisex','Gender',NULL,30,N''),
                  ('ca000005-0000-0000-0000-000000000001','TARGET_BABY',N'Em bé','TargetUser',NULL,10,N''),
                  ('ca000005-0000-0000-0000-000000000002','TARGET_CHILD',N'Trẻ em','TargetUser',NULL,20,N''),
                  ('ca000005-0000-0000-0000-000000000003','TARGET_ADULT',N'Người lớn','TargetUser',NULL,30,N''),
                  ('ca000006-0000-0000-0000-000000000001','SIZE_S',N'S','Size',NULL,10,N''),
                  ('ca000006-0000-0000-0000-000000000002','SIZE_M',N'M','Size',NULL,20,N''),
                  ('ca000006-0000-0000-0000-000000000003','SIZE_L',N'L','Size',NULL,30,N''),
                  ('ca000006-0000-0000-0000-000000000004','SIZE_XL',N'XL','Size',NULL,40,N''),
                  ('ca000006-0000-0000-0000-000000000005','SIZE_XXL',N'XXL','Size',NULL,50,N''),
                  ('ca000006-0000-0000-0000-000000000006','SIZE_XXXL',N'XXXL','Size',NULL,60,N''),
                  ('ca000006-0000-0000-0000-000000000007','SIZE_FREESIZE',N'Freesize','Size',NULL,70,N''),
                  ('ca000007-0000-0000-0000-000000000001','GRADE_A',N'Nhãn A','ConditionGrade',NULL,10,N'Tốt, ưu tiên từ thiện'),
                  ('ca000007-0000-0000-0000-000000000002','GRADE_B',N'Nhãn B','ConditionGrade',NULL,20,N'Tái chế'),
                  ('ca000007-0000-0000-0000-000000000003','GRADE_C',N'Nhãn C','ConditionGrade',NULL,30,N'Loại bỏ')
                ) v(Id,Code,Name,Type,ParentId,SortOrder,Description)
                WHERE NOT EXISTS (SELECT 1 FROM Categories c WHERE c.Code = v.Code);

                UPDATE i SET
                  FabricTypeId=f.Id, GarmentGroupId=g.Id, ClothingTypeId=ct.Id, GenderId=ge.Id,
                  TargetUserId=t.Id, SizeId=s.Id, ConditionGradeId=gr.Id
                FROM ClassifiedItems i
                LEFT JOIN Categories f ON f.Type='FabricType' AND f.Name=i.FabricType
                LEFT JOIN Categories g ON g.Type='GarmentGroup' AND g.Name=i.GarmentGroup
                LEFT JOIN Categories ct ON ct.Type='ClothingType' AND ct.Name=i.ClothingType
                LEFT JOIN Categories ge ON ge.Type='Gender' AND ge.Name=i.Gender
                LEFT JOIN Categories t ON t.Type='TargetUser' AND t.Name=i.TargetUser
                LEFT JOIN Categories s ON s.Type='Size' AND s.Name=i.Size
                LEFT JOIN Categories gr ON gr.Code=CONCAT('GRADE_',CASE i.ConditionRating WHEN 1 THEN 'A' WHEN 2 THEN 'B' ELSE 'C' END);

                UPDATE b SET
                  FabricTypeId=f.Id, GarmentGroupId=g.Id, ClothingTypeId=ct.Id, GenderId=ge.Id,
                  TargetUserId=t.Id, SizeId=s.Id, ConditionGradeId=gr.Id
                FROM ClassifiedBatches b
                LEFT JOIN Categories f ON f.Type='FabricType' AND f.Name=b.FabricType
                LEFT JOIN Categories g ON g.Type='GarmentGroup' AND g.Name=b.GarmentGroup
                LEFT JOIN Categories ct ON ct.Type='ClothingType' AND ct.Name=b.ClothingType
                LEFT JOIN Categories ge ON ge.Type='Gender' AND ge.Name=b.Gender
                LEFT JOIN Categories t ON t.Type='TargetUser' AND t.Name=b.TargetUser
                LEFT JOIN Categories s ON s.Type='Size' AND s.Name=b.Size
                LEFT JOIN Categories gr ON gr.Code=CONCAT('GRADE_',CASE b.ConditionRating WHEN 1 THEN 'A' WHEN 2 THEN 'B' ELSE 'C' END);

                UPDATE i SET
                  FabricTypeId=f.Id, GarmentGroupId=g.Id, ClothingTypeId=ct.Id, GenderId=ge.Id,
                  TargetUserId=t.Id, SizeId=s.Id, ConditionGradeId=gr.Id
                FROM Inventories i
                LEFT JOIN Categories f ON f.Type='FabricType' AND f.Name=i.FabricType
                LEFT JOIN Categories g ON g.Type='GarmentGroup' AND g.Name=i.GarmentGroup
                LEFT JOIN Categories ct ON ct.Type='ClothingType' AND ct.Name=i.ClothingType
                LEFT JOIN Categories ge ON ge.Type='Gender' AND ge.Name=i.Gender
                LEFT JOIN Categories t ON t.Type='TargetUser' AND t.Name=i.TargetUser
                LEFT JOIN Categories s ON s.Type='Size' AND s.Name=i.Size
                LEFT JOIN Categories gr ON gr.Code=CONCAT('GRADE_',CASE i.ConditionRating WHEN 1 THEN 'A' WHEN 2 THEN 'B' ELSE 'C' END);
                """
            );

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Code",
                table: "Categories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Type_ParentId_Name",
                table: "Categories",
                columns: new[] { "Type", "ParentId", "Name" },
                unique: true,
                filter: "[ParentId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_Code",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Type_ParentId_Name",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ClothingTypeId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ConditionGradeId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "FabricTypeId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "GarmentGroupId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "GenderId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "SizeId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "TargetUserId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ClothingTypeId",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "ConditionGradeId",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "FabricTypeId",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "GarmentGroupId",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "GenderId",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "SizeId",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "TargetUserId",
                table: "ClassifiedItems");

            migrationBuilder.DropColumn(
                name: "ClothingTypeId",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "ConditionGradeId",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "FabricTypeId",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "GarmentGroupId",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "GenderId",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "SizeId",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "TargetUserId",
                table: "ClassifiedBatches");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Categories");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
