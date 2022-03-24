using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abyss.Migrations
{
    public partial class JsonRebuild : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "guilds",
                newName: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "guilds",
                newName: "GuildId");
        }
    }
}
