using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarsityTrade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResidenceNameToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResidentName",
                table: "Locations",
                newName: "ResidenceName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResidenceName",
                table: "Locations",
                newName: "ResidentName");
        }
    }
}
