using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultManagerAndConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @Now datetime2 = '2025-01-01T00:00:00Z';
                DECLARE @WarehouseId uniqueidentifier = 'B17468FF-CBE1-46A0-8375-890B50CD2F99';

                IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE Id = @WarehouseId)
                INSERT INTO Warehouses
                    (Id, WarehouseName, Address, PhoneNumber, Email, Description,
                     TotalCapacityKg, CurrentWeight, CreateAt, IsActive)
                VALUES
                    (@WarehouseId, N'Kho Thủ Đức - Võ Văn Ngân',
                     N'1 Võ Văn Ngân, Phường Thủ Đức, Thành phố Thủ Đức, TP. Hồ Chí Minh',
                     '0900000010', 'thuduc.warehouse@rethreads.local',
                     N'Kho tiếp nhận và phân phối chính tại Thành phố Thủ Đức',
                     15000, 0, @Now, 1);

                IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = '84444444-4444-4444-4444-444444444444')
                   AND NOT EXISTS (SELECT 1 FROM Users WHERE UserName = 'manager.demo')
                INSERT INTO Users
                    (Id, FullName, UserName, Email, PhoneNumber, Address, PasswordHash,
                     RoleId, UserStatus, EmailConfirmed, CreateAt, IsActive)
                VALUES
                    ('84444444-4444-4444-4444-444444444444', N'Manager Demo', 'manager.demo',
                     'manager.demo@rethreads.local', '0900000000', N'Ho Chi Minh City',
                     '$2a$11$cBjjgdFX6yIzoSj7KpCIReKi8UwMvd8BNSrlSFhHBdBZWd9o2ZJcy',
                     '44444444-4444-4444-4444-444444444444', 'Active', 1, @Now, 1);

                UPDATE Users
                SET PasswordHash = '$2a$11$cBjjgdFX6yIzoSj7KpCIReKi8UwMvd8BNSrlSFhHBdBZWd9o2ZJcy',
                    RoleId = '44444444-4444-4444-4444-444444444444',
                    UserStatus = 'Active', EmailConfirmed = 1, IsActive = 1,
                    DeleteAt = NULL, DeletedBy = NULL
                WHERE UserName = 'manager.demo';

                INSERT INTO WarehouseAreas
                    (Id, WarehouseId, AreaName, Description, CapacityKg, CurrentKg, CreateAt, IsActive)
                SELECT v.Id, @WarehouseId, v.Name, v.Description, 5000, 0, @Now, 1
                FROM (VALUES
                    (CAST('A1000000-0000-0000-0000-000000000001' AS uniqueidentifier), N'Khu hàng từ thiện', N'Khu lưu trữ hàng nhãn A theo hướng xử lý Charity'),
                    (CAST('A1000000-0000-0000-0000-000000000002' AS uniqueidentifier), N'Khu hàng tái chế', N'Khu lưu trữ hàng nhãn B theo hướng xử lý Recycling'),
                    (CAST('A1000000-0000-0000-0000-000000000003' AS uniqueidentifier), N'Khu cách ly/tiêu hủy', N'Khu lưu trữ hàng nhãn C theo hướng xử lý Disposal')
                ) v(Id, Name, Description)
                WHERE NOT EXISTS (SELECT 1 FROM WarehouseAreas a WHERE a.Id = v.Id);

                INSERT INTO AreaGroups
                    (Id, AreaId, GroupName, Description, CapacityKg, CurrentKg, CreateAt, IsActive)
                SELECT v.Id, v.AreaId, v.Name, v.Description, 5000, 0, @Now, 1
                FROM (VALUES
                    (CAST('B1000000-0000-0000-0000-000000000001' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000001' AS uniqueidentifier), N'Dãy CHARITY-A', N'Dãy lưu trữ hàng từ thiện'),
                    (CAST('B1000000-0000-0000-0000-000000000002' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000002' AS uniqueidentifier), N'Dãy RECYCLE-A', N'Dãy lưu trữ hàng tái chế'),
                    (CAST('B1000000-0000-0000-0000-000000000003' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000003' AS uniqueidentifier), N'Dãy DISPOSAL-A', N'Dãy lưu trữ hàng cách ly/tiêu hủy')
                ) v(Id, AreaId, Name, Description)
                WHERE NOT EXISTS (SELECT 1 FROM AreaGroups g WHERE g.Id = v.Id);

                INSERT INTO StorageLocations
                    (Id, WarehouseId, AreaId, AreaGroupId, LocationCode, AisleCode, RackCode,
                     ShelfCode, BinCode, PreferredProcessingDirection, CapacityKg,
                     CurrentWeightKg, Status, CreateAt, IsActive)
                SELECT v.Id, @WarehouseId, v.AreaId, v.GroupId, v.LocationCode, 'A01', v.RackCode,
                       v.ShelfCode, 'B01', v.Direction, 300, 0, 'Available', @Now, 1
                FROM (VALUES
                    (CAST('C1000000-0000-0000-0000-000000000001' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000001' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000001' AS uniqueidentifier), 'CHARITY-A01-R01-S01-B01', 'R01', 'S01', 'Charity'),
                    (CAST('C1000000-0000-0000-0000-000000000002' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000001' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000001' AS uniqueidentifier), 'CHARITY-A01-R01-S02-B01', 'R01', 'S02', 'Charity'),
                    (CAST('C1000000-0000-0000-0000-000000000003' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000001' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000001' AS uniqueidentifier), 'CHARITY-A01-R01-S03-B01', 'R01', 'S03', 'Charity'),
                    (CAST('C1000000-0000-0000-0000-000000000004' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000001' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000001' AS uniqueidentifier), 'CHARITY-A01-R02-S01-B01', 'R02', 'S01', 'Charity'),
                    (CAST('C1000000-0000-0000-0000-000000000005' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000001' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000001' AS uniqueidentifier), 'CHARITY-A01-R02-S02-B01', 'R02', 'S02', 'Charity'),
                    (CAST('C1000000-0000-0000-0000-000000000006' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000001' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000001' AS uniqueidentifier), 'CHARITY-A01-R02-S03-B01', 'R02', 'S03', 'Charity'),
                    (CAST('C1000000-0000-0000-0000-000000000007' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000002' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000002' AS uniqueidentifier), 'RECYCLE-A01-R01-S01-B01', 'R01', 'S01', 'Recycling'),
                    (CAST('C1000000-0000-0000-0000-000000000008' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000002' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000002' AS uniqueidentifier), 'RECYCLE-A01-R01-S02-B01', 'R01', 'S02', 'Recycling'),
                    (CAST('C1000000-0000-0000-0000-000000000009' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000002' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000002' AS uniqueidentifier), 'RECYCLE-A01-R01-S03-B01', 'R01', 'S03', 'Recycling'),
                    (CAST('C1000000-0000-0000-0000-000000000010' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000002' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000002' AS uniqueidentifier), 'RECYCLE-A01-R02-S01-B01', 'R02', 'S01', 'Recycling'),
                    (CAST('C1000000-0000-0000-0000-000000000011' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000002' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000002' AS uniqueidentifier), 'RECYCLE-A01-R02-S02-B01', 'R02', 'S02', 'Recycling'),
                    (CAST('C1000000-0000-0000-0000-000000000012' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000002' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000002' AS uniqueidentifier), 'RECYCLE-A01-R02-S03-B01', 'R02', 'S03', 'Recycling'),
                    (CAST('C1000000-0000-0000-0000-000000000013' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000003' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000003' AS uniqueidentifier), 'DISPOSAL-A01-R01-S01-B01', 'R01', 'S01', 'Disposal'),
                    (CAST('C1000000-0000-0000-0000-000000000014' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000003' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000003' AS uniqueidentifier), 'DISPOSAL-A01-R01-S02-B01', 'R01', 'S02', 'Disposal'),
                    (CAST('C1000000-0000-0000-0000-000000000015' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000003' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000003' AS uniqueidentifier), 'DISPOSAL-A01-R01-S03-B01', 'R01', 'S03', 'Disposal'),
                    (CAST('C1000000-0000-0000-0000-000000000016' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000003' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000003' AS uniqueidentifier), 'DISPOSAL-A01-R02-S01-B01', 'R02', 'S01', 'Disposal'),
                    (CAST('C1000000-0000-0000-0000-000000000017' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000003' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000003' AS uniqueidentifier), 'DISPOSAL-A01-R02-S02-B01', 'R02', 'S02', 'Disposal'),
                    (CAST('C1000000-0000-0000-0000-000000000018' AS uniqueidentifier), CAST('A1000000-0000-0000-0000-000000000003' AS uniqueidentifier), CAST('B1000000-0000-0000-0000-000000000003' AS uniqueidentifier), 'DISPOSAL-A01-R02-S03-B01', 'R02', 'S03', 'Disposal')
                ) v(Id, AreaId, GroupId, LocationCode, RackCode, ShelfCode, Direction)
                WHERE NOT EXISTS (SELECT 1 FROM StorageLocations l WHERE l.Id = v.Id)
                  AND NOT EXISTS (
                      SELECT 1 FROM StorageLocations l
                      WHERE l.WarehouseId = @WarehouseId AND l.LocationCode = v.LocationCode
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM StorageLocations
                WHERE Id IN (
                    'C1000000-0000-0000-0000-000000000001','C1000000-0000-0000-0000-000000000002',
                    'C1000000-0000-0000-0000-000000000003','C1000000-0000-0000-0000-000000000004',
                    'C1000000-0000-0000-0000-000000000005','C1000000-0000-0000-0000-000000000006',
                    'C1000000-0000-0000-0000-000000000007','C1000000-0000-0000-0000-000000000008',
                    'C1000000-0000-0000-0000-000000000009','C1000000-0000-0000-0000-000000000010',
                    'C1000000-0000-0000-0000-000000000011','C1000000-0000-0000-0000-000000000012',
                    'C1000000-0000-0000-0000-000000000013','C1000000-0000-0000-0000-000000000014',
                    'C1000000-0000-0000-0000-000000000015','C1000000-0000-0000-0000-000000000016',
                    'C1000000-0000-0000-0000-000000000017','C1000000-0000-0000-0000-000000000018');
                DELETE FROM AreaGroups WHERE Id IN (
                    'B1000000-0000-0000-0000-000000000001',
                    'B1000000-0000-0000-0000-000000000002',
                    'B1000000-0000-0000-0000-000000000003');
                DELETE FROM WarehouseAreas WHERE Id IN (
                    'A1000000-0000-0000-0000-000000000001',
                    'A1000000-0000-0000-0000-000000000002',
                    'A1000000-0000-0000-0000-000000000003');
                DELETE FROM Users WHERE Id = '84444444-4444-4444-4444-444444444444';
                """);
        }
    }
}
