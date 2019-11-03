namespace Wbooru.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyDownload : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Downloads",
                c => new
                    {
                        DownloadId = c.Int(nullable: false, identity: true),
                        DownloadStartTime = c.DateTime(nullable: false),
                        GalleryPictureID = c.Int(nullable: false),
                        DownloadUrl = c.String(),
                        GalleryName = c.String(),
                        FileName = c.String(),
                        DownloadFullPath = c.String(),
                    })
                .PrimaryKey(t => t.DownloadId);
            
            DropTable("dbo.DownloadRecords");
        }
        
        public override void Down()
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
            
            DropTable("dbo.Downloads");
        }
    }
}
