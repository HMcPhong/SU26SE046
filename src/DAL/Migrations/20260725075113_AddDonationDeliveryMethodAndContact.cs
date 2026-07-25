using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDonationDeliveryMethodAndContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "DonationRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhoneNumber",
                table: "DonationRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryMethod",
                table: "DonationRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "StaffPickup");

            migrationBuilder.Sql("""
                UPDATE request
                SET request.ContactName = donor.FullName,
                    request.ContactPhoneNumber = donor.PhoneNumber,
                    request.DeliveryMethod = 'StaffPickup'
                FROM DonationRequests request
                INNER JOIN Users donor ON donor.Id = request.DonorId;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "DonationRequests");

            migrationBuilder.DropColumn(
                name: "ContactPhoneNumber",
                table: "DonationRequests");

            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                table: "DonationRequests");
        }
    }
}
