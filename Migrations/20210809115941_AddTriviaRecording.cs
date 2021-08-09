using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Abyss.Migrations
{
    public partial class AddTriviaRecording : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TriviaRecords",
                columns: table => new
                {
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "integer", nullable: false),
                    IncorrectAnswers = table.Column<int>(type: "integer", nullable: false),
                    TotalMatches = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriviaRecords", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "TriviaCategoryVoteRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TriviaRecordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CategoryId = table.Column<string>(type: "text", nullable: true),
                    CategoryName = table.Column<string>(type: "text", nullable: true),
                    TimesPicked = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriviaCategoryVoteRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TriviaCategoryVoteRecord_TriviaRecords_TriviaRecordId",
                        column: x => x.TriviaRecordId,
                        principalTable: "TriviaRecords",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TriviaCategoryVoteRecord_TriviaRecordId",
                table: "TriviaCategoryVoteRecord",
                column: "TriviaRecordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TriviaCategoryVoteRecord");

            migrationBuilder.DropTable(
                name: "TriviaRecords");
        }
    }
}
