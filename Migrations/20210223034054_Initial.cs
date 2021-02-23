using System;
using Lament.Persistence.Document;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Lament.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildConfigurations",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Data = table.Column<GuildConfig>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildConfigurations", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatorId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildConfigurations");

            migrationBuilder.DropTable(
                name: "Reminders");
        }
    }
}
