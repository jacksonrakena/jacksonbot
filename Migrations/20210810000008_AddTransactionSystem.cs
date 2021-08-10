using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Abyss.Migrations
{
    public partial class AddTransactionSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCurrencyCreated = table.Column<bool>(type: "boolean", nullable: false),
                    IsCurrencyDestroyed = table.Column<bool>(type: "boolean", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transactions");
        }
    }
}
