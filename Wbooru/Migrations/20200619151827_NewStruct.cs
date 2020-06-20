using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Wbooru.Migrations
{
    public partial class NewStruct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GalleryItems",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PreviewImageSize_Width = table.Column<int>(nullable: true),
                    PreviewImageSize_Height = table.Column<int>(nullable: true),
                    PreviewImageDownloadLink = table.Column<string>(nullable: true),
                    DownloadFileName = table.Column<string>(nullable: true),
                    GalleryName = table.Column<string>(nullable: true),
                    GalleryItemID = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryItems", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    TagID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tag_Name = table.Column<string>(nullable: true),
                    Tag_Type = table.Column<int>(nullable: true),
                    AddTime = table.Column<DateTime>(nullable: false),
                    FromGallery = table.Column<string>(nullable: true),
                    RecordType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.TagID);
                });

            migrationBuilder.CreateTable(
                name: "Downloads",
                columns: table => new
                {
                    DownloadId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TotalBytes = table.Column<long>(nullable: false),
                    DownloadStartTime = table.Column<DateTime>(nullable: false),
                    DownloadUrl = table.Column<string>(nullable: true),
                    GalleryItemID = table.Column<int>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    DownloadFullPath = table.Column<string>(nullable: true),
                    DisplayDownloadedLength = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Downloads", x => x.DownloadId);
                    table.ForeignKey(
                        name: "FK_Downloads_GalleryItems_GalleryItemID",
                        column: x => x.GalleryItemID,
                        principalTable: "GalleryItems",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemMarks",
                columns: table => new
                {
                    GalleryItemMarkID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Time = table.Column<DateTime>(nullable: false),
                    GalleryItemID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemMarks", x => x.GalleryItemMarkID);
                    table.ForeignKey(
                        name: "FK_ItemMarks_GalleryItems_GalleryItemID",
                        column: x => x.GalleryItemID,
                        principalTable: "GalleryItems",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VisitRecords",
                columns: table => new
                {
                    VisitRecordID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GalleryItemID = table.Column<int>(nullable: true),
                    LastVisitTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitRecords", x => x.VisitRecordID);
                    table.ForeignKey(
                        name: "FK_VisitRecords_GalleryItems_GalleryItemID",
                        column: x => x.GalleryItemID,
                        principalTable: "GalleryItems",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Downloads_GalleryItemID",
                table: "Downloads",
                column: "GalleryItemID");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMarks_GalleryItemID",
                table: "ItemMarks",
                column: "GalleryItemID");

            migrationBuilder.CreateIndex(
                name: "IX_VisitRecords_GalleryItemID",
                table: "VisitRecords",
                column: "GalleryItemID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Downloads");

            migrationBuilder.DropTable(
                name: "ItemMarks");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "VisitRecords");

            migrationBuilder.DropTable(
                name: "GalleryItems");
        }
    }
}
