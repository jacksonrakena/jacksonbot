using System;
using Jacksonbot.Persistence.Document;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Abyss.Migrations
{
    public partial class DropGuildIdFromReminders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bj_games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DateGameFinish = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: false),
                    PlayerInitialBet = table.Column<decimal>(type: "numeric", nullable: false),
                    PlayerFinalBet = table.Column<decimal>(type: "numeric", nullable: false),
                    Adjustment = table.Column<decimal>(type: "numeric", nullable: false),
                    PlayerBalanceBeforeGame = table.Column<decimal>(type: "numeric", nullable: false),
                    PlayerBalanceAfterGame = table.Column<decimal>(type: "numeric", nullable: false),
                    PlayerCards = table.Column<string>(type: "text", nullable: false),
                    DidPlayerDoubleDown = table.Column<bool>(type: "boolean", nullable: false),
                    DealerCards = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bj_games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuildConfigurations",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Data = table.Column<GuildConfig>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatorId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsCurrencyCreated = table.Column<bool>(type: "boolean", nullable: false),
                    IsCurrencyDestroyed = table.Column<bool>(type: "boolean", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    PayerId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    PayerBalanceBeforeTransaction = table.Column<decimal>(type: "numeric", nullable: false),
                    PayerBalanceAfterTransaction = table.Column<decimal>(type: "numeric", nullable: false),
                    PayeeId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    PayeeBalanceBeforeTransaction = table.Column<decimal>(type: "numeric", nullable: false),
                    PayeeBalanceAfterTransaction = table.Column<decimal>(type: "numeric", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trivia_records",
                columns: table => new
                {
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "integer", nullable: false),
                    IncorrectAnswers = table.Column<int>(type: "integer", nullable: false),
                    TotalMatches = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trivia_records", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Coins = table.Column<decimal>(type: "numeric", nullable: false),
                    ColorR = table.Column<int>(type: "integer", nullable: false),
                    ColorG = table.Column<int>(type: "integer", nullable: false),
                    ColorB = table.Column<int>(type: "integer", nullable: false),
                    BadgesString = table.Column<string>(type: "text", nullable: false),
                    FirstInteraction = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LatestInteraction = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trivia_category_votes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TriviaRecordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CategoryId = table.Column<string>(type: "text", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    TimesPicked = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trivia_category_votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trivia_category_votes_trivia_records_TriviaRecordId",
                        column: x => x.TriviaRecordId,
                        principalTable: "trivia_records",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trivia_category_votes_TriviaRecordId",
                table: "trivia_category_votes",
                column: "TriviaRecordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bj_games");

            migrationBuilder.DropTable(
                name: "GuildConfigurations");

            migrationBuilder.DropTable(
                name: "reminders");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "trivia_category_votes");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "trivia_records");
        }
    }
}
