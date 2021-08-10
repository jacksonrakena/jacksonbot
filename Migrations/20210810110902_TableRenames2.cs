using Microsoft.EntityFrameworkCore.Migrations;

namespace Abyss.Migrations
{
    public partial class TableRenames2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TriviaCategoryVoteRecord_trivia_records_TriviaRecordId",
                table: "TriviaCategoryVoteRecord");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TriviaCategoryVoteRecord",
                table: "TriviaCategoryVoteRecord");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildConfigurations",
                table: "GuildConfigurations");

            migrationBuilder.RenameTable(
                name: "TriviaCategoryVoteRecord",
                newName: "trivia_category_votes");

            migrationBuilder.RenameTable(
                name: "GuildConfigurations",
                newName: "guilds");

            migrationBuilder.RenameIndex(
                name: "IX_TriviaCategoryVoteRecord_TriviaRecordId",
                table: "trivia_category_votes",
                newName: "IX_trivia_category_votes_TriviaRecordId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_trivia_category_votes",
                table: "trivia_category_votes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_guilds",
                table: "guilds",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_trivia_category_votes_trivia_records_TriviaRecordId",
                table: "trivia_category_votes",
                column: "TriviaRecordId",
                principalTable: "trivia_records",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trivia_category_votes_trivia_records_TriviaRecordId",
                table: "trivia_category_votes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_trivia_category_votes",
                table: "trivia_category_votes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_guilds",
                table: "guilds");

            migrationBuilder.RenameTable(
                name: "trivia_category_votes",
                newName: "TriviaCategoryVoteRecord");

            migrationBuilder.RenameTable(
                name: "guilds",
                newName: "GuildConfigurations");

            migrationBuilder.RenameIndex(
                name: "IX_trivia_category_votes_TriviaRecordId",
                table: "TriviaCategoryVoteRecord",
                newName: "IX_TriviaCategoryVoteRecord_TriviaRecordId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TriviaCategoryVoteRecord",
                table: "TriviaCategoryVoteRecord",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildConfigurations",
                table: "GuildConfigurations",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_TriviaCategoryVoteRecord_trivia_records_TriviaRecordId",
                table: "TriviaCategoryVoteRecord",
                column: "TriviaRecordId",
                principalTable: "trivia_records",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
