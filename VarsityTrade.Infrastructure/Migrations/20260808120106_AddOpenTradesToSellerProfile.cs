using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarsityTrade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenTradesToSellerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpenToTrade",
                table: "SellerProfiles",
                newName: "OpenToTrades");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpenToTrades",
                table: "SellerProfiles",
                newName: "OpenToTrade");
        }
    }
}
