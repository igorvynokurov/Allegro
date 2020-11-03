using Microsoft.EntityFrameworkCore.Migrations;

namespace AllegroSearchService.Data.Migrations
{
    public partial class translated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Translated",
                table: "Translations",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Translated",
                table: "Translations");
        }
    }
}
