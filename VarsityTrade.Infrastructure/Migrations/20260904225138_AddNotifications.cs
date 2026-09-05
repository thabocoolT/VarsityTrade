using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarsityTrade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifictions_AspNetUsers_UserId",
                table: "Notifictions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifictions",
                table: "Notifictions");

            migrationBuilder.RenameTable(
                name: "Notifictions",
                newName: "Notifications");

            migrationBuilder.RenameIndex(
                name: "IX_Notifictions_UserId",
                table: "Notifications",
                newName: "IX_Notifications_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "Notifictions");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId",
                table: "Notifictions",
                newName: "IX_Notifictions_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifictions",
                table: "Notifictions",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifictions_AspNetUsers_UserId",
                table: "Notifictions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
