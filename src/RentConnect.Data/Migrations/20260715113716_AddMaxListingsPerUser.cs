using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentConnect.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxListingsPerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxListingsPerUser",
                table: "SiteSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxListingsPerUser",
                table: "SiteSettings");
        }
    }
}
