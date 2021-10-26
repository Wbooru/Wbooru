using Microsoft.EntityFrameworkCore.Migrations;

namespace Wbooru.Migrations
{
    public partial class AddDetailLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DetailLink",
                table: "GalleryItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetailLink",
                table: "GalleryItems");
        }
    }
}
