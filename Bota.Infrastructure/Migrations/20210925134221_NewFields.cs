using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bota.Infrastructure.Migrations
{
    public partial class NewFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageType", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MessageText = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MessageTypeId = table.Column<int>(type: "int", nullable: true),
                    BotConfigId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Message_BotConfigs_BotConfigId",
                        column: x => x.BotConfigId,
                        principalTable: "BotConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Message_MessageType_MessageTypeId",
                        column: x => x.MessageTypeId,
                        principalTable: "MessageType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "BotConfigs",
                columns: new[] { "Id", "Prefix", "SteamApiKey" },
                values: new object[] { 889924246820749312ul, "/", "00AF2C79D6E3395D77541A5E0C5377E4" });

            migrationBuilder.CreateIndex(
                name: "IX_Message_BotConfigId",
                table: "Message",
                column: "BotConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_MessageTypeId",
                table: "Message",
                column: "MessageTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "MessageType");

            migrationBuilder.DeleteData(
                table: "BotConfigs",
                keyColumn: "Id",
                keyValue: 889924246820749312ul);
        }
    }
}
