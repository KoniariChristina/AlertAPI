using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlertAPI.Migrations
{
    public partial class initialmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "IPAddresses",
                columns: table => new
                {
                    IPString = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Blacklisted = table.Column<bool>(type: "bit", nullable: false),
                    sourceType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPAddresses", x => x.IPString);
                });

            migrationBuilder.CreateTable(
                name: "AlertIPAddresses",
                columns: table => new
                {
                    AlertID = table.Column<int>(type: "int", nullable: false),
                    IPString = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertIPAddresses", x => new { x.AlertID, x.IPString });
                    table.ForeignKey(
                        name: "FK_AlertIPAddresses_Alerts_AlertID",
                        column: x => x.AlertID,
                        principalTable: "Alerts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlertIPAddresses_IPAddresses_IPString",
                        column: x => x.IPString,
                        principalTable: "IPAddresses",
                        principalColumn: "IPString",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertIPAddresses_IPString",
                table: "AlertIPAddresses",
                column: "IPString");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertIPAddresses");

            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "IPAddresses");
        }
    }
}
