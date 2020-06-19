using Microsoft.EntityFrameworkCore.Migrations;

namespace Wbooru.Migrations
{
    public partial class AddSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreviewImageSize_Height",
                table: "GalleryItems",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviewImageSize_Width",
                table: "GalleryItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewImageSize_Height",
                table: "GalleryItems");

            migrationBuilder.DropColumn(
                name: "PreviewImageSize_Width",
                table: "GalleryItems");
        }
    }
}
