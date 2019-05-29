namespace Wbooru.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DownloadRecords",
                c => new
                    {
                        DownloadRecordID = c.Int(nullable: false, identity: true),
                        GalleryID = c.String(),
                        GalleryName = c.String(),
                        DownloadFileName = c.String(),
                        DownloadTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.DownloadRecordID);
            
            CreateTable(
                "dbo.GalleryItemMarks",
                c => new
                    {
                        GalleryItemMarkID = c.Int(nullable: false, identity: true),
                        GalleryName = c.String(),
                        Time = c.DateTime(nullable: false),
                        MarkGalleryID = c.String(),
                    })
                .PrimaryKey(t => t.GalleryItemMarkID);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        TagID = c.Int(nullable: false, identity: true),
                        TagName = c.String(),
                        AddTime = c.DateTime(nullable: false),
                        FromGallery = c.String(),
                    })
                .PrimaryKey(t => t.TagID);
            
            CreateTable(
                "dbo.VisitRecords",
                c => new
                    {
                        VisitRecordID = c.Int(nullable: false, identity: true),
                        GalleryID = c.String(),
                        GalleryName = c.String(),
                        VisitFileName = c.String(),
                        LastVisitTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.VisitRecordID)
                .Index(t => t.VisitRecordID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.VisitRecords", new[] { "VisitRecordID" });
            DropTable("dbo.VisitRecords");
            DropTable("dbo.Tags");
            DropTable("dbo.GalleryItemMarks");
            DropTable("dbo.DownloadRecords");
        }
    }
}
