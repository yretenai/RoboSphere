using Microsoft.EntityFrameworkCore.Migrations;

namespace RoboSphere.Discord.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Settings", table => new
            {
                SettingId = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                Key = table.Column<string>("TEXT", maxLength: 128, nullable: false),
                Value = table.Column<string>("TEXT", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Settings", x => x.SettingId);
            });

            migrationBuilder.CreateTable("Stopwatch", table => new
            {
                StopwatchId = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                Key = table.Column<string>("TEXT", maxLength: 128, nullable: false),
                Next = table.Column<long>("INTEGER", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Stopwatch", x => x.StopwatchId);
            });

            migrationBuilder.CreateIndex("IX_Settings_Key", "Settings", "Key");

            migrationBuilder.CreateIndex("IX_Stopwatch_Key", "Stopwatch", "Key");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Settings");

            migrationBuilder.DropTable("Stopwatch");
        }
    }
}
