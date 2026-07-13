using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentConnect.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyTypeAmenitiesContractAndOccupation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasBalcony",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasElevator",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasGarage",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasRentalContract",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasYard",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PropertyType",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresPromissoryNote",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SecurityDepositJod",
                table: "Listings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasBalcony",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "HasElevator",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "HasGarage",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "HasRentalContract",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "HasYard",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "PropertyType",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RequiresPromissoryNote",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "SecurityDepositJod",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "AspNetUsers");
        }
    }
}
