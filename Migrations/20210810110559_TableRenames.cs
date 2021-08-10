using Microsoft.EntityFrameworkCore.Migrations;

namespace Abyss.Migrations
{
    public partial class TableRenames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TriviaCategoryVoteRecord_TriviaRecords_TriviaRecordId",
                table: "TriviaCategoryVoteRecord");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reminders",
                table: "Reminders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAccounts",
                table: "UserAccounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TriviaRecords",
                table: "TriviaRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlackjackGames",
                table: "BlackjackGames");

            migrationBuilder.RenameTable(
                name: "Reminders",
                newName: "reminders");

            migrationBuilder.RenameTable(
                name: "UserAccounts",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "TriviaRecords",
                newName: "trivia_records");

            migrationBuilder.RenameTable(
                name: "BlackjackGames",
                newName: "bj_games");

            migrationBuilder.AddPrimaryKey(
                name: "PK_reminders",
                table: "reminders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_trivia_records",
                table: "trivia_records",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bj_games",
                table: "bj_games",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TriviaCategoryVoteRecord_trivia_records_TriviaRecordId",
                table: "TriviaCategoryVoteRecord",
                column: "TriviaRecordId",
                principalTable: "trivia_records",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TriviaCategoryVoteRecord_trivia_records_TriviaRecordId",
                table: "TriviaCategoryVoteRecord");

            migrationBuilder.DropPrimaryKey(
                name: "PK_reminders",
                table: "reminders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_trivia_records",
                table: "trivia_records");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bj_games",
                table: "bj_games");

            migrationBuilder.RenameTable(
                name: "reminders",
                newName: "Reminders");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "UserAccounts");

            migrationBuilder.RenameTable(
                name: "trivia_records",
                newName: "TriviaRecords");

            migrationBuilder.RenameTable(
                name: "bj_games",
                newName: "BlackjackGames");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reminders",
                table: "Reminders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAccounts",
                table: "UserAccounts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TriviaRecords",
                table: "TriviaRecords",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlackjackGames",
                table: "BlackjackGames",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TriviaCategoryVoteRecord_TriviaRecords_TriviaRecordId",
                table: "TriviaCategoryVoteRecord",
                column: "TriviaRecordId",
                principalTable: "TriviaRecords",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
