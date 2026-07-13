using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentConnect.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddListingContractDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RentalContractImageUrl",
                table: "Listings");

            migrationBuilder.CreateTable(
                name: "ListingContractDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ListingId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingContractDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingContractDocuments_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListingContractDocuments_ListingId",
                table: "ListingContractDocuments",
                column: "ListingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListingContractDocuments");

            migrationBuilder.AddColumn<string>(
                name: "RentalContractImageUrl",
                table: "Listings",
                type: "TEXT",
                nullable: true);
        }
    }
}
