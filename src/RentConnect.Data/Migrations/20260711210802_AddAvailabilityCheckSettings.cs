using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentConnect.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailabilityCheckSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AvailabilityCheckEnabled",
                table: "SiteSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AvailabilityCheckIntervalDays",
                table: "SiteSettings",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityCheckEnabled",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "AvailabilityCheckIntervalDays",
                table: "SiteSettings");
        }
    }
}
