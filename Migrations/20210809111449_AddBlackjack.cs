using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Abyss.Migrations
{
    public partial class AddBlackjack : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlackjackGames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: false),
                    PlayerInitialBet = table.Column<decimal>(type: "numeric", nullable: false),
                    PlayerFinalBet = table.Column<decimal>(type: "numeric", nullable: false),
                    PlayerBalanceBeforeGame = table.Column<decimal>(type: "numeric", nullable: false),
                    PlayerBalanceAfterGame = table.Column<decimal>(type: "numeric", nullable: false),
                    PlayerCards = table.Column<string>(type: "text", nullable: true),
                    DidPlayerDoubleDown = table.Column<bool>(type: "boolean", nullable: false),
                    DealerCards = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackjackGames", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlackjackGames");
        }
    }
}
