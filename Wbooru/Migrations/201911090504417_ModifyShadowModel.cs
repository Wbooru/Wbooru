namespace Wbooru.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyShadowModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Downloads",
                c => new
                    {
                        DownloadId = c.Int(nullable: false, identity: true),
                        TotalBytes = c.Long(nullable: false),
                        DownloadStartTime = c.DateTime(nullable: false),
                        GalleryPictureID = c.String(),
                        DownloadUrl = c.String(),
                        GalleryName = c.String(),
                        FileName = c.String(),
                        DownloadFullPath = c.String(),
                        DisplayDownloadedLength = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.DownloadId);
            
            CreateTable(
                "dbo.GalleryItemMarks",
                c => new
                    {
                        GalleryItemMarkID = c.Int(nullable: false, identity: true),
                        GalleryName = c.String(),
                        Time = c.DateTime(nullable: false),
                        MarkGalleryID = c.String(),
                        Item_ID = c.Int(),
                    })
                .PrimaryKey(t => t.GalleryItemMarkID)
                .ForeignKey("dbo.ShadowGalleryItems", t => t.Item_ID)
                .Index(t => t.Item_ID);
            
            CreateTable(
                "dbo.ShadowGalleryItems",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PreviewImageDownloadLink = c.String(),
                        DownloadFileName = c.String(),
                        GalleryItemID = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.TagRecords",
                c => new
                    {
                        TagID = c.Int(nullable: false, identity: true),
                        Tag_Name = c.String(),
                        Tag_Type = c.Int(nullable: false),
                        AddTime = c.DateTime(nullable: false),
                        FromGallery = c.String(),
                        IsFilter = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.TagID);
            
            CreateTable(
                "dbo.VisitRecords",
                c => new
                    {
                        VisitRecordID = c.Int(nullable: false, identity: true),
                        GalleryName = c.String(),
                        LastVisitTime = c.DateTime(nullable: false),
                        GalleryItem_ID = c.Int(),
                    })
                .PrimaryKey(t => t.VisitRecordID)
                .ForeignKey("dbo.ShadowGalleryItems", t => t.GalleryItem_ID)
                .Index(t => t.VisitRecordID)
                .Index(t => t.GalleryItem_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.VisitRecords", "GalleryItem_ID", "dbo.ShadowGalleryItems");
            DropForeignKey("dbo.GalleryItemMarks", "Item_ID", "dbo.ShadowGalleryItems");
            DropIndex("dbo.VisitRecords", new[] { "GalleryItem_ID" });
            DropIndex("dbo.VisitRecords", new[] { "VisitRecordID" });
            DropIndex("dbo.GalleryItemMarks", new[] { "Item_ID" });
            DropTable("dbo.VisitRecords");
            DropTable("dbo.TagRecords");
            DropTable("dbo.ShadowGalleryItems");
            DropTable("dbo.GalleryItemMarks");
            DropTable("dbo.Downloads");
        }
    }
}
