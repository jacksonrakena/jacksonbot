using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Abyss.Migrations
{
    public partial class AddInteractionMonitoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FirstInteraction",
                table: "UserAccounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LatestInteraction",
                table: "UserAccounts",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstInteraction",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "LatestInteraction",
                table: "UserAccounts");
        }
    }
}
