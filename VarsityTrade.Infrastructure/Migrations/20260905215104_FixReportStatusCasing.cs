using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarsityTrade.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixReportStatusCasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLog_AspNetUsers_UserId",
                table: "AuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditLog",
                table: "AuditLog");

            migrationBuilder.RenameTable(
                name: "AuditLog",
                newName: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Reports",
                newName: "Status");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLog_UserId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditLogs",
                table: "AuditLogs",
                column: "AuditLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AspNetUsers_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_AspNetUsers_UserId",
                table: "AuditLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditLogs",
                table: "AuditLogs");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                newName: "AuditLog");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Reports",
                newName: "status");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLog",
                newName: "IX_AuditLog_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditLog",
                table: "AuditLog",
                column: "AuditLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLog_AspNetUsers_UserId",
                table: "AuditLog",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
